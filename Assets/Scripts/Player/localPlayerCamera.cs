using UnityEngine;
using Mirror;

public class LocalPlayerCamera : NetworkBehaviour
{
    [Header("Camera Setup")]
    [Tooltip("Rien mettre dedans, sera trouv� automatiquement")]
    public ThirdPersonCamera thirdPersonCamera;

    [Header("Audio Listener")]
    public bool manageAudioListener = true;

    [Header("Debug")]
    public bool showDebugLogs = false;

    public override void OnStartLocalPlayer()
    {
        // Méthode est appelée seulement pour le joueur local
        if (showDebugLogs)
            Debug.Log($"[LocalPlayerCamera] OnStartLocalPlayer appelé pour {gameObject.name}");

        SetupCamera();
    }

    /// <summary>
    /// Alternative: Setup via OnStartAuthority pour mode Host
    /// </summary>
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        
        if (showDebugLogs)
            Debug.Log($"[LocalPlayerCamera] OnStartAuthority - Setup caméra (isLocal: {isLocalPlayer})");
        
        // Si isLocalPlayer est false mais on a l'autorité, on est en mode Host
        if (!isLocalPlayer)
        {
            Debug.Log($"[LocalPlayerCamera] Mode Host détecté - Setup caméra via autorité");
            SetupCamera();
        }
    }

    void SetupCamera()
    {
        if (showDebugLogs)
            Debug.Log("[LocalPlayerCamera] Setup cam�ra...");

        // Trouver script ThirdPersonCamera
        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();

            if (thirdPersonCamera == null)
            {
                Debug.LogError("[LocalPlayerCamera] aucun ThirdPersonCamera trouv� dans la sc�ne");
                return;
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log($"[LocalPlayerCamera] TPC trouv�e sur : {thirdPersonCamera.gameObject.name}");
            }
        }

        // Assigner ce joueur comme target
        thirdPersonCamera.target = transform;

        Debug.Log($"[LocalPlayerCamera] Cam�ra bien attach�e au  joueur local");
        Debug.Log($"  Personnage : {gameObject.name}");
        Debug.Log($"  Cam�ra : {thirdPersonCamera.gameObject.name}");

        // G�rer AudioListener
        if (manageAudioListener)
        {
            // D�sactiver tous les AudioListener des autres joueurs
            AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
            foreach (var listener in allListeners)
            {
                // Garder seulement celui de la Main Camera
                if (listener.transform != Camera.main?.transform)
                {
                    listener.enabled = false;
                    if (showDebugLogs)
                        Debug.Log($"[LocalPlayerCamera] AudioListener d�sactiv� sur {listener.name}");
                }
            }

            // S'assurer que Main Camera a un audioListener actif
            if (Camera.main != null)
            {
                AudioListener mainListener = Camera.main.GetComponent<AudioListener>();
                if (mainListener == null)
                {
                    mainListener = Camera.main.gameObject.AddComponent<AudioListener>();
                    if (showDebugLogs)
                        Debug.Log("[LocalPlayerCamera] AudioListener ajout� � main Camera");
                }
                else
                {
                    mainListener.enabled = true;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (isLocalPlayer && thirdPersonCamera != null)
        {
            thirdPersonCamera.target = null;

            if (showDebugLogs)
                Debug.Log("[LocalPlayerCamera] Cam�ra d�tach�e (joueur d�truit)");
        }
    }
}