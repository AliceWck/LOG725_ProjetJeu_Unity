using Mirror;
using UnityEngine;

//public class WindowInteraction : MonoBehaviour
//{
//    [Header("Références")]
//    public GameObject windowGlass; // La vitre/fenêtre
//    public GameObject interactionUI; // Le texte "E"

//    [Header("Paramètres")]
//    public float interactionDistance = 3f;
//    public KeyCode interactionKey = KeyCode.E;

//    [Header("Sons")]
//    public AudioClip openSound; // Son d'ouverture
//    public AudioClip closeSound; // Son de fermeture
//    [Range(0f, 1f)]
//    public float soundVolume = 0.5f;

//    [Header("Textes d'interaction")]
//    public string openText = "E - Ouvrir la fenêtre";
//    public string closedText = "E - Fermer la fenêtre";

//    private Transform localPlayer;
//    private bool isPlayerNear = false;
//    private bool isWindowOpen = false; // false = fermée (vitre visible)
//    private TMPro.TextMeshProUGUI interactionText;
//    private AudioSource audioSource;

//    void Start()
//    {
//        // Cacher le UI au début
//        if (interactionUI != null)
//        {
//            interactionUI.SetActive(false);
//            interactionText = interactionUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
//        }

//        // Vérifier que la vitre existe
//        if (windowGlass == null)
//        {
//            Debug.LogError("WindowInteraction: Vitre non assignée sur " + gameObject.name);
//        }

//        // Créer un AudioSource pour les sons
//        audioSource = gameObject.AddComponent<AudioSource>();
//        audioSource.playOnAwake = false;
//        audioSource.spatialBlend = 1f; // Son 3D
//        audioSource.minDistance = 1f;
//        audioSource.maxDistance = 10f;
//        audioSource.volume = soundVolume;

//        // État initial : fenêtre fermée (vitre visible)
//        if (windowGlass != null)
//        {
//            windowGlass.SetActive(!isWindowOpen);
//        }

//        UpdateInteractionText();
//    }

//    void Update()
//    {
//        // Chercher le joueur
//        if (localPlayer == null)
//        {
//            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//            if (playerObj != null)
//            {
//                localPlayer = playerObj.transform;
//            }

//            if (localPlayer == null)
//            {
//                GamePlayer player = FindObjectOfType<GamePlayer>();
//                if (player != null)
//                {
//                    localPlayer = player.transform;
//                }
//            }

//            if (localPlayer == null)
//                return;
//        }

//        // Calculer la distance
//        float distance = Vector3.Distance(localPlayer.position, transform.position);

//        // Vérif si le joueur est à portée
//        if (distance <= interactionDistance)
//        {
//            if (!isPlayerNear)
//            {
//                isPlayerNear = true;
//                if (interactionUI != null)
//                {
//                    interactionUI.SetActive(true);
//                }
//            }

//            // Détecter appui E
//            if (Input.GetKeyDown(interactionKey))
//            {
//                ToggleWindow();
//            }
//        }
//        else
//        {
//            if (isPlayerNear)
//            {
//                isPlayerNear = false;
//                if (interactionUI != null)
//                {
//                    interactionUI.SetActive(false);
//                }
//            }
//        }
//    }

//    void ToggleWindow()
//    {
//        isWindowOpen = !isWindowOpen;

//        // Activer/désactiver vitre
//        if (windowGlass != null)
//        {
//            windowGlass.SetActive(!isWindowOpen); // Si ouverte = pas de vitre
//        }

//        // Jouer le son qu'il faut (ouvrir ou fermer)
//        if (audioSource != null)
//        {
//            AudioClip soundToPlay = isWindowOpen ? openSound : closeSound;
//            if (soundToPlay != null)
//            {
//                audioSource.PlayOneShot(soundToPlay);
//            }
//        }

//        UpdateInteractionText();

//        Debug.Log($"Fenêtre {(isWindowOpen ? "ouverte (vitre cachée)" : "fermée (vitre visible)")}");
//    }

//    void UpdateInteractionText()
//    {
//        if (interactionText != null)
//        {
//            interactionText.text = isWindowOpen ? closedText : openText;
//        }
//    }

//    void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.cyan;
//        Gizmos.DrawWireSphere(transform.position, interactionDistance);
//    }
//}



///////////////////////// VERSION ONLINE
///

public class WindowInteraction : NetworkBehaviour
{
    [Header("Références")]
    public GameObject windowGlass;
    public GameObject interactionUI;

    [Header("Paramètres")]
    public float interactionDistance = 4f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Sons")]
    public AudioClip openSound;
    public AudioClip closeSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    public float minHearDistance = 1f;      // Distance où le son est à volume max
    public float maxHearDistance = 15f;     // Distance où le son devient inaudible

    [SyncVar(hook = nameof(OnWindowStateChanged))]
    private bool isWindowOpen = false;

    private Transform localPlayer;
    private bool isPlayerNear = false;
    private AudioSource audioSource;
    private bool playerSearchLogged = false;

    void Start()
    {
        Debug.Log($"[Window {gameObject.name}] Start - IsServer: {isServer}, IsClient: {isClient}");

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        // Créer AudioSource avec spatialisation 3D
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;              // 1 = son 3D complet (spatialisation)
        audioSource.volume = soundVolume;
        audioSource.minDistance = minHearDistance;  // Distance min pour atténuation
        audioSource.maxDistance = maxHearDistance;  // Distance max audible
        audioSource.rolloffMode = AudioRolloffMode.Linear; // Atténuation linéaire
        audioSource.dopplerLevel = 0f;              // Pas d'effet Doppler pour fenêtres

        // Appliquer état initial (sans son au démarrage)
        if (windowGlass != null)
        {
            windowGlass.SetActive(!isWindowOpen);
        }
    }

    void Update()
    {
        // Chercher le joueur local de plusieurs façons
        if (localPlayer == null)
        {
            // Méthode 1 : Via GamePlayer
            GamePlayer[] allPlayers = FindObjectsOfType<GamePlayer>();

            if (!playerSearchLogged)
            {
                Debug.Log($"[Window] Recherche joueur... {allPlayers.Length} GamePlayer(s) trouvé(s)");
            }

            foreach (GamePlayer player in allPlayers)
            {
                if (!playerSearchLogged)
                {
                    Debug.Log($"[Window] GamePlayer: {player.PlayerName}, isLocalPlayer: {player.isLocalPlayer}");
                }

                if (player.isLocalPlayer)
                {
                    localPlayer = player.transform;
                    Debug.Log($"[Window] Joueur local trouvé via GamePlayer: {player.PlayerName}");
                    playerSearchLogged = true;
                    break;
                }
            }

            // Méthode 2 : Via Tag "Player" (fallback)
            if (localPlayer == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    NetworkIdentity netId = playerObj.GetComponent<NetworkIdentity>();
                    if (netId != null && netId.isLocalPlayer)
                    {
                        localPlayer = playerObj.transform;
                        Debug.Log($"[Window] Joueur local trouvé via Tag");
                        playerSearchLogged = true;
                    }
                }
            }

            // Méthode 3 : Via Camera (last resort)
            if (localPlayer == null && !playerSearchLogged)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    // Chercher le joueur parent de la caméra
                    Transform parent = mainCam.transform.parent;
                    while (parent != null)
                    {
                        NetworkIdentity netId = parent.GetComponent<NetworkIdentity>();
                        if (netId != null && netId.isLocalPlayer)
                        {
                            localPlayer = parent;
                            Debug.Log($"[Window] Joueur local trouvé via Camera: {parent.name}");
                            playerSearchLogged = true;
                            break;
                        }
                        parent = parent.parent;
                    }
                }
            }

            if (localPlayer == null)
            {
                playerSearchLogged = true; // Pour ne pas spammer les logs
                return;
            }
        }

        // Calculer la distance
        float distance = Vector3.Distance(localPlayer.position, transform.position);

        // Vérifier si le joueur est à portée
        if (distance <= interactionDistance)
        {
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                Debug.Log($"[Window {gameObject.name}] Joueur proche. Distance: {distance:F2}m");

                if (interactionUI != null)
                {
                    interactionUI.SetActive(true);
                }
            }

            if (Input.GetKeyDown(interactionKey))
            {
                Debug.Log($"[Window {gameObject.name}] Touche E pressée");
                CmdToggleWindow();
            }
        }
        else
        {
            if (isPlayerNear)
            {
                isPlayerNear = false;
                if (interactionUI != null)
                {
                    interactionUI.SetActive(false);
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    void CmdToggleWindow()
    {
        Debug.Log($"[Window {gameObject.name}] Command reçue sur serveur");
        isWindowOpen = !isWindowOpen;

        Debug.Log($"[Window Server] Fenêtre {(isWindowOpen ? "OUVERTE" : "FERMÉE")}");
    }

    void OnWindowStateChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[Window {gameObject.name}] Hook: {oldValue} -> {newValue}");

        if (windowGlass != null)
        {
            windowGlass.SetActive(!newValue);
        }

        // Jouer le son localement sur CHAQUE client quand l'état change
        if (audioSource != null && oldValue != newValue) // Vérif qu'il y a vraiment un changement
        {
            AudioClip soundToPlay = newValue ? openSound : closeSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
                Debug.Log($"[Window {gameObject.name}] Son joué: {(newValue ? "ouverture" : "fermeture")}");
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}