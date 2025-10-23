using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
        if (readyButton != null) readyButton.onClick.AddListener(OnReadyButtonClicked);
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
            startGameButton.gameObject.SetActive(false);
        }
        if (backButton != null) backButton.onClick.AddListener(OnBackButtonClicked);
        InvokeRepeating(nameof(RefreshPlayerList), 0.5f, 0.5f);
    }

    private void OnEnable()
    {
        Debug.Log("[LobbyUI] OnEnable - Attente des joueurs...");
    }

    // Rafraîchit la liste des joueurs
    public void RefreshPlayerList()
    {
        // Récupère le room manager
        CustomNetworkRoomManager roomManager = CustomNetworkRoomManager.Instance;
        if (roomManager == null)
        {
            return;
        }

        Debug.Log($"[LobbyUI] RefreshPlayerList - roomSlots count: {roomManager.roomSlots.Count}");
        if (localRoomPlayer == null) TryFindLocalPlayer();

        // Supprime les anciens éléments UI
        foreach (var item in playerListItems) if (item != null) Destroy(item);
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
            CreatePlayerListItem(roomPlayer);
        }

        // Met à jour le texte de statut
        UpdateStatusText(totalPlayers, readyCount, roomManager.maxConnections);

        // Met à jour le bouton Ready
        UpdateReadyButton();

        // Met à jour la visibilité du bouton Start
        UpdateStartButton(roomManager.allPlayersReady && totalPlayers >= roomManager.minPlayers);
    }

    // Tente de localiser le joueur local (hôte ou client)
    private void TryFindLocalPlayer()
    {
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
                    Debug.Log($"[LobbyUI] Joueur local trouvé via roomSlots: {rp.PlayerName}");
                    return;
                }
            }
        }

        // recherche dans la scène
        CustomRoomPlayer[] allPlayers = FindObjectsOfType<CustomRoomPlayer>();
        foreach (var player in allPlayers)
        {
            if (player.isLocalPlayer)
            {
                localRoomPlayer = player;
                Debug.Log($"[LobbyUI] Joueur local trouvé: {player.PlayerName}");
                return;
            }
        }

        if (localRoomPlayer == null)
        {
            Debug.Log("[LobbyUI] Joueur local non trouvé, lancement d'une tentative réessayée...");
            StartCoroutine(RetryFindLocalPlayer(10, 0.25f));
        }
    }

    private IEnumerator RetryFindLocalPlayer(int attempts, float delaySeconds)
    {
        for (int i = 0; i < attempts; i++)
        {
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
                        Debug.Log($"[LobbyUI] Joueur local trouvé via retry (roomSlots): {rp.PlayerName}");
                        yield break;
                    }
                }
            }

            CustomRoomPlayer[] allPlayers = FindObjectsOfType<CustomRoomPlayer>();
            foreach (var p in allPlayers)
            {
                if (p.isLocalPlayer)
                {
                    localRoomPlayer = p;
                    Debug.Log($"[LobbyUI] Joueur local trouvé via retry (FindObjects): {p.PlayerName}");
                    yield break;
                }
            }

            yield return new WaitForSeconds(delaySeconds);
        }

        Debug.LogWarning("[LobbyUI] Echec: impossible de trouver le joueur local après plusieurs tentatives.");
    }

    // Crée un élément d'UI pour un joueur
    private void CreatePlayerListItem(CustomRoomPlayer player)
    {
        if (playerItemPrefab == null || playerListContainer == null) return;

        GameObject item = Instantiate(playerItemPrefab, playerListContainer);

        var nameTransform = item.transform.Find("PlayerName");
        var statusTransform = item.transform.Find("PlayerStatus");
        GameObject hostIcon = item.transform.Find("HostIcon")?.gameObject;

        TMPro.TMP_Text tmpName = null, tmpStatus = null;
        Text uiName = null, uiStatus = null;

        if (nameTransform != null)
        {
            uiName = nameTransform.GetComponent<Text>();
            tmpName = nameTransform.GetComponent<TMPro.TMP_Text>();
            if (tmpName != null) tmpName.text = player.PlayerName + (player.isLocalPlayer ? " (Vous)" : "");
            else if (uiName != null)
            {
                uiName.text = player.PlayerName + (player.isLocalPlayer ? " (Vous)" : "");
                if (player.isLocalPlayer) uiName.color = Color.cyan;
            }
            else Debug.LogWarning("[LobbyUI] PlayerName: pas de composant Text/TMP");
        }
        else Debug.LogWarning("[LobbyUI] PlayerName introuvable dans le prefab");

        if (statusTransform != null)
        {
            uiStatus = statusTransform.GetComponent<Text>();
            tmpStatus = statusTransform.GetComponent<TMPro.TMP_Text>();
            string statusString = player.readyToBegin ? "✓ Prêt" : "En attente...";
            Color statusColor = player.readyToBegin ? Color.green : Color.yellow;
            if (tmpStatus != null) { tmpStatus.text = statusString; tmpStatus.color = statusColor; }
            else if (uiStatus != null) { uiStatus.text = statusString; uiStatus.color = statusColor; }
            else Debug.LogWarning("[LobbyUI] PlayerStatus: pas de composant Text/TMP");
        }
        else Debug.LogWarning("[LobbyUI] PlayerStatus introuvable dans le prefab");

        if (hostIcon != null) hostIcon.SetActive(player.index == 0);

        playerListItems.Add(item);
    }

    // Met à jour le texte de statut général
    private void UpdateStatusText(int current, int ready, int max)
    {
        if (statusText != null) statusText.text = $"{current}/{max} joueurs - {ready} prêt(s)";
    }

    // Met à jour le bouton Ready
    private void UpdateReadyButton()
    {
        if (localRoomPlayer == null)
        {
            if (readyButton != null) readyButton.interactable = false;
            return;
        }

        if (readyButtonText != null) readyButtonText.text = localRoomPlayer.readyToBegin ? "Annuler" : "Prêt";
        if (readyButton != null) readyButton.interactable = true;
    }

    // Met à jour le bouton Start (visible seulement pour l'hôte)
    private void UpdateStartButton(bool canStart)
    {
        if (startGameButton == null) return;
        bool isHost = NetworkServer.active;
        startGameButton.gameObject.SetActive(isHost);
        if (isHost) startGameButton.interactable = canStart;
    }

    #region Button Callbacks

    private void OnReadyButtonClicked()
    {
        if (localRoomPlayer == null) { Debug.LogWarning("[LobbyUI] Joueur local introuvable"); return; }
        localRoomPlayer.ToggleReady();
        Debug.Log("[LobbyUI] Toggle ready");
    }

    private void OnStartGameClicked()
    {
        CustomNetworkRoomManager roomManager = CustomNetworkRoomManager.Instance;
        if (roomManager != null) roomManager.StartGameFromLobby();
        else Debug.LogError("[LobbyUI] NetworkRoomManager introuvable");
    }

    private void OnBackButtonClicked()
    {
        CustomNetworkRoomManager roomManager = CustomNetworkRoomManager.Instance;
        if (roomManager != null) roomManager.ReturnToGameSelection();
        else
        {
            if (NetworkServer.active && NetworkClient.isConnected) NetworkManager.singleton.StopHost();
            else if (NetworkClient.isConnected) NetworkManager.singleton.StopClient();
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