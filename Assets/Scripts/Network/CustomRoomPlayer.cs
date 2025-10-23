using Mirror;
using UnityEngine;

public class CustomRoomPlayer : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string playerName = "Joueur";

    public string PlayerName => playerName;

    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log($"[RoomPlayer] Client démarré - IsLocal: {isLocalPlayer}, Nom: {playerName}");

        // Pour le joueur local, définir un nom aléatoire
        if (isLocalPlayer)
            CmdSetPlayerName($"Joueur_{Random.Range(1000, 9999)}");

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

    /// Change le nom du joueur (appelé par le client)
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

    /// Hook appelé quand le nom change
    private void OnPlayerNameChanged(string oldName, string newName)
    {
        Debug.Log($"[RoomPlayer] Hook nom: {oldName} → {newName}");
        RefreshLobbyUI();
    }

    /// Mise à jour du state "ready"
    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        base.ReadyStateChanged(oldReadyState, newReadyState);
        
        Debug.Log($"[RoomPlayer] {playerName} est maintenant: {(newReadyState ? "PRÊT" : "EN ATTENTE")}");
        
        // Rafraîchir l'UI
        RefreshLobbyUI();
    }

    /// Rafraîchit l'UI du lobby
    private void RefreshLobbyUI()
    {
        if (LobbyUI.Instance != null)
        {
            LobbyUI.Instance.RefreshPlayerList();
        }
    }

    /// Change l'état ready (depuis l'UI)
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

    /// Retourne les informations du joueur pour l'UI
    public (string name, bool isReady, bool isLocal) GetPlayerInfo()
    {
        return (playerName, readyToBegin, isLocalPlayer);
    }
}