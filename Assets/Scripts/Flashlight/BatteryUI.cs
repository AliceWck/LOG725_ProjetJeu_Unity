using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BatteryUI : MonoBehaviour
{
    [Header("Battery UI Elements")]
    [SerializeField] private Scrollbar batteryScrollbar; // Lier au scrollbar UI�
    [SerializeField] private TextMeshProUGUI batteryText; // Pourcentage batterie

    [Header("R�f�rence batterie lampe de poche")]
    [SerializeField] private FlashlightBattery flashlightBattery; // Lier au script batterie lampe de poche

    [Header("Couleurs scrollbar")]
    [SerializeField] private bool useColorGradient = true;
    [SerializeField] private Color fullBatteryColor = Color.green;
    [SerializeField] private Color lowBatteryColor = Color.yellow;
    [SerializeField] private Color emptyBatteryColor = Color.red;
    [SerializeField] private float lowBatterySeuil = 0.3f; // Seuil pour changer de couleur � jaune

    private Image scrollbarRemplissage;

    // Start is called before the first frame update
    void Start()
    {
        // R�cup�ration de l'image de remplissage du scrollbar
        if (batteryScrollbar != null && batteryScrollbar.handleRect != null)
        {
            scrollbarRemplissage = batteryScrollbar.handleRect.GetComponent<Image>();
        }

        // Cas o� image pas assign�e, cherche dans la sc�ne
        if (flashlightBattery == null)
        {
            flashlightBattery = FindObjectOfType<FlashlightBattery>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (flashlightBattery != null)
        {
            float batteryPercent = flashlightBattery.GetBatteryPercentage();
            float batteryNormalized = batteryPercent / 100f;

            // M�j scrollbar
            if (batteryScrollbar != null)
            {
                batteryScrollbar.size = batteryNormalized;

                // Changement couleur scrollbar selon niveau batterie si image existe et option activ�e
                if (useColorGradient && scrollbarRemplissage != null)
                {
                    if (batteryNormalized <= 0) // Plus de batterie
                    {
                        scrollbarRemplissage.color = emptyBatteryColor;
                    }
                    else if (batteryNormalized <= lowBatterySeuil) // Batterie faible
                    {
                        scrollbarRemplissage.color = Color.Lerp(emptyBatteryColor, lowBatteryColor, batteryNormalized / lowBatterySeuil); // Interpolation entre rouge et jaune
                    }
                    else // Batterie ok
                    {
                        scrollbarRemplissage.color = Color.Lerp(lowBatteryColor, fullBatteryColor, (batteryNormalized - lowBatterySeuil) / (1f - lowBatterySeuil)); // Interpolation entre jaune et vert
                    }
                }
            }

            // M�j texte pourcentage si existe
            if (batteryText != null)
            {
                batteryText.text = Mathf.RoundToInt(batteryPercent).ToString() + "%";
            }
        }
    }
}
