using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string playerName = "Joueur";

    [SyncVar(hook = nameof(OnReadyStateChanged))]
    private bool isReady = false;

    public string PlayerName => playerName;
    public bool IsReady => isReady;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.RegisterPlayer(this);
        }

        if (isLocalPlayer)
        {
            CmdSetPlayerName($"Joueur_{Random.Range(1000, 9999)}");
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.UnregisterPlayer(this);
        }
    }

    [Command]
    public void CmdSetPlayerName(string newName)
    {
        playerName = newName;
    }

    [Command]
    public void CmdSetReady(bool ready)
    {
        isReady = ready;
    }

    private void OnPlayerNameChanged(string oldName, string newName)
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.RefreshPlayerList();
        }
    }

    private void OnReadyStateChanged(bool oldState, bool newState)
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.RefreshPlayerList();
        }
    }
}