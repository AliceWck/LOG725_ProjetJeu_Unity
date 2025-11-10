using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetFlashLight : MonoBehaviour
{
    [Header("Réglages de la lampe")]
    //public GameObject FollowCam; // Camera qui suit le joueur
    [SerializeField] private float MoveSpeed = 13f; // Change vitesse de mouvement de la lampe
    [SerializeField] private float verticalRotationSpeed = 50f; // vitesse de rotation verticale
    [SerializeField] private float minVerticalAngle = -80f; // limite vers le bas
    [SerializeField] private float maxVerticalAngle = 80f;  // limite haut
    
    public Light FlashLight; // Flashlight component

    // Gestion de la batterie
    private FlashlightBattery batterySystem;

    // audio
    public AudioSource Source;
    public AudioClip FlashLightOnSound;
    public AudioClip FlashLightOffSound;

    private float currentVerticalAngle = 0f; // rotation verticale



    // Start appelé avant la première frame update
    void Start()
    {
        //OffsetVect3 = transform.position - FollowCam.transform.position;

        batterySystem = GetComponent<FlashlightBattery>(); // Récupération infos batterie
    }

    // Update est appelé une fois par frame
    void Update()
    {
        //transform.position = FollowCam.transform.position + OffsetVect3; // Light regarde la caméra avec un offset

        //transform.rotation = Quaternion.Slerp(transform.rotation, FollowCam.transform.rotation, Time.deltaTime * MoveSpeed); // Smooth rotation vers la caméra

        if (Input.GetKeyDown(KeyCode.F)) // Si appuie sur F
        {
            if(!FlashLight.enabled)
            {
                // Vérifie si la lampe peut être allumée (batterie > 0)
                if (batterySystem == null || batterySystem.CanTurnOnFlashlight())
                {
                    FlashLight.enabled = true; // Allume la lampe
                    Source.PlayOneShot(FlashLightOnSound); // Son clic allumage lampe
                } 
            }

            else
            {
                FlashLight.enabled = false;
                Source.PlayOneShot(FlashLightOffSound); // Son clic extinction lampe
            }                
        }

        // Contrôle de la molette pour inclinaison verticale du faisceau
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            currentVerticalAngle -= scrollInput * verticalRotationSpeed;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        }

        // Application de la rotation sur la lampe
        Quaternion targetRotation = Quaternion.Euler(currentVerticalAngle - 90f, 0f, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * MoveSpeed);
    }
}
