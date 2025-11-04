using UnityEngine;
using Mirror;

public class LocalPlayerCamera : NetworkBehaviour
{
    [Header("Camera Setup")]
    [Tooltip("Rien mettre dedans, sera trouvé automatiquement")]
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

    void SetupCamera()
    {
        if (showDebugLogs)
            Debug.Log("[LocalPlayerCamera] Setup caméra...");

        // Trouver script ThirdPersonCamera
        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();

            if (thirdPersonCamera == null)
            {
                Debug.LogError("[LocalPlayerCamera] aucun ThirdPersonCamera trouvé dans la scène");
                return;
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log($"[LocalPlayerCamera] TPC trouvée sur : {thirdPersonCamera.gameObject.name}");
            }
        }

        // Assigner ce joueur comme target
        thirdPersonCamera.target = transform;

        Debug.Log($"[LocalPlayerCamera] Caméra bien attachée au  joueur local");
        Debug.Log($"  Personnage : {gameObject.name}");
        Debug.Log($"  Caméra : {thirdPersonCamera.gameObject.name}");

        // Gérer AudioListener
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

            // S'assurer que Main Camera a un audioListener actif
            if (Camera.main != null)
            {
                AudioListener mainListener = Camera.main.GetComponent<AudioListener>();
                if (mainListener == null)
                {
                    mainListener = Camera.main.gameObject.AddComponent<AudioListener>();
                    if (showDebugLogs)
                        Debug.Log("[LocalPlayerCamera] AudioListener ajouté à main Camera");
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