using UnityEngine;

public class CatController : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Calcul de la vitesse horizontale
        float speed = rb.velocity.magnitude;

        // Envoi au paramètre Animator
        animator.SetFloat("Speed", speed);
    }
}