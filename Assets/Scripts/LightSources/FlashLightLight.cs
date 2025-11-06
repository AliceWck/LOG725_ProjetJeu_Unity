using UnityEngine;

public class FlashLightLight : MonoBehaviour, ILightSource
{
    [Header("Flashlight Settings")]
    public float range = 10f;
    [Range(1f, 179f)]
    public float spotAngle = 30f;

    public bool IsPlayerInLight(Vector3 playerPosition)
    {
        Vector3 toPlayer = playerPosition - transform.position;

        // Flatten both vectors so Y (height) doesn’t matter as much
        Vector3 forwardFlat = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 toPlayerFlat = new Vector3(toPlayer.x, 0f, toPlayer.z).normalized;

        float distanceToPlayer = new Vector3(toPlayer.x, 0f, toPlayer.z).magnitude;
        if (distanceToPlayer > range)
            return false;

        float angleToPlayer = Vector3.Angle(forwardFlat, toPlayerFlat);
        return angleToPlayer <= spotAngle / 2f;
    }

    public bool IsGuardianLight()
    {
        return true;
    }

    public Vector3 GetLightPosition()
    {
        return transform.position;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * range);

        // Visualize horizontal cone (XZ only)
        UnityEditor.Handles.color = new Color(1f, 1f, 0f, 0.2f);
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up,
            Quaternion.Euler(0, -spotAngle / 2f, 0) * transform.forward,
            spotAngle, range);
    }

#endif
}