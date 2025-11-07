using UnityEngine;

public class ChatAI : MonoBehaviour
{
    public float speed = 2f;          // vitesse de déplacement du Chat
    private Transform target;         // cible actuelle (Player Ombre)

    void OnTriggerEnter(Collider other)
    {
        ShadowPlayer shadow = other.GetComponent<ShadowPlayer>();
        if (shadow != null)
        {
            target = other.transform;

            // Jouer un bruit
            GetComponent<AudioSource>()?.Play();

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
            Debug.Log("[ChatAI] Ombre hors de portée → Chat s’arrête.");
        }
    }

    void Update()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );

            // Debug message
            Debug.Log("[ChatAI] Chat poursuit l’ombre. Position actuelle : " + transform.position);
        }
        else
        {
            // Debug message
            Debug.Log("[ChatAI] Chat est immobile, aucune cible.");
        }
    }
}