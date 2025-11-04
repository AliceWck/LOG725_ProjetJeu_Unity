using Mirror;
using UnityEngine;

public class WindowInteraction : MonoBehaviour
{
    [Header("Références")]
    public GameObject windowGlass; // La vitre/fenêtre
    public GameObject interactionUI; // Le texte "E"

    [Header("Paramètres")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Sons")]
    public AudioClip openSound; // Son d'ouverture
    public AudioClip closeSound; // Son de fermeture
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;

    [Header("Textes d'interaction")]
    public string openText = "E - Ouvrir la fenêtre";
    public string closedText = "E - Fermer la fenêtre";

    private Transform localPlayer;
    private bool isPlayerNear = false;
    private bool isWindowOpen = false; // false = fermée (vitre visible)
    private TMPro.TextMeshProUGUI interactionText;
    private AudioSource audioSource;

    void Start()
    {
        // Cacher le UI au début
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
            interactionText = interactionUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }

        // Vérifier que la vitre existe
        if (windowGlass == null)
        {
            Debug.LogError("WindowInteraction: Vitre non assignée sur " + gameObject.name);
        }

        // Créer un AudioSource pour les sons
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Son 3D
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 10f;
        audioSource.volume = soundVolume;

        // État initial : fenêtre fermée (vitre visible)
        if (windowGlass != null)
        {
            windowGlass.SetActive(!isWindowOpen);
        }

        UpdateInteractionText();
    }

    void Update()
    {
        // Chercher le joueur
        if (localPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                localPlayer = playerObj.transform;
            }

            if (localPlayer == null)
            {
                GamePlayer player = FindObjectOfType<GamePlayer>();
                if (player != null)
                {
                    localPlayer = player.transform;
                }
            }

            if (localPlayer == null)
                return;
        }

        // Calculer la distance
        float distance = Vector3.Distance(localPlayer.position, transform.position);

        // Vérif si le joueur est à portée
        if (distance <= interactionDistance)
        {
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                if (interactionUI != null)
                {
                    interactionUI.SetActive(true);
                }
            }

            // Détecter appui E
            if (Input.GetKeyDown(interactionKey))
            {
                ToggleWindow();
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

    void ToggleWindow()
    {
        isWindowOpen = !isWindowOpen;

        // Activer/désactiver vitre
        if (windowGlass != null)
        {
            windowGlass.SetActive(!isWindowOpen); // Si ouverte = pas de vitre
        }

        // Jouer le son qu'il faut (ouvrir ou fermer)
        if (audioSource != null)
        {
            AudioClip soundToPlay = isWindowOpen ? openSound : closeSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }

        UpdateInteractionText();

        Debug.Log($"Fenêtre {(isWindowOpen ? "ouverte (vitre cachée)" : "fermée (vitre visible)")}");
    }

    void UpdateInteractionText()
    {
        if (interactionText != null)
        {
            interactionText.text = isWindowOpen ? closedText : openText;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}



///////////////////////// VERSION ONLINE
///
//using UnityEngine;
//using Mirror;

//public class WindowInteraction : NetworkBehaviour
//{
//    [Header("Références")]
//    public GameObject windowGlass;
//    public GameObject interactionUI;

//    [Header("Paramètres")]
//    public float interactionDistance = 3f;
//    public KeyCode interactionKey = KeyCode.E;

//    [Header("Sons")]
//    public AudioClip openSound;
//    public AudioClip closeSound;
//    [Range(0f, 1f)]
//    public float soundVolume = 0.5f;

//    [SyncVar(hook = nameof(OnWindowStateChanged))]
//    private bool isWindowOpen = false;

//    private Transform localPlayer;
//    private bool isPlayerNear = false;
//    private AudioSource audioSource;

//    void Start()
//    {
//        if (interactionUI != null)
//        {
//            interactionUI.SetActive(false);
//        }

//        // Créer AudioSource
//        audioSource = gameObject.AddComponent<AudioSource>();
//        audioSource.playOnAwake = false;
//        audioSource.spatialBlend = 1f;
//        audioSource.volume = soundVolume;

//        // Appliquer état init
//        if (windowGlass != null)
//        {
//            windowGlass.SetActive(!isWindowOpen);
//        }
//    }

//    void Update()
//    {
//        // Chercher le joueur local
//        if (localPlayer == null)
//        {
//            GamePlayer[] allPlayers = FindObjectsOfType<GamePlayer>();

//            foreach (GamePlayer player in allPlayers)
//            {
//                if (player.isLocalPlayer)
//                {
//                    localPlayer = player.transform;
//                    break;
//                }
//            }

//            if (localPlayer == null)
//                return;
//        }

//        float distance = Vector3.Distance(localPlayer.position, transform.position);

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

//            if (Input.GetKeyDown(interactionKey))
//            {
//                CmdToggleWindow();
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

//    [Command(requiresAuthority = false)]
//    void CmdToggleWindow()
//    {
//        isWindowOpen = !isWindowOpen;

//        // Jouer le son sur le serveur (sera entendu par tous)
//        RpcPlaySound(isWindowOpen);

//        Debug.Log($"[Server] Fenêtre {(isWindowOpen ? "ouverte" : "fermée")}");
//    }

//    void OnWindowStateChanged(bool oldValue, bool newValue)
//    {
//        if (windowGlass != null)
//        {
//            windowGlass.SetActive(!newValue);
//        }
//    }

//    [ClientRpc]
//    void RpcPlaySound(bool opening)
//    {
//        if (audioSource != null)
//        {
//            AudioClip soundToPlay = opening ? openSound : closeSound;
//            if (soundToPlay != null)
//            {
//                audioSource.PlayOneShot(soundToPlay);
//            }
//        }
//    }

//    void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.cyan;
//        Gizmos.DrawWireSphere(transform.position, interactionDistance);
//    }
//}