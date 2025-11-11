using Mirror;
using UnityEngine;

/// <summary>
/// Script de vérification pour s'assurer que les prefabs de joueurs
/// ont tous les composants réseau nécessaires.
/// 
/// COMPOSANTS REQUIS SUR LE PREFAB JOUEUR :
/// 1. NetworkIdentity - Identifie l'objet sur le réseau
/// 2. NetworkTransform (ou NetworkTransformReliable) - Synchronise la position/rotation
/// 3. GamePlayer (ou votre script de joueur) - La logique du joueur
/// 
/// OPTIONNEL :
/// - NetworkAnimator - Si vous utilisez des animations
/// - NetworkRigidbody - Si vous utilisez la physique
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class NetworkPlayerSetup : NetworkBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        VerifyNetworkComponents();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (showDebugLogs)
        {
            Debug.Log($"[NetworkPlayerSetup] Client démarré - IsLocal: {isLocalPlayer}, " +
                      $"HasAuthority: {isOwned}, NetID: {netId}");
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (showDebugLogs)
        {
            Debug.Log($"[NetworkPlayerSetup] ✓ Joueur local confirmé - NetID: {netId}");
        }
    }

    private void VerifyNetworkComponents()
    {
        bool allGood = true;

        // Vérifier NetworkIdentity
        NetworkIdentity identity = GetComponent<NetworkIdentity>();
        if (identity == null)
        {
            Debug.LogError("[NetworkPlayerSetup] ❌ NetworkIdentity manquant !", this);
            allGood = false;
        }
        else if (showDebugLogs)
        {
            Debug.Log($"[NetworkPlayerSetup] ✓ NetworkIdentity présent (NetID: {identity.netId})");
        }

        // Vérifier NetworkTransform ou NetworkTransformReliable en utilisant GetComponent avec string
        Component netTransform = GetComponent("NetworkTransform") as Component;
        Component netTransformReliable = GetComponent("NetworkTransformReliable") as Component;

        if (netTransform == null && netTransformReliable == null)
        {
            Debug.LogError("[NetworkPlayerSetup] ❌ NetworkTransform ou NetworkTransformReliable manquant ! " +
                          "Le joueur ne sera pas synchronisé sur le réseau.", this);
            allGood = false;
        }
        else if (showDebugLogs)
        {
            string transformType = netTransform != null ? "NetworkTransform" : "NetworkTransformReliable";
            Debug.Log($"[NetworkPlayerSetup] ✓ {transformType} présent");
        }

        // Vérifier GamePlayer
        GamePlayer gamePlayer = GetComponent<GamePlayer>();
        if (gamePlayer == null)
        {
            Debug.LogWarning("[NetworkPlayerSetup] ⚠ GamePlayer script manquant !", this);
        }
        else if (showDebugLogs)
        {
            Debug.Log("[NetworkPlayerSetup] ✓ GamePlayer script présent");
        }

        // Optionnel : Vérifier NetworkAnimator si présent
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();
        if (netAnimator != null && showDebugLogs)
        {
            Debug.Log("[NetworkPlayerSetup] ✓ NetworkAnimator présent (optionnel)");
        }

        if (allGood && showDebugLogs)
        {
            Debug.Log($"[NetworkPlayerSetup] ✅ Tous les composants réseau requis sont présents sur {gameObject.name}");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Dans l'éditeur, vérifier si NetworkTransform existe en utilisant GetComponent avec string
        Component netTransform = GetComponent("NetworkTransform") as Component;
        Component netTransformReliable = GetComponent("NetworkTransformReliable") as Component;
        
        if (netTransform == null && netTransformReliable == null)
        {
            Debug.LogWarning($"[NetworkPlayerSetup] Le prefab '{gameObject.name}' n'a pas de NetworkTransform ! " +
                           "Ajoutez NetworkTransform ou NetworkTransformReliable pour synchroniser la position.", this);
        }
    }
#endif
}
