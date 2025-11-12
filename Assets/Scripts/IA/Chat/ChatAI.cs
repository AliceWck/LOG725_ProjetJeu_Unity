using UnityEngine;

public class ChatAI : MonoBehaviour
{
    public float speed = 2f;          // vitesse de déplacement du Chat
    public float stopDistance = 1.5f; // distance minimale avant de s'arrêter
    public float rotationSpeed = 5f;  // vitesse de rotation du chat
    private Transform target;         // cible actuelle (Player Ombre)
    private float soundTimer = 0f;    // chronomètre pour le son
    private float soundInterval = 3f; // intervalle entre les sons (3 secondes)
    private Rigidbody rb;             // composant Rigidbody

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Empêcher le chat de tomber
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        ShadowPlayer shadow = other.GetComponent<ShadowPlayer>();
        if (shadow != null)
        {
            target = other.transform;

            // Jouer un bruit
            GetComponent<AudioSource>()?.Play();

            // Réinitialiser le chronomètre
            soundTimer = 0f;

            // Debug message
            Debug.Log("[ChatAI] Ombre détectée → Chat commence la poursuite !");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ShadowPlayer>() != null)
        {
            target = null;

            // Debug message
            Debug.Log("[ChatAI] Ombre hors de portée → Chat s'arrête.");
        }
    }

    void Update()
    {
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // Vérifier si le chat n'est pas trop proche
            if (distanceToTarget > stopDistance)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target.position,
                    speed * Time.deltaTime
                );

                // Faire tourner le chat vers la cible
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Gérer le son toutes les 3 secondes
            soundTimer += Time.deltaTime;
            if (soundTimer >= soundInterval)
            {
                GetComponent<AudioSource>()?.Play();
                soundTimer = 0f;
            }

            // Debug message
            Debug.Log("[ChatAI] Chat poursuit l'ombre. Position actuelle : " + transform.position);
        }
        else
        {
            // Debug message
            //Debug.Log("[ChatAI] Chat est immobile, aucune cible.");
        }
    }
}