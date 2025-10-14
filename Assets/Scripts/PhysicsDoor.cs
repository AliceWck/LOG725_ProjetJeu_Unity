using UnityEngine;

public class OuverturePorte2 : MonoBehaviour
{
    [Header("Settings")]
    public float maxOpenAngle = 90f;       // Angle maximum d'ouverture
    public float openSpeed = 5f;           // Vitesse d'ouverture/fermeture
    public float closeDelay = 1f;          // Temps avant fermeture auto quand le joueur part
    public bool autoClose = true;          // Fermeture automatique ?

    [Header("Push Detection")]
    public float pushSensitivity = 50f;    // Sensibilité : combien la porte s'ouvre par rapport au mouvement
    public float minPushForce = 0.1f;      // Force minimum pour bouger la porte

    [Header("References")]
    public Transform doorWing;             // La partie qui tourne (doorWing)
    public Transform doorPivot;            // Le pivot de la porte (généralement doorWing lui-même)

    private float currentAngle = 0f;       // Angle actuel de la porte
    private float targetAngle = 0f;        // Angle cible
    private float closeTimer = 0f;
    private Quaternion closedRotation;
    private int openDirection = 0;         // 0 = non défini, 1 = droite, -1 = gauche
    private Vector3 playerLastPosition;
    private bool playerNearby = false;

    void Start()
    {
        // Si doorWing n'est pas assigné, cherche dans les enfants
        if (doorWing == null)
        {
            doorWing = transform.Find("doorWing");
            if (doorWing == null)
            {
                Debug.LogError("doorWing introuvable ! Assigne-le dans l'Inspector.");
                enabled = false;
                return;
            }
        }

        if (doorPivot == null)
        {
            doorPivot = doorWing;
        }

        // Sauvegarde la rotation fermée
        closedRotation = doorWing.localRotation;
        currentAngle = 0f;
        targetAngle = 0f;
    }

    void Update()
    {
        // Interpoler vers l'angle cible
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * openSpeed);

        // Appliquer la rotation
        doorWing.localRotation = closedRotation * Quaternion.Euler(0, currentAngle, 0);

        // Timer de fermeture auto quand le joueur n'est plus là
        if (autoClose && !playerNearby && Mathf.Abs(targetAngle) > 0.1f)
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0f)
            {
                targetAngle = 0f;
                openDirection = 0;
                Debug.Log("Porte se referme automatiquement");
            }
        }
    }

    // Appelé quand le joueur entre dans le trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            playerLastPosition = other.transform.position;
            closeTimer = closeDelay;
            Debug.Log("Joueur entre dans la zone de la porte");
        }
    }

    // Appelé pendant que le joueur est dans le trigger
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 currentPosition = other.transform.position;
            Vector3 movement = currentPosition - playerLastPosition;

            // Si le joueur bouge suffisamment
            if (movement.magnitude > minPushForce * Time.deltaTime)
            {
                // Calculer la composante du mouvement qui pousse la porte
                Vector3 doorForward = doorPivot.forward;
                Vector3 doorRight = doorPivot.right;

                float pushForward = Vector3.Dot(movement, doorForward);

                // Déterminer de quel côté le joueur se trouve par rapport au pivot
                Vector3 toPlayer = currentPosition - doorPivot.position;
                float side = Vector3.Dot(toPlayer, doorRight);

                // Si c'est la première poussée, déterminer la direction d'ouverture
                if (openDirection == 0 && Mathf.Abs(pushForward) > 0.01f)
                {
                    openDirection = (side > 0) ? 1 : -1;
                    Debug.Log($"Direction d'ouverture : {(openDirection > 0 ? "droite" : "gauche")}");
                }

                // Calculer l'angle cible basé sur le mouvement
                if (openDirection != 0)
                {
                    // Si le joueur avance (pousse), augmente l'angle
                    if (pushForward > 0)
                    {
                        float pushStrength = movement.magnitude * pushSensitivity;
                        targetAngle += pushStrength * Time.deltaTime * openDirection;
                    }
                    // Si le joueur recule, diminue l'angle
                    else if (pushForward < 0)
                    {
                        float pullStrength = movement.magnitude * pushSensitivity * 0.7f; // Un peu moins sensible en fermeture
                        targetAngle -= pullStrength * Time.deltaTime * openDirection;
                    }

                    // Limiter l'angle entre 0 et maxOpenAngle
                    if (openDirection > 0)
                    {
                        targetAngle = Mathf.Clamp(targetAngle, 0f, maxOpenAngle);
                    }
                    else
                    {
                        targetAngle = Mathf.Clamp(targetAngle, -maxOpenAngle, 0f);
                    }

                    Debug.Log($"Mouvement: {pushForward:F3}, Angle cible: {targetAngle:F1}°");
                }
            }

            playerLastPosition = currentPosition;
            closeTimer = closeDelay; // Reset le timer tant que le joueur est là
        }
    }

    // Appelé quand le joueur sort du trigger
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            playerLastPosition = Vector3.zero;

            if (autoClose)
            {
                closeTimer = closeDelay;
            }

            Debug.Log("Joueur quitte la zone de la porte");
        }
    }

    // Méthodes publiques pour contrôle manuel si besoin
    public void OpenDoor()
    {
        targetAngle = maxOpenAngle * (openDirection != 0 ? openDirection : 1);
        closeTimer = closeDelay;
    }

    public void CloseDoor()
    {
        targetAngle = 0f;
        openDirection = 0;
    }

    // Visualisation dans l'éditeur
    void OnDrawGizmosSelected()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        if (trigger != null && trigger.isTrigger)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(trigger.center, trigger.size);
        }

        // Affiche la direction d'ouverture
        if (doorWing != null)
        {
            // Droite (bleu)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(doorWing.position, doorWing.right * 2f);

            // Gauche (rouge)
            Gizmos.color = Color.red;
            Gizmos.DrawRay(doorWing.position, -doorWing.right * 2f);

            // Forward (vert) - direction de poussée
            Gizmos.color = Color.green;
            Gizmos.DrawRay(doorWing.position, doorWing.forward * 1.5f);

            // Angle max d'ouverture
            Gizmos.color = Color.yellow;
            Vector3 maxOpenRight = Quaternion.Euler(0, maxOpenAngle, 0) * doorWing.forward;
            Vector3 maxOpenLeft = Quaternion.Euler(0, -maxOpenAngle, 0) * doorWing.forward;
            Gizmos.DrawRay(doorWing.position, maxOpenRight * 2f);
            Gizmos.DrawRay(doorWing.position, maxOpenLeft * 2f);
        }
    }
}