using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class CustomNetworkRoomManager : NetworkRoomManager
{
    [Header("Configuration Initiale")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool autoLoadMainMenu = false;

    // Propriétés publiques pour diagnostic
    public string MainMenuSceneName => mainMenuSceneName;
    public bool AutoLoadMainMenu => autoLoadMainMenu;

    [Header("Prefabs de Rôles")]
    [SerializeField] private GameObject gardienPrefab;
    [SerializeField] private GameObject ombrePrefab;

    public static CustomNetworkRoomManager Instance { get; private set; }

    public override void Awake()
    {
        ConfigureDefaultSettings();
        base.Awake();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void ConfigureDefaultSettings()
    {
        if (maxConnections == 0)
            maxConnections = 5;

        if (minPlayers == 0)
            minPlayers = 2;

        if (string.IsNullOrEmpty(RoomScene))
            RoomScene = "Lobby";

        if (string.IsNullOrEmpty(GameplayScene))
            GameplayScene = "OutdoorsScene";

        if (RoomScene.Contains("/") || RoomScene.Contains(".unity"))
        {
            RoomScene = System.IO.Path.GetFileNameWithoutExtension(RoomScene);
        }

        if (GameplayScene.Contains("/") || GameplayScene.Contains(".unity"))
        {
            GameplayScene = System.IO.Path.GetFileNameWithoutExtension(GameplayScene);
        }

        autoCreatePlayer = true;
        showRoomGUI = false;

        if (string.IsNullOrWhiteSpace(onlineScene))
        {
            onlineScene = RoomScene;
        }

        if (roomPlayerPrefab != null && !spawnPrefabs.Contains(roomPlayerPrefab.gameObject))
        {
            spawnPrefabs.Add(roomPlayerPrefab.gameObject);
        }

        if (gardienPrefab != null && !spawnPrefabs.Contains(gardienPrefab))
        {
            spawnPrefabs.Add(gardienPrefab);
        }

        if (ombrePrefab != null && !spawnPrefabs.Contains(ombrePrefab))
        {
            spawnPrefabs.Add(ombrePrefab);
        }

        if (playerPrefab == null && gardienPrefab != null)
        {
            playerPrefab = gardienPrefab;
        }
    }

    public override void Start()
    {
        base.Start();

        string currentScene = SceneManager.GetActiveScene().name;

        if (autoLoadMainMenu && currentScene == "NetworkSetup")
        {
            LoadMainMenu();
        }
    }

    private void LoadMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("[NetworkRoomManager] Le nom de la scène du menu principal est vide!");
            return;
        }

        try
        {
            bool sceneExists = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneName == mainMenuSceneName)
                {
                    sceneExists = true;
                    break;
                }
            }

            if (!sceneExists)
            {
                Debug.LogError($"[NetworkRoomManager] Scène '{mainMenuSceneName}' NON trouvée dans Build Settings!");
                return;
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NetworkRoomManager] Erreur lors du chargement de {mainMenuSceneName}: {e.Message}");
            Debug.LogError("[NetworkRoomManager] Vérifiez que la scène est bien dans Build Settings!");
        }
    }

    #region Server Callbacks

    public override void OnRoomServerPlayersReady()
    {

    }

    public override void OnRoomServerConnect(NetworkConnectionToClient conn)
    {
        base.OnRoomServerConnect(conn);
        Debug.Log($"[NetworkRoomManager] Joueur {conn.connectionId} connecté au lobby");
    }

    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnRoomServerDisconnect(conn);
        Debug.Log($"[NetworkRoomManager] Joueur {conn.connectionId} déconnecté du lobby");
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        base.OnRoomServerSceneChanged(sceneName);

        if (sceneName == GameplayScene)
        {
            Debug.Log("[NetworkRoomManager] ✓ Transition vers la partie !");
        }
        else if (sceneName == RoomScene)
        {
            Debug.Log("[NetworkRoomManager] ✓ Retour au lobby !");
        }
    }

    public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnRoomServerAddPlayer(conn);
        Debug.Log($"[NetworkRoomManager] Joueur ajouté: {conn.identity?.netId}");
        // Rafraîchir l'UI du lobby si possible
        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.RefreshPlayerList();
        }
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        CustomRoomPlayer lobbyPlayer = roomPlayer.GetComponent<CustomRoomPlayer>();

        GameObject gamePlayer;

        // Choisir le prefab selon le rôle
        if (lobbyPlayer != null && lobbyPlayer.PlayerRole == Role.Gardien)
        {
            if (gardienPrefab == null)
            {
                Debug.LogError("[NetworkRoomManager] GardienPrefab manquant !");
                return null;
            }
            gamePlayer = gardienPrefab;
        }
        else
        {
            if (ombrePrefab == null)
            {
                Debug.LogError("[NetworkRoomManager] OmbrePrefab manquant !");
                return null;
            }
            gamePlayer = ombrePrefab;
        }

        // Obtenir une position de spawn
        Transform spawnPoint = GetStartPosition();

        GameObject playerInstance;
        if (spawnPoint != null)
        {
            // Spawn avec position et rotation du spawn point
            playerInstance = Instantiate(gamePlayer, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            // Fallback: spawn avec un offset basé sur le nombre de joueurs
            // Disposer les joueurs en cercle pour éviter les collisions
            int playerIndex = roomSlots.Count - 1; // Index commence à 0
            float angle = playerIndex * 60f; // 60 degrés entre chaque joueur (max 6 joueurs)
            float radius = 5f; // Rayon du cercle en mètres
            float spawnHeight = 1f; // Hauteur au-dessus du sol

            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                spawnHeight,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );

            playerInstance = Instantiate(gamePlayer, offset, Quaternion.identity);
        }

        // Spawner sur le réseau
        NetworkServer.Spawn(playerInstance, conn);

        if (lobbyPlayer != null && playerInstance != null)
        {
            GamePlayer gamePlayerComponent = playerInstance.GetComponent<GamePlayer>();
            if (gamePlayerComponent != null)
            {
                gamePlayerComponent.SetPlayerName(lobbyPlayer.PlayerName);
                gamePlayerComponent.SetPlayerRole(lobbyPlayer.PlayerRole);
            }
        }
        else
        {
            Debug.LogWarning($"[NetworkRoomManager] ⚠ Problème lors de la création du joueur de jeu");
        }

        // Retourne l'objet joueur de jeu
        return playerInstance;
    }

    /// <summary>
    /// Démarre la partie depuis l'UI du lobby (appelé par LobbyUI)
    /// </summary>
    public void StartGameFromLobby()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("[NetworkRoomManager] StartGameFromLobby appelé mais le serveur n'est pas actif");
            return;
        }

        if (allPlayersReady && roomSlots.Count >= minPlayers)
        {
            Debug.Log("[NetworkRoomManager] Démarrage manuel de la partie...");
            ServerChangeScene(GameplayScene);
        }
        else
        {
            Debug.LogWarning($"[NetworkRoomManager] Impossible de démarrer : {roomSlots.Count}/{minPlayers} joueurs, prêts: {allPlayersReady}");
        }
    }

    public void ReturnToLobby()
    {
        if (NetworkServer.active)
        {
            Debug.Log("[NetworkRoomManager] Retour au lobby...");
            ServerChangeScene(RoomScene);
        }
        else
        {
            Debug.LogWarning("[NetworkRoomManager] Impossible de retourner au lobby : pas serveur");
        }
    }

    public void ReturnToGameSelection()
    {
        Debug.Log("[NetworkRoomManager] Retour au menu de sélection...");

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            StopClient();
        }
        else if (NetworkServer.active)
        {
            StopServer();
        }

        SceneManager.LoadScene("GameSelectionMenu");
    }

    public int GetConnectedPlayersCount()
    {
        return roomSlots.Count;
    }

    public int GetReadyPlayersCount()
    {
        int count = 0;
        foreach (var slot in roomSlots)
        {
            NetworkRoomPlayer roomPlayer = slot as NetworkRoomPlayer;
            if (roomPlayer != null && roomPlayer.readyToBegin)
                count++;
        }
        return count;
    }

    public enum RoleAssignmentMode
    {
        Random,
        HostIsGardien,
        HostIsOmbre
    }

    [Header("Assignation du rôle au démarrage")]
    [SerializeField] public RoleAssignmentMode roleAssignmentMode = RoleAssignmentMode.HostIsGardien;

    #endregion

    #region Validation

    protected new void OnValidate()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("[NetworkRoomManager] ⚠ Main Menu Scene Name non défini !");
        }

        if (string.IsNullOrEmpty(RoomScene))
        {
            Debug.LogWarning("[NetworkRoomManager] ⚠ Room Scene non définie !");
        }

        if (string.IsNullOrEmpty(GameplayScene))
        {
            Debug.LogWarning("[NetworkRoomManager] ⚠ Gameplay Scene non définie !");
        }

        if (roomPlayerPrefab == null)
        {
            Debug.LogWarning("[NetworkRoomManager] ⚠ Room Player Prefab non assigné !");
        }

        if (gardienPrefab == null)
        {
            Debug.LogWarning("[NetworkRoomManager] ⚠ Gardien Prefab non assigné ! Vérifiez les prefabs de rôles.");
        }

        if (ombrePrefab == null)
        {
            Debug.LogWarning("[NetworkRoomManager] ⚠ Ombre Prefab non assigné ! Vérifiez les prefabs de rôles.");
        }

        if (minPlayers <= 0)
        {
            Debug.LogWarning("[NetworkRoomManager] ⚠ Min Players doit être > 0 !");
        }

        if (maxConnections <= 0)
        {
            Debug.LogWarning("[NetworkRoomManager] ⚠ Max Connections doit être > 0 !");
        }
    }

    #endregion

    #region Debug Helpers

    [ContextMenu("Force Load Main Menu")]
    private void ForceLoadMainMenu()
    {
        LoadMainMenu();
    }

    [ContextMenu("Show Current Configuration")]
    private void ShowConfiguration()
    {
        Debug.Log("=== CONFIGURATION ===");
        Debug.Log($"Main Menu Scene: {mainMenuSceneName}");
        Debug.Log($"Room Scene: {RoomScene}");
        Debug.Log($"Gameplay Scene: {GameplayScene}");
        Debug.Log($"Min Players: {minPlayers}");
        Debug.Log($"Max Connections: {maxConnections}");
        Debug.Log($"Auto Load Main Menu: {autoLoadMainMenu}");
        Debug.Log($"Room Player Prefab: {roomPlayerPrefab?.name ?? "NULL"}");
        Debug.Log($"Gardien Prefab: {gardienPrefab?.name ?? "NULL"}");
        Debug.Log($"Ombre Prefab: {ombrePrefab?.name ?? "NULL"}");
        Debug.Log($"Current Scene: {SceneManager.GetActiveScene().name}");
    }

    #endregion
}