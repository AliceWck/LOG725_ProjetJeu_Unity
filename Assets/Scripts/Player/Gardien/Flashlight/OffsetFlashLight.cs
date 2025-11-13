using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetFlashLight : NetworkBehaviour, ILightSource
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
        batterySystem = GetComponent<FlashlightBattery>(); // Récupération infos batterie
    }

    // Update est appelé une fois par frame
    void Update()
    {
        // Multi : seul le joueur local peut contrôler sa lampe
        if (!isLocalPlayer) return;

        HandleFlashlightToggle();
        HandleVerticalRotation();
    }

    private void HandleFlashlightToggle()
    {
        if (Input.GetKeyDown(KeyCode.F)) // Si appuie sur F
        {
            if (!FlashLight.enabled)
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

    private void HandleVerticalRotation()
    {
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

    public bool IsPlayerInLight(Vector3 playerPosition)
    {
        if (FlashLight == null || !FlashLight.enabled)
            return false;

        float distance = Vector3.Distance(transform.position, playerPosition);

        if (distance > FlashLight.range)
            return false;

        Vector3 directionToPlayer = (playerPosition - transform.position).normalized;
        Vector3 lightDirection = transform.forward;

        float angle = Vector3.Angle(lightDirection, directionToPlayer);

        return angle < (FlashLight.spotAngle / 2f);
    }

    public Vector3 GetLightPosition()
    {
        return transform.position;
    }

    public bool IsGuardianLight()
    {
        return true; // Cette lampe blesse les Ombres
    }
}
