using UnityEngine;
using Mirror;

// ATTENTION pas oublier de rajouter le network identity sur la lampe dans l'inspecteur Unity, sinon marche pas du tout
public class NetworkedLampInteraction : NetworkBehaviour
{
    [Header("R�f�rences")]
    public Light lampLight;
    public GameObject interactionUI;

    [Header("Param�tres")]
    public float interactionDistance = 1f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Sons")]
    public AudioClip lightOnSound;
    public AudioClip lightOffSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    public float minHearDistance = 1f;      // Distance o� le son est � volume max
    public float maxHearDistance = 15f;     // Distance o� le son devient inaudible

    [SyncVar(hook = nameof(OnLampStateChanged))]
    private bool isLampOn = true;

    private Transform localPlayer;
    private bool isPlayerNear = false;
    private bool playerSearchLogged = false;
    private AudioSource audioSource;

    void Start()
    {
        Debug.Log($"[Lamp {gameObject.name}] Start - IsServer: {isServer}, IsClient: {isClient}");

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        if (lampLight == null)
        {
            lampLight = GetComponentInChildren<Light>();
        }

        // Cr�er AudioSource avec spatialisation 3D
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;              // 1 = son 3D complet (spatialisation)
        audioSource.volume = soundVolume;
        audioSource.minDistance = minHearDistance;  // Distance min pour att�nuation
        audioSource.maxDistance = maxHearDistance;  // Distance max audible
        audioSource.rolloffMode = AudioRolloffMode.Linear; // Att�nuation lin�aire
        audioSource.dopplerLevel = 0f;              // Pas d'effet Doppler pour les lampes

        // Appliquer �tat initial (sans son au d�marrage)
        if (lampLight != null)
        {
            lampLight.enabled = isLampOn;
        }
    }

    void Update()
    {
        // Chercher le joueur local de plusieurs fa�ons
        if (localPlayer == null)
        {
            // M�thode 1 : Via GamePlayer
            GamePlayer[] allPlayers = FindObjectsOfType<GamePlayer>();

            if (!playerSearchLogged)
            {
                Debug.Log($"[Lamp] Recherche joueur... {allPlayers.Length} GamePlayer(s) trouv�(s)");
            }

            foreach (GamePlayer player in allPlayers)
            {
                if (!playerSearchLogged)
                {
                    Debug.Log($"[Lamp] GamePlayer: {player.PlayerName}, isLocalPlayer: {player.isLocalPlayer}");
                }

                if (player.isLocalPlayer)
                {
                    localPlayer = player.transform;
                    Debug.Log($"[Lamp] Joueur local trouv� via GamePlayer: {player.PlayerName}");
                    playerSearchLogged = true;
                    break;
                }
            }

            // M�thode 2 : Via Tag "Player" (fallback)
            if (localPlayer == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    NetworkIdentity netId = playerObj.GetComponent<NetworkIdentity>();
                    if (netId != null && netId.isLocalPlayer)
                    {
                        localPlayer = playerObj.transform;
                        Debug.Log($"[Lamp] Joueur local trouv� via Tag");
                        playerSearchLogged = true;
                    }
                }
            }

            // M�thode 3 : Via Camera (last resort)
            if (localPlayer == null && !playerSearchLogged)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    // Chercher le joueur parent de la cam�ra
                    Transform parent = mainCam.transform.parent;
                    while (parent != null)
                    {
                        NetworkIdentity netId = parent.GetComponent<NetworkIdentity>();
                        if (netId != null && netId.isLocalPlayer)
                        {
                            localPlayer = parent;
                            Debug.Log($"[Lamp] Joueur local trouv� via Camera: {parent.name}");
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

        // V�rifier si le joueur est � port�e
        if (distance <= interactionDistance)
        {
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                Debug.Log($"[Lamp {gameObject.name}] Joueur proche. Distance: {distance:F2}m");

                if (interactionUI != null)
                {
                    interactionUI.SetActive(true);
                }
            }

            if (Input.GetKeyDown(interactionKey))
            {
                Debug.Log($"[Lamp {gameObject.name}] Touche E press�e");
                CmdToggleLamp();
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

    public bool GetLampState()
    {
        return isLampOn;
    }

    [Command(requiresAuthority = false)]
    void CmdToggleLamp()
    {
        Debug.Log($"[Lamp {gameObject.name}] Command re�ue sur serveur");
        isLampOn = !isLampOn;
        Debug.Log($"[Lamp Server] Lampe {(isLampOn ? "ALLUM�E" : "�TEINTE")}");
    }

    // Méthode serveur publique pour être appelée par des objets serveur (ex : fantôme IA côté serveur)
    [Server]
    public void ServerToggleLamp()
    {
        isLampOn = !isLampOn;
        Debug.Log($"[Lamp {gameObject.name}] ServerToggleLamp appelé. Nouvel état: {(isLampOn ? "ON" : "OFF")}");
    }

    void OnLampStateChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[Lamp {gameObject.name}] Hook: {oldValue} -> {newValue}");

        // Changer l'�tat visuel
        if (lampLight != null)
        {
            lampLight.enabled = newValue;
        }

        // Jouer le son localement sur chaque client quand l'�tat change
        if (audioSource != null && oldValue != newValue) // V�rifier qu'il y a vraiment un changement
        {
            AudioClip soundToPlay = newValue ? lightOnSound : lightOffSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
                Debug.Log($"[Lamp {gameObject.name}] Son jou�: {(newValue ? "allumage" : "extinction")}");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}