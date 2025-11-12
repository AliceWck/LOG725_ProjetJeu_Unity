using Mirror;
using UnityEngine;

/// <summary>
/// Fix pour le problème de rotation du Gardien au spawn.
/// Le personnage spawn avec une rotation incorrecte (X ≈ -30°) causée par le ThirdPersonController.
/// Ce script force la rotation à rester verticale (X=0, Z=0) tout en conservant Y (direction).
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class RotationDebugFix : NetworkBehaviour
{
    [Header("Configuration")]
    [Tooltip("Forcer la rotation verticale (debout) en permanence")]
    [SerializeField] private bool forceUpright = true;

    [Tooltip("GameObject du modèle visuel (Ch42_nonPBR)")]
    [SerializeField] private Transform visualModel;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool disableThirdPersonControllerOnSpawn = false;

    private Quaternion correctRotation = Quaternion.identity;
    private bool hasLogged = false;
    private StarterAssets.ThirdPersonController thirdPersonController;

    private void Awake()
    {
        // Récupérer le ThirdPersonController
        thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();

        // Trouver le modèle visuel si non assigné
        if (visualModel == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Ch42") || child.name.Contains("nonPBR"))
                {
                    visualModel = child;
                    break;
                }
            }
        }

        // Sauvegarder la rotation correcte (debout)
        correctRotation = Quaternion.identity;

        // Option: Désactiver le ThirdPersonController pour diagnostic
        if (disableThirdPersonControllerOnSpawn && thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
            if (showDebugLogs)
                Debug.Log($"[RotationDebugFix] ThirdPersonController désactivé pour diagnostic");
        }

        // Forcer immédiatement la rotation correcte
        FixRotation();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        FixRotation();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        FixRotation();
    }

    private void Update()
    {
        if (!forceUpright) return;

        // Vérifier si la rotation a changé
        bool rootNeedsfix = Mathf.Abs(transform.rotation.eulerAngles.x) > 1f ||
                            Mathf.Abs(transform.rotation.eulerAngles.z) > 1f;

        bool modelNeedsFix = visualModel != null &&
                             (Mathf.Abs(visualModel.localRotation.eulerAngles.x) > 1f ||
                              Mathf.Abs(visualModel.localRotation.eulerAngles.z) > 1f);

        if (rootNeedsfix || modelNeedsFix)
        {
            if (showDebugLogs && !hasLogged)
            {
                Debug.LogWarning($"[RotationDebugFix] Rotation incorrecte corrigée sur {gameObject.name}");
                hasLogged = true;
            }
            FixRotation();
        }
    }

    private void LateUpdate()
    {
        if (!forceUpright) return;

        // Re-forcer la rotation à la fin de la frame (garde seulement Y)
        Vector3 currentEuler = transform.rotation.eulerAngles;
        if (Mathf.Abs(currentEuler.x) > 0.1f || Mathf.Abs(currentEuler.z) > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0f, currentEuler.y, 0f);
        }
    }

    private void FixRotation()
    {
        // Garder uniquement la rotation Y (direction horizontale)
        Vector3 currentEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, currentEuler.y, 0f);

        // Corriger aussi le modèle visuel
        if (visualModel != null)
        {
            visualModel.localRotation = Quaternion.identity;
        }
    }
}
