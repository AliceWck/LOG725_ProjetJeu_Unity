using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChargingStation : MonoBehaviour
{
    [Header("Paramètres de la station de recharge")]
    [SerializeField] private float chargingTime = 100f; // Temps nécessaire pour une recharge complète
    [SerializeField] private float detectionRange = 3f; // distance à laquelle le joueur peut interagir

    [Header("Références")]
    [SerializeField] private Transform playerTransform; // Référence au transform du joueur
    [SerializeField] private FlashlightBattery flashlightBattery; // Référence à la lampe de poche du joueur

    [Header("UI (optionnel)")]
    [SerializeField] private TextMeshProUGUI chargingText; // Texte affiché pendant la recharge
    [SerializeField] private string lancerChargeMessage = "Appuyez sur 'R' pour recharger";
    [SerializeField] private string chargingMessage = "Rechargement...";

    private bool playerInRange = false;
    private bool isCharging = false;
    private float currentChargeAmount = 0f;
    private Coroutine chargingCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        // Si pas assigné, recherce auto
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (flashlightBattery == null)
        {
            flashlightBattery = FindObjectOfType<FlashlightBattery>();
        }

        if (chargingText != null)
        {
            chargingText.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Si joueur à portée
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            playerInRange = distance <= detectionRange;

            // S'il s'éloigne pendant la recharge, arrêter
            if (!playerInRange && isCharging)
            {
                StopCharging();
            }
        }

        // Afficher ou non le texte 
        if (chargingText != null && !isCharging)
        {
            chargingText.text = lancerChargeMessage;
            chargingText.gameObject.SetActive(playerInRange);
        }

        // Si le joueur appuie sur R et est à portée 
        if (playerInRange && Input.GetKeyDown(KeyCode.R) && !isCharging && flashlightBattery!= null)
        {
            chargingCoroutine = StartCoroutine(ChargeBattery());
        }
    }

    IEnumerator ChargeBattery()
    {
        isCharging = true;
        currentChargeAmount = 0f;

        // Afficher le texte de rechargement
        if (chargingText != null)
        {
            chargingText.text = chargingMessage;
            chargingText.gameObject.SetActive(true);
        }

        // Calculer combien de batterie il faut recharger par seconde
        float currentBatteryPercentage = flashlightBattery.GetBatteryPercentage();
        float batteryToRecharge = 100f - currentBatteryPercentage; // % manquants
        float totalSecondsToRecharge = (240 * (batteryToRecharge / 100f)); // converti en secondes de batterie
        float rechargePerSecond = totalSecondsToRecharge / chargingTime;

        while (currentChargeAmount < chargingTime)
        {
            // Vérifie si le joueur a rallumé sa lampe ou s'est éloigné
            if (!playerInRange)
            {
                StopCharging();
                yield break;
            }

            float rechargeThisFrame = rechargePerSecond * Time.deltaTime;

            if (flashlightBattery != null)
            {
                flashlightBattery.RechargeBattery(rechargeThisFrame);
            }

            currentChargeAmount += Time.deltaTime;
            yield return null;
        }

        // Recharge complète à la fin
        if (flashlightBattery != null)
        {
            flashlightBattery.FullRecharge();
        }

        // Termine la recharge
        EndCharging();
    }

    void StopCharging()
    {
        if (chargingCoroutine != null)
        {
            StopCoroutine(chargingCoroutine);
            chargingCoroutine = null;
        }
        EndCharging();
    }

    void EndCharging()
    {
        isCharging = false;

        // Cacher le texte de rechargement
        if (chargingText != null)
        {
            chargingText.gameObject.SetActive(false);
        }
    }
}