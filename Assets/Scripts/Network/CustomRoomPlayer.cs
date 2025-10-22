using Mirror;
using UnityEngine;

/// <summary>
/// Joueur dans le lobby (avant le jeu)
/// Hérite de NetworkRoomPlayer pour bénéficier du système de "ready" intégré
/// </summary>
public class CustomRoomPlayer : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string playerName = "Joueur";

    public string PlayerName => playerName;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Debug.Log($"[RoomPlayer] Client démarré - IsLocal: {isLocalPlayer}, Nom: {playerName}");

        // Si c'est notre joueur local, on définit un nom aléatoire
        if (isLocalPlayer)
        {
            CmdSetPlayerName($"Joueur_{Random.Range(1000, 9999)}");
        }

        // Rafraîchir l'UI
        RefreshLobbyUI();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"[RoomPlayer] Serveur démarré pour {playerName}");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log($"[RoomPlayer] Client arrêté pour {playerName}");
        
        // Rafraîchir l'UI
        RefreshLobbyUI();
    }

    /// <summary>
    /// Change le nom du joueur (appelé par le client)
    /// </summary>
    [Command]
    public void CmdSetPlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning("[RoomPlayer] Tentative de définir un nom vide");
            return;
        }

        playerName = newName;
        Debug.Log($"[RoomPlayer] Nom changé: {newName}");
    }

    /// <summary>
    /// Appelé quand le nom change (hook)
    /// </summary>
    private void OnPlayerNameChanged(string oldName, string newName)
    {
        Debug.Log($"[RoomPlayer] Hook nom: {oldName} → {newName}");
        RefreshLobbyUI();
    }

    /// <summary>
    /// Override pour personnaliser le comportement du bouton ready
    /// </summary>
    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        base.ReadyStateChanged(oldReadyState, newReadyState);
        
        Debug.Log($"[RoomPlayer] {playerName} est maintenant: {(newReadyState ? "PRÊT" : "EN ATTENTE")}");
        
        // Rafraîchir l'UI
        RefreshLobbyUI();
    }

    /// <summary>
    /// Rafraîchit l'UI du lobby
    /// </summary>
    private void RefreshLobbyUI()
    {
        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.RefreshPlayerList();
        }
    }

    /// <summary>
    /// Toggle l'état ready (appelé par l'UI)
    /// </summary>
    public void ToggleReady()
    {
        if (!isLocalPlayer)
        {
            Debug.LogWarning("[RoomPlayer] Tentative de changer ready pour un joueur non-local");
            return;
        }

        // Utilise la méthode de base de NetworkRoomPlayer
        CmdChangeReadyState(!readyToBegin);
    }

    /// <summary>
    /// Retourne les informations du joueur pour l'UI
    /// </summary>
    public (string name, bool isReady, bool isLocal) GetPlayerInfo()
    {
        return (playerName, readyToBegin, isLocalPlayer);
    }
}