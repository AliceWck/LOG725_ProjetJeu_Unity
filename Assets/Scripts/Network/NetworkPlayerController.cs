using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private bool autoFindControllers = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private MonoBehaviour thirdPersonController;
    private CharacterController characterController;
    private Camera playerCamera;

    // Surcharger les méthodes de sérialisation pour éviter les erreurs
    // Ce script ne synchronise aucune donnée, juste gère les composants localement
    public override void OnSerialize(NetworkWriter writer, bool initialState)
    {
        // Ne rien sérialiser - ce script gère uniquement l'activation locale des composants
        // Appeler la méthode de base pour éviter les erreurs de sérialisation
        base.OnSerialize(writer, initialState);
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        // Appeler la méthode de base pour maintenir la compatibilité
        base.OnDeserialize(reader, initialState);
    }

    private void Awake()
    {
        if (autoFindControllers)
        {
            FindControllers();
        }

        // Désactiver tout par défaut
        DisableRemotePlayerComponents();
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

        // NOTE: On n'active PLUS les composants ici
        // L'activation se fait maintenant dans OnStartAuthority()
        // qui est appelé automatiquement après OnStartLocalPlayer

        if (showDebugLogs)
        {
            Debug.Log($"[NetworkPlayerController] ✓ Joueur local configuré - NetID: {netId}");
        }

        // Activer directement pour le joueur local
        EnableLocalPlayerComponents();
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
    /// Appelé quand ce client reçoit l'autorité sur cet objet
    /// C'EST ICI qu'on doit activer les contrôles, pas dans OnStartLocalPlayer
    /// </summary>
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        if (showDebugLogs)
            Debug.Log($"[NetworkPlayerController] ✓ AUTORITÉ REÇUE - Activation des contrôles pour NetID: {netId}");

        // Activer les composants maintenant qu'on a l'autorité
        EnableLocalPlayerComponents();
    }

    /// <summary>
    /// Appelé quand ce client perd l'autorité sur cet objet
    /// </summary>
    public override void OnStopAuthority()
    {
        base.OnStopAuthority();

        if (showDebugLogs)
            Debug.Log($"[NetworkPlayerController] ✗ AUTORITÉ PERDUE - Désactivation des contrôles pour NetID: {netId}");

        // Désactiver les composants
        DisableRemotePlayerComponents();
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
            // Ne pas se sélectionner soi-même !
            if (component == this) continue;

            string typeName = component.GetType().Name;

            // Exclure les scripts réseau
            if (typeName.Contains("Network") ||
                typeName == "CharacterController" ||
                typeName == "GamePlayer")
            {
                continue;
            }

            // Chercher le vrai contrôleur de mouvement
            if (typeName.Contains("ThirdPerson") ||
                typeName.Contains("FirstPerson") ||
                typeName.Contains("PlayerController") ||
                typeName.Contains("PlayerMovement") ||
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
        if (showDebugLogs)
            Debug.Log($"[NetworkPlayerController] EnableLocalPlayerComponents appelé - IsLocal: {isLocalPlayer}, NetID: {netId}");

        // Activer le contrôleur de mouvement
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
            if (showDebugLogs)
                Debug.Log($"[NetworkPlayerController] ✓ {thirdPersonController.GetType().Name} ACTIVÉ");
        }
        else
        {
            Debug.LogError("[NetworkPlayerController] ✗ ThirdPersonController non trouvé");
        }

        // Activer la caméra
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
            playerCamera.enabled = true;

            // Activer l'Audio Listener
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
            if (showDebugLogs)
                Debug.Log("[NetworkPlayerController] ✓ CharacterController activé");
        }

        // CRITIQUE : Activer aussi les inputs
        var starterInputs = GetComponent<StarterAssets.StarterAssetsInputs>();
        if (starterInputs != null)
        {
            starterInputs.enabled = true;
            if (showDebugLogs)
                Debug.Log("[NetworkPlayerController] ✓ StarterAssetsInputs activé");
        }

    #if ENABLE_INPUT_SYSTEM
            var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = true;
                if (showDebugLogs)
                    Debug.Log("[NetworkPlayerController] ✓ PlayerInput activé");
            }
    #endif
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

        // Désactiver le CharacterController
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Désactiver les inputs
        var starterInputs = GetComponent<StarterAssets.StarterAssetsInputs>();
        if (starterInputs != null)
        {
            starterInputs.enabled = false;
        }
#if ENABLE_INPUT_SYSTEM
        var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }
#endif
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        Component networkTransform = GetComponent("NetworkTransform") as Component;
        Component networkTransformReliable = GetComponent("NetworkTransformReliable") as Component;

        if (networkTransform == null && networkTransformReliable == null)
        {
            Debug.LogWarning($"[NetworkPlayerController] '{gameObject.name}' n'a pas de NetworkTransform !", this);
        }
    }
#endif
}