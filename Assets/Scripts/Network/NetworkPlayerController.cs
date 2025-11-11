using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private bool autoFindControllers = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private MonoBehaviour thirdPersonController;
    private CharacterController characterController;
    private Camera playerCamera;

    private void Awake()
    {
        if (autoFindControllers)
        {
            FindControllers();
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (showDebugLogs)
        {
            Debug.Log($"[NetworkPlayerController] Client démarré - IsLocal: {isLocalPlayer}, " +
                      $"HasAuthority: {isOwned}, NetID: {netId}");
        }
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Activer les composants pour le joueur local
        EnableLocalPlayerComponents();

        if (showDebugLogs)
        {
            Debug.Log($"[NetworkPlayerController] ✓ Joueur local configuré - NetID: {netId}");
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (showDebugLogs)
        {
            Debug.Log($"[NetworkPlayerController] Serveur démarré pour NetID: {netId}");
        }
    }

    /// <summary>
    /// Trouve automatiquement les contrôleurs sur le GameObject
    /// </summary>
    private void FindControllers()
    {
        // Chercher le ThirdPersonController (ou tout contrôleur personnalisé)
        MonoBehaviour[] allComponents = GetComponents<MonoBehaviour>();
        foreach (var component in allComponents)
        {
            string typeName = component.GetType().Name;
            if (typeName.Contains("ThirdPerson") ||
                (typeName.Contains("Controller") && !typeName.Contains("Character")))
            {
                thirdPersonController = component;
                if (showDebugLogs)
                {
                    Debug.Log($"[NetworkPlayerController] ThirdPersonController trouvé: {typeName}");
                }
                break;
            }
        }

        // CharacterController
        characterController = GetComponent<CharacterController>();

        // Caméra
        playerCamera = GetComponentInChildren<Camera>();
    }

    /// <summary>
    /// Active les composants nécessaires pour le joueur local
    /// </summary>
    private void EnableLocalPlayerComponents()
    {
        // Activer le contrôleur de mouvement
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
            if (showDebugLogs)
            {
                Debug.Log($"[NetworkPlayerController] ✓ {thirdPersonController.GetType().Name} activé pour le joueur local");
            }
        }
        else
        {
            Debug.LogWarning("[NetworkPlayerController] ThirdPersonController non trouvé !");
        }

        // Activer la caméra
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
            playerCamera.enabled = true;

            // Désactiver l'Audio Listener des autres joueurs pour éviter les conflits
            AudioListener audioListener = playerCamera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }

            if (showDebugLogs)
            {
                Debug.Log("[NetworkPlayerController] ✓ Caméra activée pour le joueur local");
            }
        }

        // Activer le CharacterController si présent
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    /// <summary>
    /// Désactive les composants pour les joueurs distants
    /// </summary>
    private void DisableRemotePlayerComponents()
    {
        // Désactiver le contrôleur de mouvement pour les joueurs distants
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
            if (showDebugLogs)
            {
                Debug.Log($"[NetworkPlayerController] {thirdPersonController.GetType().Name} désactivé pour joueur distant");
            }
        }

        // Désactiver la caméra
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
            playerCamera.enabled = false;

            // Désactiver l'Audio Listener
            AudioListener audioListener = playerCamera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }
        }
    }

    private void Start()
    {
        // Si ce n'est pas le joueur local, désactiver les composants
        if (!isLocalPlayer)
        {
            DisableRemotePlayerComponents();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Vérifier que NetworkTransform est présent en cherchant par nom de type
        Component networkTransform = GetComponent("NetworkTransform") as Component;
        Component networkTransformReliable = GetComponent("NetworkTransformReliable") as Component;
        
        if (networkTransform == null && networkTransformReliable == null)
        {
            Debug.LogWarning($"[NetworkPlayerController] '{gameObject.name}' n'a pas de NetworkTransform ! " +
                           "Ajoutez NetworkTransform ou NetworkTransformReliable pour synchroniser la position.", this);
        }

        // Vérifier que NetworkIdentity a Local Player Authority
        NetworkIdentity identity = GetComponent<NetworkIdentity>();
        if (identity != null)
        {
            // Note: Dans Mirror, l'autorité est gérée automatiquement au runtime
            // Pas besoin de vérifier localPlayerAuthority en OnValidate
        }
    }
#endif
}
