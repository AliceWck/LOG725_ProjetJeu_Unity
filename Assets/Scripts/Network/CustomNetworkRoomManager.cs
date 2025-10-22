using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomNetworkRoomManager : NetworkRoomManager
{
    [Header("Configuration Initiale")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    // By default do NOT auto-load the Main Menu to avoid interfering with programmatic host/client startup.
    // You can re-enable this behavior in the inspector if you prefer automatic navigation from NetworkSetup -> MainMenu.
    [SerializeField] private bool autoLoadMainMenu = false;

    public static CustomNetworkRoomManager Instance { get; private set; }

    public override void Awake()
    {
        base.Awake();
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Configure les paramètres par défaut
        ConfigureDefaultSettings();
    }

    private void ConfigureDefaultSettings()
    {
        if (maxConnections == 0)
            maxConnections = 5;
        
        if (minPlayers == 0)
            minPlayers = 2;
        
        // IMPORTANT : Utilise juste le NOM de la scène, pas le chemin complet!
        if (string.IsNullOrEmpty(RoomScene))
            RoomScene = "Lobby";
        
        if (string.IsNullOrEmpty(GameplayScene))
            GameplayScene = "OutdoorsScene";
        
        // Corrige si le chemin complet a été mis par erreur
        if (RoomScene.Contains("/") || RoomScene.Contains(".unity"))
        {
            Debug.LogWarning($"[NetworkRoomManager] RoomScene contient un chemin! Correction: {RoomScene}");
            RoomScene = System.IO.Path.GetFileNameWithoutExtension(RoomScene);
            Debug.LogWarning($"[NetworkRoomManager] RoomScene corrigé: {RoomScene}");
        }
        
        if (GameplayScene.Contains("/") || GameplayScene.Contains(".unity"))
        {
            Debug.LogWarning($"[NetworkRoomManager] GameplayScene contient un chemin! Correction: {GameplayScene}");
            GameplayScene = System.IO.Path.GetFileNameWithoutExtension(GameplayScene);
            Debug.LogWarning($"[NetworkRoomManager] GameplayScene corrigé: {GameplayScene}");
        }
        
        autoCreatePlayer = true;
        showRoomGUI = false;

        // Ensure NetworkManager's onlineScene is set so StartHost will load the room scene
        // (NetworkManager uses onlineScene when starting a host to load scene on server before connecting local client)
        if (string.IsNullOrWhiteSpace(onlineScene))
        {
            onlineScene = RoomScene;
            Debug.Log($"[NetworkRoomManager] onlineScene non défini, réglage automatique vers: {onlineScene}");
        }

        // Ajoute le prefab RoomPlayer dans les spawnables si besoin
        if (roomPlayerPrefab != null && !spawnPrefabs.Contains(roomPlayerPrefab.gameObject))
        {
            spawnPrefabs.Add(roomPlayerPrefab.gameObject);
            Debug.Log("[NetworkRoomManager] RoomPlayerPrefab ajouté à spawnPrefabs");
        }
        
        Debug.Log($"[NetworkRoomManager] Configuration par défaut appliquée");
    }

    public override void Start()
    {
        base.Start();
        
        Debug.Log($"[NetworkRoomManager] Initialisé - Lobby: {RoomScene}, Jeu: {GameplayScene}");
        Debug.Log($"[NetworkRoomManager] Min Players: {minPlayers}, Max: {maxConnections}");
        
        // Charge automatiquement le menu principal si on est dans NetworkSetup
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[NetworkRoomManager] Scène actuelle: {currentScene}");
        
        if (autoLoadMainMenu && currentScene == "NetworkSetup")
        {
            Debug.Log($"[NetworkRoomManager] Chargement automatique de {mainMenuSceneName}...");
            Invoke(nameof(LoadMainMenu), 0.2f);
        }
    }

    private void LoadMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("[NetworkRoomManager] Le nom de la scène du menu principal est vide!");
            return;
        }

        Debug.Log($"[NetworkRoomManager] >> Chargement de {mainMenuSceneName}");
        
        try
        {
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
        if (roomSlots.Count < minPlayers)
        {
            Debug.LogWarning($"[NetworkRoomManager] Pas assez de joueurs ({roomSlots.Count}/{minPlayers})");
            return;
        }

        if (allPlayersReady)
        {
            Debug.Log("[NetworkRoomManager] Tous les joueurs sont prêts ! Démarrage de la partie...");
            base.OnRoomServerPlayersReady();
        }
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
        
        GameObject gamePlayer = base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
        
        if (lobbyPlayer != null && gamePlayer != null)
        {
            GamePlayer gamePlayerComponent = gamePlayer.GetComponent<GamePlayer>();
            if (gamePlayerComponent != null)
            {
                gamePlayerComponent.SetPlayerName(lobbyPlayer.PlayerName);
                Debug.Log($"[NetworkRoomManager] ✓ Joueur de jeu créé: {lobbyPlayer.PlayerName}");
            }
        }
        else
        {
            Debug.LogWarning($"[NetworkRoomManager] ⚠ Problème lors de la création du joueur de jeu");
        }
        
        // Retourne l'objet joueur de jeu (assure que toutes les routes retournent quelque chose)
        return gamePlayer;
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

        Debug.Log("[NetworkRoomManager] StartGameFromLobby appelé - tentative de démarrage de la partie");

        // Reutilise la logique existante qui vérifie le nombre de joueurs et si tous sont prêts
        OnRoomServerPlayersReady();
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

        if (playerPrefab == null)
        {
            Debug.LogWarning("[NetworkRoomManager] ⚠ Game Player Prefab (playerPrefab) non assigné !");
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
        Debug.Log($"Current Scene: {SceneManager.GetActiveScene().name}");
    }

    #endregion
}