using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                // Le joueur � suivre

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -5f);  // Position relative � la cible
    public float smoothSpeed = 10f;         // Vitesse de suivi (plus �lev� = plus rapide)
    public bool lookAtTarget = true;        // La cam�ra regarde toujours le joueur ?

    [Header("Optional - Mouse Rotation")]
    public bool enableMouseRotation = false;
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    private float currentX = 0f;
    private float currentY = 0f;

    void Start()
    {
        // Si la target n'est pas assign�e, chercher le Player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("Target automatiquement trouv�e : " + player.name);
            }
            else
            {
                Debug.LogError("Aucune target assign�e et aucun objet avec tag 'Player' trouv� !");
            }
        }

        // Initialiser les angles de rotation
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Position cible
        Vector3 desiredPosition;

        if (enableMouseRotation)
        {
            // Rotation avec la souris
            currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            desiredPosition = target.position + rotation * offset;

            // D�placer la cam�ra
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
        else
        {
            // Position fixe relative au joueur
            desiredPosition = target.position + offset;

            // D�placer la cam�ra avec interpolation
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Regarder le joueur
            if (lookAtTarget)
            {
                Vector3 lookPosition = target.position + Vector3.up * 1.5f; // Regarde un peu au-dessus du joueur
                transform.LookAt(lookPosition);
            }
        }
    }

    // Visualisation dans l'�diteur
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Ligne vers la target
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target.position);

        // Position de l'offset
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position + offset, 0.3f);
    }
}