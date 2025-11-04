using UnityEngine;
using Mirror;

public class LocalPlayerCamera : NetworkBehaviour
{
    [Header("Camera Setup")]
    [Tooltip("Laisse vide, sera trouvé automatiquement")]
    public ThirdPersonCamera thirdPersonCamera;

    [Header("Audio Listener")]
    public bool manageAudioListener = true;

    [Header("Debug")]
    public bool showDebugLogs = true;

    public override void OnStartLocalPlayer()
    {
        // Cette méthode est appelée SEULEMENT pour le joueur local
        if (showDebugLogs)
            Debug.Log($"[LocalPlayerCamera] OnStartLocalPlayer appelé pour {gameObject.name}");

        SetupCamera();
    }

    void SetupCamera()
    {
        if (showDebugLogs)
            Debug.Log("[LocalPlayerCamera] Setup caméra...");

        // 1. Trouver le script ThirdPersonCamera
        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();

            if (thirdPersonCamera == null)
            {
                Debug.LogError("[LocalPlayerCamera] AUCUN ThirdPersonCamera trouvé dans la scène !");
                Debug.LogError(" Assure-toi que Main Camera a le script ThirdPersonCamera");
                return;
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log($"[LocalPlayerCamera] ThirdPersonCamera trouvé sur : {thirdPersonCamera.gameObject.name}");
            }
        }

        // 2. Assigner CE joueur comme target
        thirdPersonCamera.target = transform;

        Debug.Log($"[LocalPlayerCamera]CAMÉRA ATTACHÉE AU JOUEUR LOCAL ");
        Debug.Log($"  Personnage : {gameObject.name}");
        Debug.Log($"  Caméra : {thirdPersonCamera.gameObject.name}");

        // 3. Gérer l'AudioListener
        if (manageAudioListener)
        {
            // Désactiver tous les AudioListener des autres joueurs
            AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
            foreach (var listener in allListeners)
            {
                // Garder seulement celui de la Main Camera
                if (listener.transform != Camera.main?.transform)
                {
                    listener.enabled = false;
                    if (showDebugLogs)
                        Debug.Log($"[LocalPlayerCamera] AudioListener désactivé sur {listener.name}");
                }
            }

            // S'assurer que Main Camera a un AudioListener actif
            if (Camera.main != null)
            {
                AudioListener mainListener = Camera.main.GetComponent<AudioListener>();
                if (mainListener == null)
                {
                    mainListener = Camera.main.gameObject.AddComponent<AudioListener>();
                    if (showDebugLogs)
                        Debug.Log("[LocalPlayerCamera] AudioListener ajouté à Main Camera");
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
                Debug.Log("[LocalPlayerCamera] Caméra détachée (joueur détruit)");
        }
    }
}