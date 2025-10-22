using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// Si tu utilises TextMeshPro, décommente :
// using TMPro;

public class GameSelectionMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;
    [SerializeField] private InputField ipInputField; // ou TMP_InputField si TMPro
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private Text connectionStatusText; // ou TMP_Text si TMPro

    [Header("Settings")]
    [SerializeField] private string defaultIP = "localhost";

    private CustomNetworkRoomManager roomManager;
    // Flags pour savoir si on démarre une connexion (utilisés par l'UI)
    private bool isStartingHost = false;
    private bool isStartingClient = false;

    private void Start()
    {
        Debug.Log("[GameSelectionMenu] Start() appelé");
        
        // Récupère le NetworkRoomManager
        roomManager = CustomNetworkRoomManager.Instance;
        
        if (roomManager == null)
        {
            Debug.LogError("[GameSelectionMenu] NetworkRoomManager introuvable!");
            return;
        }
        
        Debug.Log($"[GameSelectionMenu] NetworkRoomManager trouvé: {roomManager.name}");

        // Configure les boutons
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(OnHostButtonClicked);
            Debug.Log("[GameSelectionMenu] Host button configuré");
        }
        else
        {
            Debug.LogError("[GameSelectionMenu] Host button est NULL!");
        }

        if (joinButton != null)
        {
            joinButton.onClick.AddListener(OnJoinButtonClicked);
            Debug.Log("[GameSelectionMenu] Join button configuré");
        }
        else
        {
            Debug.LogWarning("[GameSelectionMenu] Join button est NULL!");
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            Debug.Log("[GameSelectionMenu] Back button configuré");
        }
        else
        {
            Debug.LogWarning("[GameSelectionMenu] Back button est NULL!");
        }

        // Configure l'input field avec l'IP par défaut
        if (ipInputField != null)
        {
            string savedIP = PlayerPrefs.GetString("LastUsedIP", defaultIP);
            ipInputField.text = savedIP;
        }

        // Cache le panel de connexion
        if (connectionPanel != null)
            connectionPanel.SetActive(false);

        Debug.Log("[GameSelectionMenu] Initialisé complètement");
    }

    /// <summary>
    /// Crée une partie (Host)
    /// </summary>
    private void OnHostButtonClicked()
    {
        Debug.Log("[GameSelectionMenu] ========== HOST BUTTON CLICKED! ==========");

        if (roomManager == null)
        {
            Debug.LogError("[GameSelectionMenu] NetworkRoomManager introuvable");
            return;
        }

        Debug.Log("[GameSelectionMenu] Création d'une partie (Host)...");
        ShowConnectionPanel("Création de la partie...");

        isStartingHost = true;

        // Démarre le host directement et laisse Mirror charger la RoomScene et spawn les RoomPlayer
        roomManager.StartHost();
    }

    // ...coroutine LoadSceneAndStartHost supprimée...

    /// <summary>
    /// Rejoint une partie (Client)
    /// </summary>
    private void OnJoinButtonClicked()
    {
        Debug.Log("[GameSelectionMenu] ========== JOIN BUTTON CLICKED! ==========");
        
        if (roomManager == null)
        {
            Debug.LogError("[GameSelectionMenu] NetworkRoomManager introuvable");
            return;
        }

        string ip = ipInputField != null ? ipInputField.text : defaultIP;

        if (string.IsNullOrWhiteSpace(ip))
        {
            Debug.LogWarning("[GameSelectionMenu] IP vide, utilisation de l'IP par défaut");
            ip = defaultIP;
        }

        Debug.Log($"[GameSelectionMenu] Tentative de connexion à: {ip}");
        ShowConnectionPanel($"Connexion à {ip}...");

        // Sauvegarde l'IP
        PlayerPrefs.SetString("LastUsedIP", ip);
        PlayerPrefs.Save();

        // Configure l'adresse
        roomManager.networkAddress = ip;

        // Marque qu'on démarre en mode client
        isStartingClient = true;

        Debug.Log($"[GameSelectionMenu] Chargement de la scène: {roomManager.RoomScene}");
        // Démarre le client
        roomManager.StartClient();
        
    // Mirror gère la connexion client, pas besoin de coroutine ici
    }

    // Note: Mirror/NetworkRoomManager handles scene loading and spawning for host, so we start host directly.

    // ...coroutine LoadSceneAndStartClient supprimée...

    /// <summary>
    /// Retour au menu principal
    /// </summary>
    private void OnBackButtonClicked()
    {
        Debug.Log("[GameSelectionMenu] Retour au menu principal");
        
        // Arrête toute connexion en cours
        if (NetworkClient.isConnected)
        {
            roomManager?.StopClient();
        }
        if (NetworkServer.active)
        {
            roomManager?.StopHost();
        }

        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Affiche le panel de connexion
    /// </summary>
    private void ShowConnectionPanel(string message)
    {
        if (connectionPanel != null)
        {
            connectionPanel.SetActive(true);
            
            if (connectionStatusText != null)
                connectionStatusText.text = message;
        }

        SetButtonsInteractable(false);
    }

    /// <summary>
    /// Cache le panel de connexion
    /// </summary>
    private void HideConnectionPanel()
    {
        if (connectionPanel != null)
            connectionPanel.SetActive(false);

        SetButtonsInteractable(true);
    }

    /// <summary>
    /// Active/désactive les boutons
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        if (hostButton != null)
            hostButton.interactable = interactable;
        if (joinButton != null)
            joinButton.interactable = interactable;
        if (backButton != null)
            backButton.interactable = interactable;
        if (ipInputField != null)
            ipInputField.interactable = interactable;
    }

    private void OnDestroy()
    {
        // Nettoie les listeners
        if (hostButton != null)
            hostButton.onClick.RemoveListener(OnHostButtonClicked);
        if (joinButton != null)
            joinButton.onClick.RemoveListener(OnJoinButtonClicked);
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);
    }
} 