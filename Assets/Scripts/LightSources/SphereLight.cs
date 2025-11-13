using UnityEngine;

public class SphereLight : MonoBehaviour, ILightSource
{
    public float radius = 5f;
    private float currentRadius = 5f;
    private NetworkedLampInteraction lamp;

    void Start()
    {
        lamp = GetComponent<NetworkedLampInteraction>();
    }

    void Update()
    {
        // If there is a lamp script → use its state
        if (lamp != null)
        {
            // If lamp is ON → radius = 0
            if (lamp.GetLampState())
            {
                currentRadius = radius;
            }
            else
            {
                currentRadius = 0f;
            }
        }
        else
        {
            // No lamp detected → always use default radius
            currentRadius = radius;
        }
    }

    public bool IsPlayerInLight(Vector3 playerPosition)
    {
        float distance = Vector3.Distance(transform.position, playerPosition);
        return distance <= currentRadius;
    }

    public Vector3 GetLightPosition()
    {
        return transform.position;
    }

    public bool IsGuardianLight()
    {
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentRadius);
    }
#endif
}