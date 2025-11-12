using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                // Le joueur � suivre

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -5f);  // Position relative � la cible
    public float smoothSpeed = 10f;         // Vitesse de suivi (plus �lev� = plus rapide)
    public bool lookAtTarget = true;        // La cam�ra regarde toujours le joueur ?

    [Header("Collision Settings")]
    public LayerMask collisionLayers;       // Les layers qui bloquent la cam�ra (murs, etc)
    public float cameraRadius = 0.3f;       // Rayon de la "sph�re" de la cam�ra
    public float minDistance = 0.5f;        // Distance minimale au joueur
    public float collisionSmoothSpeed = 15f; // Vitesse d'ajustement lors de collision


    [Header("Optional - Mouse Rotation")]
    public bool enableMouseRotation = false;
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    private float currentX = 0f;
    private float currentY = 0f;
    private float currentDistance;

    void Start()
    {
        // Si la target n'est pas assignée, chercher le joueur local via GamePlayer
        if (target == null)
        {
            // Méthode 1: Chercher via GamePlayer.isLocalPlayer
            GamePlayer[] allPlayers = FindObjectsOfType<GamePlayer>();
            foreach (var player in allPlayers)
            {
                if (player.isLocalPlayer || player.isOwned)
                {
                    target = player.transform;
                    Debug.Log($"[ThirdPersonCamera] Target trouvée via GamePlayer: {player.name}");
                    break;
                }
            }
            
            // Méthode 2: Si toujours rien, chercher par tag
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                    Debug.Log("[ThirdPersonCamera] Target trouvée via tag Player: " + player.name);
                }
            }
            
            if (target == null)
            {
                Debug.LogWarning("[ThirdPersonCamera] ⚠️ Aucune target trouvée! Caméra inactive.");
            }
        }

        // Initialiser les angles de rotation
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        // Distance initiale
        currentDistance = offset.magnitude;

        // Si collisionLayers n'est pas configur�, utiliser tout sauf le joueur
        if (collisionLayers.value == 0)
        {
            collisionLayers = ~(1 << LayerMask.NameToLayer("Player"));
            Debug.LogWarning("collisionLayers non configur�, utilise tous les layers sauf Player");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Position cible d�sir�e
        Vector3 desiredPosition;
        Vector3 targetPosition = target.position + Vector3.up * 1.5f; // Point de focus

        if (enableMouseRotation)
        {
            // Rotation avec la souris
            currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 direction = rotation * offset.normalized;

            // V�rifier les collisions
            float targetDistance = offset.magnitude;
            float adjustedDistance = CheckCameraCollision(targetPosition, direction, targetDistance);

            desiredPosition = targetPosition + direction * adjustedDistance;
        }
        else
        {
            // Position fixe relative au joueur
            Vector3 direction = offset.normalized;
            float targetDistance = offset.magnitude;

            // V�rifier les collisions
            float adjustedDistance = CheckCameraCollision(targetPosition, direction, targetDistance);

            desiredPosition = targetPosition + direction * adjustedDistance;
        }

        // D�placer la cam�ra avec interpolation
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Regarder le joueur
        if (lookAtTarget)
        {
            transform.LookAt(targetPosition);
        }
    }

    float CheckCameraCollision(Vector3 targetPos, Vector3 direction, float desiredDistance)
    {
        RaycastHit hit;

        // Lancer un SphereCast depuis le joueur vers la position de cam�ra
        if (Physics.SphereCast(
            targetPos,
            cameraRadius,
            direction,
            out hit,
            desiredDistance,
            collisionLayers))
        {
            // Obstacle = rapprocher la cam�ra
            float safeDistance = Mathf.Max(hit.distance - cameraRadius, minDistance);

            // Smooth transition de la distance
            currentDistance = Mathf.Lerp(currentDistance, safeDistance, collisionSmoothSpeed * Time.deltaTime);
            return currentDistance;
        }
        else
        {
            // Pas d'obstacle, revenir progressivement � la distance d�sir�e
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, collisionSmoothSpeed * Time.deltaTime);
            return currentDistance;
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