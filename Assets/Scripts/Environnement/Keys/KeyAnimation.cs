using UnityEngine;

public class KeyAnimation: MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverAmplitude = 0.25f; // height of the bob
    [SerializeField] private float hoverSpeed = 2f;        // speed of up/down movement

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 50f;    // degrees per second

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Up and down movement (hover)
        float newY = startPos.y + Mathf.Cos(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // Rotation around Y axis
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}