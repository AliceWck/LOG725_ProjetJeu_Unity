using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float maxGameTime = 300f; // durée totale fixe
    private float gameTime;

    public enum GameState { Loading, Playing, GameOver }
    public GameOverUI gameOverUI; // Assign in Inspector

    public GameState CurrentState { get; private set; } = GameState.Playing;
    private List<ShadowPlayer> players = new();
    private List<KeySpawnLocation> keySpawnLocations = new();

    public GameObject keyPrefab;

    private void Start()
    {
        gameTime = maxGameTime; // initialisation au début de la partie
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);


        // CHECK ICI VIEILLE VERSION
        //players.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<ShadowPlayer>());
        //keySpawnLocations.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<KeySpawnLocation>());

        //if (keySpawnLocations.Count < players.Count) throw new Exception("Not enough spawn locations");

        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        //for (int i = 0; i < players.Count + 1; i++)
        //{
        //    int choice = Random.Range(0, keySpawnLocations.Count);
        //    Instantiate(keyPrefab, keySpawnLocations[choice].transform.position, keySpawnLocations[choice].transform.rotation);
        //    keySpawnLocations.Remove(keySpawnLocations[choice]);
        //}
        // Debug : afficher combien de joueurs et de spawn locations

        Debug.Log($"Players: {players.Count}, Key spawn locations: {keySpawnLocations.Count}");

        // Vérifier si on a au moins 1 spawn location
        if (keySpawnLocations.Count == 0)
        {
            Debug.LogWarning("Aucune KeySpawnLocation trouvée ! Les clés ne seront pas spawnées.");
            return; // On arrête ici, sinon exception
        }

        // Nombre de clés à spawn = min(players + 1, nombre de spawn locations)
        int keysToSpawn = Mathf.Min(players.Count + 1, keySpawnLocations.Count);

        for (int i = 0; i < keysToSpawn; i++)
        {
            int choice = Random.Range(0, keySpawnLocations.Count);
            Instantiate(keyPrefab, keySpawnLocations[choice].transform.position, keySpawnLocations[choice].transform.rotation);
            keySpawnLocations.RemoveAt(choice); // retirer la location pour ne pas spawn dessus
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            gameTime -= Time.deltaTime;
            gameTime = Mathf.Max(gameTime, 0f); // clamp à 0

            float progress = (1f - (gameTime / maxGameTime)) * 100f;

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.SetGameProgress(progress);
            }
                
            if (gameTime <= 0) EndGameShadowsWin(false);
        }
    }

    public void UpdatePlayerStatus()
    {
        bool alive = false;
        bool escaped = false;

        foreach (var player in players)
        {
            if (player.playerStatus == PlayerStatus.Alive)
                alive = true;
            else if (player.playerStatus == PlayerStatus.Escaped)
                escaped = true;
        }

        if (!alive && !escaped)
            EndGameShadowsWin(false);
        if (!alive && escaped)
            EndGameShadowsWin(true);
    }


    private void EndGameShadowsWin(bool shadowsWin)
    {
        if (CurrentState == GameState.GameOver)
            return; // Prevent double triggers

        CurrentState = GameState.GameOver;

        // Stop all players
        foreach (var player in players)
        {
            player.enabled = false; // or disable their movement scripts only
            if (player.TryGetComponent(out Rigidbody rb))
                rb.velocity = Vector3.zero;
        }

        // Optionally freeze world time
        Time.timeScale = 0f;

        // Determine if local player won
        bool localPlayerWon = DetermineLocalPlayerWin(shadowsWin);

        PlayerPrefs.SetInt("GameOver_Win", localPlayerWon ? 1 : 0);
        PlayerPrefs.SetString("GameOver_Subtitle", shadowsWin ? "Les Ombres ont gagné !" : "Les Chercheurs ont gagné !");
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameOver");
    }

    private bool DetermineLocalPlayerWin(bool shadowsWin)
    {
        // Find all GamePlayers and get the local one
        GamePlayer[] gamePlayers = FindObjectsOfType<GamePlayer>();
        foreach (GamePlayer gp in gamePlayers)
        {
            if (gp.isLocalPlayer)
            {
                // If shadows win, and local player is Shadow, then won
                if (shadowsWin && gp.PlayerRole == Role.Ombre)
                    return true;
                // If seekers win, and local player is Gardien, then won
                if (!shadowsWin && gp.PlayerRole == Role.Gardien)
                    return true;
                return false;
            }
        }
        // If no local player found, assume lost
        return false;
    }
}
