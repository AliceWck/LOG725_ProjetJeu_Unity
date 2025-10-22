using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// Pour TextMeshPro, inclure "using TMPro;" si besoin

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
    // Flags d'état pour l'UI
    private bool isStartingHost = false;
    private bool isStartingClient = false;

    private void Start()
    {
        Debug.Log("[GameSelectionMenu] Start");

        roomManager = CustomNetworkRoomManager.Instance;
        if (roomManager == null)
        {
            Debug.LogError("[GameSelectionMenu] NetworkRoomManager introuvable");
            return;
        }

        // Configuration des boutons
        if (hostButton != null) hostButton.onClick.AddListener(OnHostButtonClicked);
        else Debug.LogError("[GameSelectionMenu] Host button est NULL");

        if (joinButton != null) joinButton.onClick.AddListener(OnJoinButtonClicked);
        if (backButton != null) backButton.onClick.AddListener(OnBackButtonClicked);

        // Restaure l'IP précédente
        if (ipInputField != null)
            ipInputField.text = PlayerPrefs.GetString("LastUsedIP", defaultIP);

        if (connectionPanel != null) connectionPanel.SetActive(false);
    }

    /// <summary>
    /// Crée une partie (Host)
    /// </summary>
    private void OnHostButtonClicked()
    {
        Debug.Log("[GameSelectionMenu] Création d'une partie (Host)");
        if (roomManager == null) { Debug.LogError("NetworkRoomManager introuvable"); return; }

        ShowConnectionPanel("Création de la partie...");
        isStartingHost = true;
        roomManager.StartHost();
    }

    // ...coroutine LoadSceneAndStartHost supprimée...

    /// <summary>
    /// Rejoint une partie (Client)
    /// </summary>
    private void OnJoinButtonClicked()
    {
        Debug.Log("[GameSelectionMenu] Rejoindre une partie");
        if (roomManager == null) { Debug.LogError("NetworkRoomManager introuvable"); return; }

        string ip = ipInputField != null ? ipInputField.text : defaultIP;
        if (string.IsNullOrWhiteSpace(ip)) ip = defaultIP;

        ShowConnectionPanel($"Connexion à {ip}...");
        PlayerPrefs.SetString("LastUsedIP", ip);
        PlayerPrefs.Save();

        roomManager.networkAddress = ip;
        isStartingClient = true;
        roomManager.StartClient();
    }

    /// <summary>
    /// Retour au menu principal
    /// </summary>
    private void OnBackButtonClicked()
    {
        Debug.Log("[GameSelectionMenu] Retour au menu principal");
        if (NetworkClient.isConnected) roomManager?.StopClient();
        if (NetworkServer.active) roomManager?.StopHost();
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Affiche le panel de connexion
    /// </summary>
    private void ShowConnectionPanel(string message)
    {
        if (connectionPanel != null) connectionPanel.SetActive(true);
        if (connectionStatusText != null) connectionStatusText.text = message;
        SetButtonsInteractable(false);
    }

    /// <summary>
    /// Cache le panel de connexion
    /// </summary>
    private void HideConnectionPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
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
        if (hostButton != null) hostButton.onClick.RemoveListener(OnHostButtonClicked);
        if (joinButton != null) joinButton.onClick.RemoveListener(OnJoinButtonClicked);
        if (backButton != null) backButton.onClick.RemoveListener(OnBackButtonClicked);
    }
} 