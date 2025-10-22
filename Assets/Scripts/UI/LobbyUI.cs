using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Si tu utilises TextMeshPro, décommente cette ligne et change Text par TMP_Text partout :
// using TMPro;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [Header("Player List")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerItemPrefab;

    [Header("Buttons")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;

    [Header("Texts")]
    [SerializeField] private Text readyButtonText;
    [SerializeField] private Text statusText;
    
    private List<GameObject> playerListItems = new List<GameObject>();
    private CustomRoomPlayer localRoomPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        Debug.Log("[LobbyUI] Awake - Instance créée");
    }

    private void Start()
    {
        Debug.Log("[LobbyUI] Start");
        
        // Configure les boutons
        if (readyButton != null)
            readyButton.onClick.AddListener(OnReadyButtonClicked);

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
            startGameButton.gameObject.SetActive(false);
        }

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);

        // Rafraîchit périodiquement (pour mettre à jour l'UI)
        InvokeRepeating(nameof(RefreshPlayerList), 0.5f, 0.5f);
    }

    private void OnEnable()
    {
        Debug.Log("[LobbyUI] OnEnable - Attente des joueurs...");
    }

    /// <summary>
    /// Rafraîchit la liste complète des joueurs
    /// Appelé automatiquement toutes les 0.5 secondes
    /// </summary>
    public void RefreshPlayerList()
    {
        // Récupère le room manager
        CustomNetworkRoomManager roomManager = CustomNetworkRoomManager.Instance;
        if (roomManager == null)
        {
            return; // Pas encore initialisé
        }

        Debug.Log($"[LobbyUI] RefreshPlayerList - roomSlots count: {roomManager.roomSlots.Count}");

        // Si on n'a pas encore de joueur local, essaye de le trouver
        if (localRoomPlayer == null)
        {
            TryFindLocalPlayer();
        }

        // Nettoie les anciens items
        foreach (var item in playerListItems)
        {
            if (item != null)
                Destroy(item);
        }
        playerListItems.Clear();

        int readyCount = 0;
        int totalPlayers = 0;

        // Parcourt les slots du room manager
        foreach (var slot in roomManager.roomSlots)
        {
            if (slot == null) continue;

            CustomRoomPlayer roomPlayer = slot as CustomRoomPlayer;
            if (roomPlayer == null) continue;

            totalPlayers++;
            if (roomPlayer.readyToBegin) readyCount++;

            // Crée un item d'UI pour ce joueur
            CreatePlayerListItem(roomPlayer);
        }

        // Met à jour le texte de statut
        UpdateStatusText(totalPlayers, readyCount, roomManager.maxConnections);

        // Met à jour le bouton Ready
        UpdateReadyButton();

        // Met à jour la visibilité du bouton Start
        UpdateStartButton(roomManager.allPlayersReady && totalPlayers >= roomManager.minPlayers);
    }

    /// <summary>
    /// Essaye de trouver le joueur local (appelé automatiquement)
    /// </summary>
    private void TryFindLocalPlayer()
    {
        // First try to find the local player among the room manager's slots (works in host mode too)
        CustomNetworkRoomManager roomManager = CustomNetworkRoomManager.Instance;
        if (roomManager != null)
        {
            foreach (var slot in roomManager.roomSlots)
            {
                if (slot == null) continue;
                CustomRoomPlayer rp = slot as CustomRoomPlayer;
                if (rp != null && rp.isLocalPlayer)
                {
                    localRoomPlayer = rp;
                    Debug.Log($"[LobbyUI] ✓ Joueur local trouvé via roomSlots: {rp.PlayerName}");
                    return;
                }
            }
        }

        // Fallback: search scene objects (older approach)
        CustomRoomPlayer[] allPlayers = FindObjectsOfType<CustomRoomPlayer>();
        foreach (var player in allPlayers)
        {
            if (player.isLocalPlayer)
            {
                localRoomPlayer = player;
                Debug.Log($"[LobbyUI] ✓ Joueur local trouvé: {player.PlayerName}");
                return;
            }
        }

        // If still not found, start a retry coroutine (useful in host mode where timing varies)
        if (localRoomPlayer == null)
        {
            Debug.Log("[LobbyUI] Joueur local non trouvé immédiatement, démarrage d'une tentative réessayée...");
            StartCoroutine(RetryFindLocalPlayer(10, 0.25f));
        }
    }

    private IEnumerator RetryFindLocalPlayer(int attempts, float delaySeconds)
    {
        for (int i = 0; i < attempts; i++)
        {
            // try via roomSlots first
            CustomNetworkRoomManager roomManager = CustomNetworkRoomManager.Instance;
            if (roomManager != null)
            {
                foreach (var slot in roomManager.roomSlots)
                {
                    if (slot == null) continue;
                    CustomRoomPlayer rp = slot as CustomRoomPlayer;
                    if (rp != null && rp.isLocalPlayer)
                    {
                        localRoomPlayer = rp;
                        Debug.Log($"[LobbyUI] ✓ Joueur local trouvé via retry (roomSlots): {rp.PlayerName}");
                        yield break;
                    }
                }
            }

            // fallback search in scene
            CustomRoomPlayer[] allPlayers = FindObjectsOfType<CustomRoomPlayer>();
            foreach (var p in allPlayers)
            {
                if (p.isLocalPlayer)
                {
                    localRoomPlayer = p;
                    Debug.Log($"[LobbyUI] ✓ Joueur local trouvé via retry (FindObjects): {p.PlayerName}");
                    yield break;
                }
            }

            yield return new WaitForSeconds(delaySeconds);
        }

        Debug.LogWarning("[LobbyUI] Echec: impossible de trouver le joueur local après plusieurs tentatives.");
    }

    /// <summary>
    /// Crée un élément d'UI pour un joueur
    /// </summary>
    private void CreatePlayerListItem(CustomRoomPlayer player)
    {
        if (playerItemPrefab == null || playerListContainer == null) return;

        GameObject item = Instantiate(playerItemPrefab, playerListContainer);

        // Nom du joueur (supporte TextMeshPro ou UnityEngine.UI.Text)
        var nameTransform = item.transform.Find("PlayerName");
    if (nameTransform != null)
        {
            Text uiName = nameTransform.GetComponent<Text>();
            TMPro.TMP_Text tmpName = nameTransform.GetComponent<TMP_Text>();

            if (uiName != null)
            {
                uiName.text = player.PlayerName;
                if (player.isLocalPlayer)
                {
                    uiName.text += " (Vous)";
                    uiName.color = Color.cyan;
                }
            }
            else if (tmpName != null)
            {
                tmpName.text = player.PlayerName + (player.isLocalPlayer ? " (Vous)" : "");
                tmpName.color = player.isLocalPlayer ? Color.cyan : Color.white;
            }
        }
        else
        {
            Debug.LogWarning("[LobbyUI] PlayerName transform not found on player item prefab. Vérifiez le nom de l'enfant (PlayerName).");
        }

        // Statut (Prêt / En attente) - supporte TMP ou UI.Text
        var statusTransform = item.transform.Find("PlayerStatus");
    if (statusTransform != null)
        {
            Text uiStatus = statusTransform.GetComponent<Text>();
            TMPro.TMP_Text tmpStatus = statusTransform.GetComponent<TMP_Text>();

            string statusString = player.readyToBegin ? "✓ Prêt" : "En attente...";
            Color statusColor = player.readyToBegin ? Color.green : Color.yellow;

            if (uiStatus != null)
            {
                uiStatus.text = statusString;
                uiStatus.color = statusColor;
            }
            else if (tmpStatus != null)
            {
                tmpStatus.text = statusString;
                tmpStatus.color = statusColor;
            }
        }
        else
        {
            Debug.LogWarning("[LobbyUI] PlayerStatus transform not found on player item prefab. Vérifiez le nom de l'enfant (PlayerStatus).");
        }

        // Icône hôte (optionnel)
        GameObject hostIcon = item.transform.Find("HostIcon")?.gameObject;
        if (hostIcon != null)
        {
            hostIcon.SetActive(player.index == 0);
        }

        playerListItems.Add(item);
    }

    /// <summary>
    /// Met à jour le texte de statut général
    /// </summary>
    private void UpdateStatusText(int current, int ready, int max)
    {
        if (statusText != null)
        {
            statusText.text = $"{current}/{max} joueurs - {ready} prêt(s)";
        }
    }

    /// <summary>
    /// Met à jour le bouton Ready
    /// </summary>
    private void UpdateReadyButton()
    {
        if (localRoomPlayer == null)
        {
            if (readyButton != null)
                readyButton.interactable = false;
            return;
        }

        if (readyButtonText != null)
            readyButtonText.text = localRoomPlayer.readyToBegin ? "Annuler" : "Prêt";
        
        if (readyButton != null)
            readyButton.interactable = true;
    }

    /// <summary>
    /// Met à jour le bouton Start (visible seulement pour l'hôte)
    /// </summary>
    private void UpdateStartButton(bool canStart)
    {
        if (startGameButton == null) return;

        bool isHost = NetworkServer.active;
        startGameButton.gameObject.SetActive(isHost);
        
        if (isHost)
        {
            startGameButton.interactable = canStart;
        }
    }

    #region Button Callbacks

    private void OnReadyButtonClicked()
    {
        if (localRoomPlayer == null)
        {
            Debug.LogWarning("[LobbyUI] Joueur local introuvable");
            return;
        }

        localRoomPlayer.ToggleReady();
        Debug.Log($"[LobbyUI] Toggle ready");
    }

    private void OnStartGameClicked()
    {
        CustomNetworkRoomManager roomManager = CustomNetworkRoomManager.Instance;
        if (roomManager != null)
        {
            roomManager.StartGameFromLobby();
        }
        else
        {
            Debug.LogError("[LobbyUI] NetworkRoomManager introuvable");
        }
    }

    private void OnBackButtonClicked()
    {
        CustomNetworkRoomManager roomManager = CustomNetworkRoomManager.Instance;
        if (roomManager != null)
        {
            roomManager.ReturnToGameSelection();
        }
        else
        {
            // Fallback
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }
            
            SceneManager.LoadScene("GameSelectionMenu");
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Nettoie les listeners
        if (readyButton != null)
            readyButton.onClick.RemoveListener(OnReadyButtonClicked);
        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnStartGameClicked);
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);

        // Annule les invokes
        CancelInvoke();

        // Nettoie le singleton
        if (Instance == this)
            Instance = null;
    }
}