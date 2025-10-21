using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    [Header("Player List")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerItemPrefab;


    [Header("Buttons")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text readyButtonText;

    private List<GameObject> playerListItems = new List<GameObject>();
    // private bool isReady = false; // Supprimé, on se base sur le réseau
    private LobbyPlayer localPlayer;

    private void Start()
    {
        if (readyButton != null)
            readyButton.onClick.AddListener(OnReadyButtonClicked);

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
            startGameButton.gameObject.SetActive(Mirror.NetworkServer.active);
        }

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);

        Invoke(nameof(FindLocalPlayer), 0.5f);
    }

    private void FindLocalPlayer()
    {
        foreach (var player in FindObjectsOfType<LobbyPlayer>())
        {
            if (player.isLocalPlayer)
            {
                localPlayer = player;
                break;
            }
        }
    }

    public void UpdatePlayerList(List<LobbyPlayer> players)
    {
        foreach (var item in playerListItems)
        {
            Destroy(item);
        }
        playerListItems.Clear();

        foreach (var player in players)
        {
            GameObject item = Instantiate(playerItemPrefab, playerListContainer);
            
            TMP_Text nameText = item.transform.Find("PlayerName")?.GetComponent<TMP_Text>();
            TMP_Text statusText = item.transform.Find("PlayerStatus")?.GetComponent<TMP_Text>();

            if (nameText != null)
                nameText.text = player.PlayerName;

            if (statusText != null)
            {
                statusText.text = player.IsReady ? "✓ Prêt" : "En attente...";
                statusText.color = player.IsReady ? Color.green : Color.yellow;
            }

            playerListItems.Add(item);
        }

        // Met à jour le texte du bouton prêt selon l'état réseau réel du joueur local
        if (localPlayer != null && readyButtonText != null)
        {
            readyButtonText.text = localPlayer.IsReady ? "Annuler" : "Prêt";
        }
    }

    private void OnReadyButtonClicked()
    {
        if (localPlayer == null)
        {
            Debug.LogWarning("[LobbyUI] Joueur local introuvable");
            return;
        }

        bool newReady = !localPlayer.IsReady;
        localPlayer.CmdSetReady(newReady);


        Debug.Log($"[LobbyUI] État prêt: {newReady}");
    }

    private void OnStartGameClicked()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.StartGame();
        }
    }

    private void OnBackButtonClicked()
    {
        if (Mirror.NetworkServer.active && Mirror.NetworkClient.isConnected)
        {
            Mirror.NetworkManager.singleton.StopHost();
        }
        else if (Mirror.NetworkClient.isConnected)
        {
            Mirror.NetworkManager.singleton.StopClient();
        }
        else if (Mirror.NetworkServer.active)
        {
            Mirror.NetworkManager.singleton.StopServer();
        }

        SceneManager.LoadScene("GameSelectionMenu");
    }

    private void OnDestroy()
    {
        if (readyButton != null)
            readyButton.onClick.RemoveListener(OnReadyButtonClicked);

        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnStartGameClicked);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);
    }
}