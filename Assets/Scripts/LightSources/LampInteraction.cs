using UnityEngine;
using Mirror;

// ATTENTION pas oublier de rajouter le network identity sur la lampe dans l'inspecteur Unity, sinon marche pas du totu
public class NetworkedLampInteraction : NetworkBehaviour
{
    [Header("Références")]
    public Light lampLight;
    public GameObject interactionUI;

    [Header("Paramètres")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Debug")]
    public bool useLocalModeForTesting = false; // AJOUT pour tester sans réseau

    [SyncVar(hook = nameof(OnLampStateChanged))]
    private bool isLampOn = true;

    private Transform localPlayer;
    private bool isPlayerNear = false;

    void Start()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        if (lampLight == null)
        {
            lampLight = GetComponentInChildren<Light>();
        }

        if (lampLight != null)
        {
            lampLight.enabled = isLampOn;
        }

        Debug.Log($"[NetworkedLamp] Démarré - IsServer: {isServer}, IsClient: {isClient}");
    }

    void Update()
    {
        // Chercher le joueur local
        if (localPlayer == null)
        {
            GamePlayer[] allPlayers = FindObjectsOfType<GamePlayer>();

            foreach (GamePlayer player in allPlayers)
            {
                if (player.isLocalPlayer)
                {
                    localPlayer = player.transform;
                    Debug.Log($"NetworkedLamp: joueur local trouvé ({player.PlayerName})");
                    break;
                }
            }

            if (localPlayer == null)
                return;
        }

        // Calculer la distance
        float distance = Vector3.Distance(localPlayer.position, transform.position);

        // Vérifier si le joueur est à portée
        if (distance <= interactionDistance)
        {
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                Debug.Log($"Joueur proche de la lampe. Distance: {distance:F2}m");
                if (interactionUI != null)
                {
                    interactionUI.SetActive(true);
                }
            }

            // Détecter l'appui sur E
            if (Input.GetKeyDown(interactionKey))
            {
                Debug.Log("Touche E pressée");

                // MODE TEST : Toggle direct si pas de réseau
                if (useLocalModeForTesting)
                {
                    Debug.Log("[Mode Test Local] Toggle direct de la lampe");
                    isLampOn = !isLampOn;
                    OnLampStateChanged(isLampOn, isLampOn);
                }
                else
                {
                    // Mode réseau normal
                    if (isServer)
                    {
                        Debug.Log("[Mode Serveur] Toggle direct");
                        isLampOn = !isLampOn;
                    }
                    else
                    {
                        Debug.Log("[Mode Client] Envoi commande au serveur");
                        CmdToggleLamp();
                    }
                }
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
        Debug.Log($"[Command reçue] Toggle lampe sur serveur");
        isLampOn = !isLampOn;
        Debug.Log($"[Server] Lampe {(isLampOn ? "allumée" : "éteinte")}");
    }

    void OnLampStateChanged(bool oldValue, bool newValue)
    {
        if (lampLight != null)
        {
            lampLight.enabled = newValue;
        }
        Debug.Log($"[Hook] Lampe {(newValue ? "allumée" : "éteinte")}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}