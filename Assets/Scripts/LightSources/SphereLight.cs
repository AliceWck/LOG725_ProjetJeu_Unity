using UnityEngine;

public class SphereLight : MonoBehaviour, ILightSource
{
    public float radius = 5f;

    public bool IsPlayerInLight(Vector3 playerPosition)
    {
        float distance = Vector3.Distance(transform.position, playerPosition);
        return distance <= radius;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}