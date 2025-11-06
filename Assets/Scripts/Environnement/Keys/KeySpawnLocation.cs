using UnityEngine;

public class KeySpawnLocation : MonoBehaviour
{
    public float radius = 1f;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}