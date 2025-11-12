using Mirror;
using UnityEngine;

/// <summary>
/// Gère l'autorité réseau pour les contrôleurs de mouvement.
/// Ce script DOIT être sur tous les prefabs de joueurs (Gardien + Ombre).
/// 
/// IMPORTANT: Ce script active/désactive les contrôleurs selon l'AUTORITÉ réseau,
/// pas seulement selon isLocalPlayer.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class NetworkMovementAuthority : NetworkBehaviour
{
    [Header("Components à gérer")]
    [Tooltip("Le script de contrôle du mouvement (ThirdPersonController, etc.)")]
    [SerializeField] private MonoBehaviour movementController;

    [Tooltip("Le CharacterController Unity")]
    [SerializeField] private CharacterController characterController;

    [Header("Auto-Find")]
    [SerializeField] private bool autoFindComponents = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;  // Activé pour diagnostic

    private void Awake()
    {
        if (autoFindComponents)
        {
            FindComponents();
        }
    }

    private void Start()
    {
        // OnStartAuthority est appelé AVANT Start()
        // On ne fait rien ici pour ne pas interférer avec l'autorité déjà configurée
        // Juste vérifier et synchroniser si nécessaire
        if (!isOwned && (movementController != null && movementController.enabled))
        {
            // Seulement désactiver si on n'a pas l'autorité ET que c'est activé
            DisableMovement();
        }
    }

    /// <summary>
    /// Appelé quand ce client reçoit l'autorité sur cet objet
    /// </summary>
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        if (showDebugLogs)
            Debug.Log($"[NetworkMovementAuthority] OnStartAuthority - Activation du mouvement pour {gameObject.name}");

        EnableMovement();
    }

    /// <summary>
    /// Appelé quand ce client perd l'autorité sur cet objet
    /// </summary>
    public override void OnStopAuthority()
    {
        base.OnStopAuthority();

        if (showDebugLogs)
            Debug.Log($"[NetworkMovementAuthority] OnStopAuthority - Désactivation du mouvement pour {gameObject.name}");

        DisableMovement();
    }

    /// <summary>
    /// Active les composants de mouvement
    /// </summary>
    private void EnableMovement()
    {
        if (movementController != null)
        {
            movementController.enabled = true;
            if (showDebugLogs)
                Debug.Log($"[NetworkMovementAuthority] ✓ {movementController.GetType().Name} ACTIVÉ");
        }
        else
        {
            Debug.LogError($"[NetworkMovementAuthority] ❌ MovementController est NULL sur {gameObject.name}!");
        }

        if (characterController != null)
        {
            characterController.enabled = true;
            if (showDebugLogs)
                Debug.Log($"[NetworkMovementAuthority] ✓ CharacterController ACTIVÉ");
        }
    }

    /// <summary>
    /// Désactive les composants de mouvement
    /// </summary>
    private void DisableMovement()
    {
        if (movementController != null)
        {
            movementController.enabled = false;
            if (showDebugLogs)
                Debug.Log($"[NetworkMovementAuthority] ✗ {movementController.GetType().Name} DÉSACTIVÉ");
        }

        if (characterController != null)
        {
            characterController.enabled = false;
            if (showDebugLogs)
                Debug.Log($"[NetworkMovementAuthority] ✗ CharacterController DÉSACTIVÉ");
        }
    }

    /// <summary>
    /// Trouve automatiquement les composants
    /// </summary>
    private void FindComponents()
    {
        // Trouver le CharacterController
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        // Trouver le contrôleur de mouvement
        if (movementController == null)
        {
            MonoBehaviour[] allComponents = GetComponents<MonoBehaviour>();
            foreach (var comp in allComponents)
            {
                if (comp == null || comp == this) continue;

                string typeName = comp.GetType().Name;

                // Liste des scripts de mouvement à EXCLURE (scripts réseau uniquement)
                if (typeName == "NetworkMovementAuthority" ||
                    typeName == "NetworkPlayerController" ||
                    typeName == "NetworkInputControl" ||
                    typeName == "NetworkIdentity" ||
                    typeName == "NetworkTransform" ||
                    typeName == "GamePlayer" ||
                    typeName == "CharacterController")
                {
                    continue;
                }

                // Chercher les scripts qui contiennent "Controller", "Movement" ou des mots-clés de mouvement
                if (typeName.Contains("ThirdPerson") ||
                    typeName.Contains("FirstPerson") ||
                    typeName.Contains("PlayerMovement") ||
                    typeName.Contains("PlayerController") ||
                    typeName.Contains("Movement") ||
                    (typeName.Contains("Controller") && !typeName.Contains("Character")))
                {
                    movementController = comp;
                    Debug.Log($"[NetworkMovementAuthority] ✓ Contrôleur de mouvement trouvé: {typeName} sur {gameObject.name}");
                    break;
                }
            }
        }

        if (movementController == null)
        {
            Debug.LogWarning($"[NetworkMovementAuthority] ⚠️ Aucun contrôleur de mouvement trouvé sur {gameObject.name}! " +
                           "Scripts disponibles:");

            // Lister tous les scripts pour aider au diagnostic
            MonoBehaviour[] allComponents = GetComponents<MonoBehaviour>();
            foreach (var comp in allComponents)
            {
                if (comp != null)
                    Debug.Log($"  - {comp.GetType().Name}");
            }

            Debug.LogWarning("Assignez le contrôleur de mouvement manuellement dans l'Inspector.");
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        
        // Vérification dans l'éditeur
        if (autoFindComponents)
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            
            if (movementController == null)
                FindComponents();
        }
    }
#endif
}
