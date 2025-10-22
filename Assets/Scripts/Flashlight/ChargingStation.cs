using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChargingStation : MonoBehaviour
{
    [Header("Param�tres de la station de recharge")]
    [SerializeField] private float chargingTime = 100f; // Temps n�cessaire pour une recharge compl�te
    [SerializeField] private float detectionRange = 3f; // distance � laquelle le joueur peut interagir

    [Header("R�f�rences")]
    [SerializeField] private Transform playerTransform; // R�f�rence au transform du joueur
    [SerializeField] private FlashlightBattery flashlightBattery; // R�f�rence � la lampe de poche du joueur

    [Header("UI (optionnel)")]
    [SerializeField] private TextMeshProUGUI chargingText; // Texte affich� pendant la recharge
    [SerializeField] private string lancerChargeMessage = "Appuyez sur 'R' pour recharger";
    [SerializeField] private string chargingMessage = "Rechargement...";

    private bool playerInRange = false;
    private bool isCharging = false;
    private float currentChargeAmount = 0f;
    private Coroutine chargingCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        // Si pas assign�, recherce auto
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
        // Si joueur � port�e
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            playerInRange = distance <= detectionRange;

            // S'il s'�loigne pendant la recharge, arr�ter
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

        // Si le joueur appuie sur R et est � port�e 
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
            // V�rifie si le joueur a rallum� sa lampe ou s'est �loign�
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

        // Recharge compl�te � la fin
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