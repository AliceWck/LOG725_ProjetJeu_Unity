using StarterAssets;
using UnityEngine;

public class OfflineLampInteraction : MonoBehaviour
{
    [Header("Références")]
    public Light lampLight;
    public GameObject interactionUI;

    [Header("Paramètres")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;

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
    }

    void Update()
    {
        // Chercher le joueur
        if (localPlayer == null)
        {
            // Chercher par tag d'abord
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                localPlayer = playerObj.transform;
                Debug.Log($"Lampe: Joueur trouvé via tag ({playerObj.name})");
            }

            // Si pas trouvé, chercher GamePlayer
            if (localPlayer == null)
            {
                GamePlayer player = FindObjectOfType<GamePlayer>();
                if (player != null)
                {
                    localPlayer = player.transform;
                    Debug.Log($"Lampe: Joueur trouvé via GamePlayer({player.PlayerName})");
                }
            }

            // Si toujours pas trouvé, chercher ThirdPersonController
            if (localPlayer == null)
            {
                var controller = FindObjectOfType<ThirdPersonController>();
                if (controller != null)
                {
                    localPlayer = controller.transform;
                    Debug.Log($"Lampe: Joueur trouvé via ThirdPersonController");
                }
            }

            if (localPlayer == null)
            {
                // Désactiver le UI si pas de joueur
                if (interactionUI != null && interactionUI.activeSelf)
                {
                    interactionUI.SetActive(false);
                }
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
                Debug.Log($"Joueur proche. Distance: {distance:F2}m");
                if (interactionUI != null)
                {
                    interactionUI.SetActive(true);
                }
            }

            // Détecter l'appui sur E
            if (Input.GetKeyDown(interactionKey))
            {
                Debug.Log("Touche E pressée");
                ToggleLamp();
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

    void ToggleLamp()
    {
        isLampOn = !isLampOn;

        if (lampLight != null)
        {
            lampLight.enabled = isLampOn;
            Debug.Log($"[Lampe] {(isLampOn ? "Allumée" : "Éteinte")}");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}