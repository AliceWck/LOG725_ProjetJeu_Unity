using Mirror;
using UnityEngine;

public class CustomRoomPlayer : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string playerName = "Joueur";

    [SyncVar(hook = nameof(OnRoleChanged))]
    private Role role = Role.Ombre;

    public string PlayerName => playerName;
    public Role PlayerRole => role;

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

        if (isServer)
        {
            CustomNetworkRoomManager manager = CustomNetworkRoomManager.Instance;
            if (manager != null)
            {
                // Vérifier si ce joueur est le Host (connectionToClient == null signifie que c'est le serveur local)
                bool isHost = connectionToClient == null || connectionToClient.connectionId == 0;

                Debug.Log($"[RoomPlayer] isHost: {isHost}, connectionId: {(connectionToClient != null ? connectionToClient.connectionId.ToString() : "NULL")}, netId: {netId}");

                int gardienCount = 0;

                foreach (var slot in manager.roomSlots)
                {
                    if (slot != null && slot != this) // Ne pas compter ce joueur
                    {
                        CustomRoomPlayer roomPlayer = slot as CustomRoomPlayer;
                        if (roomPlayer != null && roomPlayer.PlayerRole == Role.Gardien)
                        {
                            gardienCount++;
                        }
                    }
                }

                // Assigner le rôle selon le mode configuré
                Role roleToSet = Role.Ombre;

                Debug.Log($"[RoomPlayer] Gardiens existants: {gardienCount}, Mode: {manager.roleAssignmentMode}");

                if (manager.roleAssignmentMode == CustomNetworkRoomManager.RoleAssignmentMode.HostIsGardien)
                {
                    // Host = Gardien, tous les autres = Ombre
                    if (isHost)
                    {
                        roleToSet = Role.Gardien;
                        Debug.Log($"[RoomPlayer] Mode HostIsGardien - Host → Gardien");
                    }
                    else
                    {
                        roleToSet = Role.Ombre;
                        Debug.Log($"[RoomPlayer] Mode HostIsGardien - Client → Ombre");
                    }
                }
                else if (manager.roleAssignmentMode == CustomNetworkRoomManager.RoleAssignmentMode.HostIsOmbre)
                {
                    // Host = Ombre, premier client qui rejoint = Gardien, les autres = Ombre
                    if (isHost)
                    {
                        // Host = Ombre
                        roleToSet = Role.Ombre;
                        Debug.Log($"[RoomPlayer] Mode HostIsOmbre - Host → Ombre");
                    }
                    else if (gardienCount == 0)
                    {
                        // Premier client = Gardien
                        roleToSet = Role.Gardien;
                        Debug.Log($"[RoomPlayer] Mode HostIsOmbre - Premier client → Gardien");
                    }
                    else
                    {
                        // Autres clients = Ombre
                        roleToSet = Role.Ombre;
                        Debug.Log($"[RoomPlayer] Mode HostIsOmbre - Autres clients → Ombre");
                    }
                }

                SetPlayerRole(roleToSet);
                Debug.Log($"[RoomPlayer] ✓ Rôle final assigné: {roleToSet}");
            }
        }
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

    /// Change le rôle du joueur (appelé par le serveur)
    [Server]
    public void SetPlayerRole(Role newRole)
    {
        role = newRole;
        Debug.Log($"[RoomPlayer] Rôle défini: {newRole}");
    }

    /// Hook appelé quand le nom change
    private void OnPlayerNameChanged(string oldName, string newName)
    {
        Debug.Log($"[RoomPlayer] Hook nom: {oldName} → {newName}");
        RefreshLobbyUI();
    }

    /// Hook appelé quand le rôle change
    private void OnRoleChanged(Role oldRole, Role newRole)
    {
        Debug.Log($"[RoomPlayer] Hook rôle: {oldRole} → {newRole}");
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
    public (string name, bool isReady, bool isLocal, Role role) GetPlayerInfo()
    {
        return (playerName, readyToBegin, isLocalPlayer, role);
    }
}