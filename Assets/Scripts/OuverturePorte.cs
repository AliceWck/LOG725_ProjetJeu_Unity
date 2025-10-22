using UnityEngine;

public class OuverturePorte : MonoBehaviour
{
    [Header("Settings")]
    public float openAngle = 90f;          // Angle d'ouverture de la porte
    public float openSpeed = 2f;           // Vitesse d'ouverture
    public float closeDelay = 2f;          // Temps avant fermeture auto
    public bool autoClose = true;          // Fermeture automatique ?
    public float pushForceRequired = 0.5f; // Force de poussée nécessaire

    [Header("References")]
    public Transform doorWing;             // La partie qui tourne (doorWing)
    public Transform doorPivot;            // Le pivot de la porte (généralement doorWing lui-même)

    private bool isOpen = false;
    private bool isOpening = false;
    private float targetAngle = 0f;
    private float closeTimer = 0f;
    private Quaternion closedRotation;
    private int openDirection = 1;         // 1 = vers la droite, -1 = vers la gauche
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
                Debug.LogError("DoorWing introuvable, l'assigner dans l'inspector");
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
        targetAngle = 0f;
    }

    void Update()
    {
        // Animation de la porte
        if (isOpening)
        {
            float currentAngle = doorWing.localEulerAngles.y;
            if (currentAngle > 180f) currentAngle -= 360f;

            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * openSpeed);
            doorWing.localRotation = closedRotation * Quaternion.Euler(0, newAngle, 0);

            // Vérifier si l'animation est terminée
            if (Mathf.Abs(newAngle - targetAngle) < 0.5f)
            {
                isOpening = false;
            }
        }

        // Timer de fermeture auto
        if (isOpen && autoClose && !playerNearby)
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0f)
            {
                CloseDoor();
            }
        }
    }

    // Appelé quand le joueur entre dans le trigger (zone de détection)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            playerLastPosition = other.transform.position;
        }
    }

    // Appelé pendant que le joueur est dans le trigger
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 currentPosition = other.transform.position;
            Vector3 movement = currentPosition - playerLastPosition;

            // Si le joueur bouge assez
            if (movement.magnitude > pushForceRequired * Time.deltaTime)
            {
                // Déterminer la direction d'ouverture selon la position du joueur
                Vector3 toDoor = doorPivot.position - currentPosition;
                Vector3 doorRight = doorPivot.right;

                // Produit scalaire pour savoir de quel côté le joueur pousse
                float side = Vector3.Dot(movement.normalized, doorRight);

                if (Mathf.Abs(side) > 0.3f) // Seuil de détection
                {
                    openDirection = side > 0 ? 1 : -1;
                    OpenDoor();
                }
            }

            playerLastPosition = currentPosition;
        }
    }

    // Appelé quand le joueur sort du trigger
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (autoClose)
            {
                closeTimer = closeDelay;
            }
        }
    }

    // Alternative : Ouvrir avec collision physique (si le joueur "pousse" vraiment)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Déterminer la direction selon la normale de collision
            Vector3 collisionDirection = collision.contacts[0].normal;
            Vector3 doorRight = doorPivot.right;

            float side = Vector3.Dot(collisionDirection, doorRight);
            openDirection = side > 0 ? -1 : 1; // Inversé car la normale pointe vers l'extérieur

            OpenDoor();
        }
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            isOpening = true;
            targetAngle = openAngle * openDirection;
            closeTimer = closeDelay;
            Debug.Log($"Porte ouverte vers {(openDirection > 0 ? "la droite" : "la gauche")} !");
        }
    }

    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            isOpening = true;
            targetAngle = 0f;
            Debug.Log("Porte fermée !");
        }
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
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(doorWing.position, doorWing.right * 2f);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(doorWing.position, -doorWing.right * 2f);
        }
    }
}