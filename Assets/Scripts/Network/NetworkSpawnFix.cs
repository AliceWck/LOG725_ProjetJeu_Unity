using Mirror;
using UnityEngine;
using System.Collections;

/// <summary>
/// Fix pour le problème du joueur qui spawn à l'horizontal
/// Désactive le CharacterController au spawn, puis le réactive après que NetworkTransform ait positionné le joueur
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class NetworkSpawnFix : NetworkBehaviour
{
    [Header("Références")]
    [SerializeField] private CharacterController characterController;

    [Header("Configuration")]
    [Tooltip("Délai avant de réactiver le CharacterController (en secondes)")]
    [SerializeField] private float reactivationDelay = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        // Trouver le CharacterController si non assigné
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        // Désactiver le CharacterController au spawn pour éviter les conflits avec NetworkTransform
        if (characterController != null)
        {
            characterController.enabled = false;
            if (showDebugLogs)
                Debug.Log($"[NetworkSpawnFix] CharacterController désactivé au spawn sur {gameObject.name}");
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (showDebugLogs)
            Debug.Log($"[NetworkSpawnFix] Client démarré sur {gameObject.name} - Attente avant réactivation...");

        // Attendre que NetworkTransform ait positionné le joueur
        StartCoroutine(ReactivateCharacterControllerDelayed());
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        // Si on a l'autorité, réactiver immédiatement (pas besoin d'attendre)
        if (characterController != null && !characterController.enabled)
        {
            characterController.enabled = true;
            if (showDebugLogs)
                Debug.Log($"[NetworkSpawnFix] ✓ CharacterController réactivé (autorité reçue) sur {gameObject.name}");
        }
    }

    private IEnumerator ReactivateCharacterControllerDelayed()
    {
        // Attendre quelques frames pour que NetworkTransform se synchronise
        yield return new WaitForSeconds(reactivationDelay);

        // Ne réactiver que si on n'a PAS l'autorité (les joueurs distants)
        // Les joueurs avec autorité sont gérés par OnStartAuthority
        if (characterController != null && !characterController.enabled && !isOwned)
        {
            characterController.enabled = true;
            if (showDebugLogs)
                Debug.Log($"[NetworkSpawnFix] ✓ CharacterController réactivé (délai écoulé) sur {gameObject.name}");
        }
    }
}
