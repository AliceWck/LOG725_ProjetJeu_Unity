using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanneauStation : MonoBehaviour
{
    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Faire face à la caméra principale
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}
