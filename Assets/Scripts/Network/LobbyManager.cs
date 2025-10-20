using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private LobbyUI lobbyUI;

    [Header("Game Settings")]
    [SerializeField] private int minPlayers = 2;
    [SerializeField] private int maxPlayers = 5;
    [SerializeField] private string gameSceneName = "GameScene";

    private List<LobbyPlayer> connectedPlayers = new List<LobbyPlayer>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterPlayer(LobbyPlayer player)
    {
        if (!connectedPlayers.Contains(player))
        {
            connectedPlayers.Add(player);
            RefreshPlayerList();
            Debug.Log($"[LobbyManager] Joueur enregistré: {player.PlayerName}. Total: {connectedPlayers.Count}/{maxPlayers}");
        }
    }

    public void UnregisterPlayer(LobbyPlayer player)
    {
        if (connectedPlayers.Contains(player))
        {
            connectedPlayers.Remove(player);
            RefreshPlayerList();
            Debug.Log($"[LobbyManager] Joueur retiré. Total: {connectedPlayers.Count}/{maxPlayers}");
        }
    }

    public void RefreshPlayerList()
    {
        if (lobbyUI != null)
        {
            lobbyUI.UpdatePlayerList(connectedPlayers);
        }
    }

    public bool CanStartGame()
    {
        if (connectedPlayers.Count < minPlayers)
        {
            Debug.LogWarning($"[LobbyManager] Pas assez de joueurs ({connectedPlayers.Count}/{minPlayers})");
            return false;
        }

        foreach (var player in connectedPlayers)
        {
            if (!player.IsReady)
            {
                Debug.LogWarning("[LobbyManager] Tous les joueurs ne sont pas prêts");
                return false;
            }
        }

        return true;
    }

    public void StartGame()
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("[LobbyManager] Seul l'hôte peut démarrer la partie");
            return;
        }

        if (!CanStartGame())
        {
            Debug.LogWarning("[LobbyManager] Impossible de démarrer la partie");
            return;
        }

        Debug.Log("[LobbyManager] Démarrage de la partie...");
        NetworkManager.singleton.ServerChangeScene(gameSceneName);
    }
}