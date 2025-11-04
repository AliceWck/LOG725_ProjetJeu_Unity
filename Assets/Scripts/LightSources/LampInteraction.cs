using UnityEngine;
using Mirror;

// ATTENTION pas oublier de rajouter le network identity sur la lampe dans l'inspecteur Unity, sinon marche pas du totu
public class NetworkedLampInteraction : NetworkBehaviour
{
    [Header("Références")]
    public Light lampLight;
    public GameObject interactionUI;

    [Header("Paramètres")]
    public float interactionDistance = 1f;
    public KeyCode interactionKey = KeyCode.E;

    [SyncVar(hook = nameof(OnLampStateChanged))]
    private bool isLampOn = true;

    private Transform localPlayer;
    private bool isPlayerNear = false;
    private bool playerSearchLogged = false;

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

        // Appliquer état initial
        if (lampLight != null)
        {
            lampLight.enabled = isLampOn;
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
                Debug.Log($"[Lamp] Recherche joueur... {allPlayers.Length} GamePlayer(s) trouvé(s)");
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
                    Debug.Log($"[Lamp] Joueur local trouvé via GamePlayer: {player.PlayerName}");
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
                        Debug.Log($"[Lamp] Joueur local trouvé via Tag");
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
                            Debug.Log($"[Lamp] Joueur local trouvé via Camera: {parent.name}");
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
                Debug.Log($"[Lamp {gameObject.name}] Joueur proche. Distance: {distance:F2}m");

                if (interactionUI != null)
                {
                    interactionUI.SetActive(true);
                }
            }

            if (Input.GetKeyDown(interactionKey))
            {
                Debug.Log($"[Lamp {gameObject.name}] Touche E pressée");
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

    [Command(requiresAuthority = false)]
    void CmdToggleLamp()
    {
        Debug.Log($"[Lamp {gameObject.name}] Command reçue sur serveur");
        isLampOn = !isLampOn;
        Debug.Log($"[Lamp Server] Lampe {(isLampOn ? "ALLUMÉE" : "ÉTEINTE")}");
    }

    void OnLampStateChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[Lamp {gameObject.name}] Hook: {oldValue} -> {newValue}");

        if (lampLight != null)
        {
            lampLight.enabled = newValue;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}