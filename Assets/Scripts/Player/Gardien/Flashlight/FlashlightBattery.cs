using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightBattery : MonoBehaviour
{
    [Header("Battery")] // Valeurs batterie max (en sec), valeur actuelle de la batterie
    [SerializeField] private float maxBatteryLife = 240f;
    private float currentBatteryLife;
    [SerializeField] private bool BatteryEnabled = true;

    [Header("R�f�rence au light component")]
    [SerializeField] private Light flashLight; // Lier au composant

    private bool isFlashlightOn = true;

    void Start()
    {
        currentBatteryLife = maxBatteryLife;

        // Si la batterie est pas assign�e dans l'inspector, essaye de r�cup�rer le composant Light attach� au m�me GameObject
        if (flashLight == null)
        {
            flashLight = GetComponent<Light>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // V�rifie l'�tat de la lampe ET d�charge imm�diatement dans la m�me frame
        if (flashLight != null && BatteryEnabled && flashLight.enabled && currentBatteryLife > 0)
        {
            currentBatteryLife -= Time.deltaTime;

            // Si batterie vide, �teindre la lampe
            if (currentBatteryLife <= 0f)
            {
                currentBatteryLife = 0f;
                flashLight.enabled = false;
            }
        }
    }


    // V�rifie si la lampe peut �tre allum�e (batterie > 0)
    public bool CanTurnOnFlashlight()
    {
        return currentBatteryLife > 0f;
    }

    // Obtenir le pourcentage de batterie restant
    public float GetBatteryPercentage()
    {
        return (currentBatteryLife / maxBatteryLife) * 100f;
    }

    // Recharger la batterie
    public void RechargeBattery(float amount)
    {
        currentBatteryLife += amount;
        if (currentBatteryLife > maxBatteryLife) // Laisse batterie au max si �a d�passe
        {
            currentBatteryLife = maxBatteryLife;
        }
    }

    // Peut �tre plus tard pour un pouvoir, recharge compl�te
    public void FullRecharge()
    {
        currentBatteryLife = maxBatteryLife;
    }
}