using Mirror;
using UnityEngine;

/// <summary>
/// SCRIPT OBSOLÈTE - Utilisez NetworkMovementAuthority à la place
/// Ce script cause des conflits avec les autres gestionnaires d'autorité.
/// Gardé uniquement pour compatibilité, mais les méthodes sont désactivées.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class NetworkInputControl : NetworkBehaviour
{
    [Header("⚠️ SCRIPT OBSOLÈTE")]
    [SerializeField] private bool showDebugLogs = false;

    private MonoBehaviour[] allControllers;

    private void Awake()
    {
        // NE RIEN FAIRE - Script obsolète
        Debug.LogWarning($"[NetworkInputControl] ⚠️ Ce script est obsolète ! Utilisez NetworkMovementAuthority sur {gameObject.name}");
    }

    // NE PLUS UTILISER Update() - Cause des conflits avec NetworkMovementAuthority
    // private void Update() { }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        // NE RIEN FAIRE - NetworkMovementAuthority gère déjà l'autorité
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
        // NE RIEN FAIRE - NetworkMovementAuthority gère déjà l'autorité
    }
}
