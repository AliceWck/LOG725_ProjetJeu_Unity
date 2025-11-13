using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightBattery : MonoBehaviour
{
    [Header("Battery")] // Valeurs batterie max (en sec), valeur actuelle de la batterie
    [SerializeField] private float maxBatteryLife = 240f;
    private float currentBatteryLife;
    [SerializeField] private bool BatteryEnabled = true;

    [Header("Référence au light component")]
    [SerializeField] private Light flashLight; // Lier au composant



    void Start()
    {
        currentBatteryLife = maxBatteryLife;

        // Si la batterie est pas assignée dans l'inspector, essaye de récupérer le composant Light attaché au même GameObject
        if (flashLight == null)
        {
            flashLight = GetComponent<Light>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Vérifie l'état de la lampe ET décharge immédiatement dans la même frame
        if (flashLight != null && BatteryEnabled && flashLight.enabled && currentBatteryLife > 0)
        {
            currentBatteryLife -= Time.deltaTime;

            // Si batterie vide, éteindre la lampe
            if (currentBatteryLife <= 0f)
            {
                currentBatteryLife = 0f;
                flashLight.enabled = false;
            }
        }

        if (GameUIManager.Instance != null && !GameUIManager.Instance.IsOmbreRole)
        {
            GameUIManager.Instance.UpdateLampUI(GetBatteryPercentage());
        }
    }


    // Vérifie si la lampe peut être allumée (batterie > 0)
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
        if (currentBatteryLife > maxBatteryLife) // Laisse batterie au max si ça dépasse
        {
            currentBatteryLife = maxBatteryLife;
        }
    }

    // Peut être plus tard pour un pouvoir, recharge complète
    public void FullRecharge()
    {
        currentBatteryLife = maxBatteryLife;
    }
}