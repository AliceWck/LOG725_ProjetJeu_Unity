using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float gameTime = 300f;
    
    public enum GameState {Loading, Playing, GameOver}
    public GameOverUI gameOverUI; // Assign in Inspector

    public GameState CurrentState { get; private set; } = GameState.Playing;
    private List<ShadowPlayer> players = new();

    private void Awake()
    {
        players.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<ShadowPlayer>());
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            gameTime -= Time.deltaTime;
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
        
        if(!alive && !escaped)
            EndGameShadowsWin(false);
        if(!alive && escaped)
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

        // Show message
        if (gameOverUI != null)
        {
            gameOverUI.gameObject.SetActive(true);
            string message = shadowsWin ? "☾ Shadows Win!" : "☀ Seekers Win!";
            gameOverUI.ShowMessage(message);
        }

        Debug.Log($"Game Over! {(shadowsWin ? "Shadows" : "Seekers")} win!");
    }
}
