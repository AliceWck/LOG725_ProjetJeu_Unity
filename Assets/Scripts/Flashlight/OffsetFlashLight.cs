using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetFlashLight : MonoBehaviour
{

    private Vector3 OffsetVect3; // Offset Vector3 entre le joueur et la lampe
    //public GameObject FollowCam; // Camera qui suit le joueur
    [SerializeField] private float MoveSpeed = 13f; // Change vitesse de mouvement de la lampe
    public Light FlashLight; // Flashlight component

    // Gestion de la batterie
    private FlashlightBattery batterySystem;

    // audio
    public AudioSource Source;
    public AudioClip FlashLightOnSound;
    public AudioClip FlashLightOffSound;



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
    }
}
