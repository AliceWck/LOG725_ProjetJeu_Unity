using Mirror;
using UnityEngine;

/// <summary>
/// Script de diagnostic pour tracer TOUS les callbacks réseau
/// À attacher temporairement au prefab Gardien pour diagnostiquer
/// </summary>
public class NetworkCallbackDebugger : NetworkBehaviour
{
    [Header("Identification")]
    [SerializeField] private string prefabName = "Inconnu";

    private void Awake()
    {
        Debug.Log($"[{prefabName}] ===== AWAKE =====");
    }

    private void Start()
    {
        Debug.Log($"[{prefabName}] ===== START ===== IsServer: {isServer}, IsClient: {isClient}, IsLocalPlayer: {isLocalPlayer}, IsOwned: {isOwned}");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"[{prefabName}] ✓✓✓ OnStartServer ✓✓✓");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[{prefabName}] ✓✓✓ OnStartClient ✓✓✓ IsLocalPlayer: {isLocalPlayer}, IsOwned: {isOwned}");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"[{prefabName}] ✓✓✓ OnStartLocalPlayer ✓✓✓");
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log($"[{prefabName}] ✓✓✓ OnStartAuthority ✓✓✓ IsLocalPlayer: {isLocalPlayer}");
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
        Debug.Log($"[{prefabName}] ⚠️ OnStopAuthority ⚠️");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log($"[{prefabName}] ⚠️ OnStopClient ⚠️");
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log($"[{prefabName}] ⚠️ OnStopServer ⚠️");
    }

    private void OnDestroy()
    {
        Debug.Log($"[{prefabName}] ===== DESTROY =====");
    }
}
