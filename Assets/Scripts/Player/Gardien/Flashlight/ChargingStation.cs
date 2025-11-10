using System.Collections;
using UnityEngine;

public class ChargingStation : MonoBehaviour
{
    [Header("Param�tres de la station de recharge")]
    [SerializeField] private float chargingTime = 5f;
    [SerializeField] private float detectionRange = 3f;

    [Header("UI (optionnel)")]
    [SerializeField] private string lancerChargeMessage = "Appuyez sur 'R' pour recharger";
    [SerializeField] private string chargingMessage = "Rechargement...";

    private bool playerInRange = false;
    private bool isCharging = false;
    private float currentChargeAmount = 0f;
    private Coroutine chargingCoroutine;

    private FlashlightBattery flashlightBattery;

    void Update()
    {
        // Trouver la batterie du joueur local
        if (flashlightBattery == null)
        {
            // Chercher dans tous les FlashlightBattery actifs
            FlashlightBattery[] allBatteries = FindObjectsOfType<FlashlightBattery>();

            foreach (var battery in allBatteries)
            {
                // V�rifier si c'est le joueur local
                GamePlayer gp = battery.GetComponentInParent<GamePlayer>();
                if (gp != null && gp.isLocalPlayer && gp.PlayerRole == Role.Gardien)
                {
                    flashlightBattery = battery;
                    Debug.Log("[ChargingStation] Batterie du joueur local trouv�e!");
                    break;
                }
            }

            if (flashlightBattery == null) return; // Pas encore trouv�
        }

        // V�rifier si le joueur est � port�e
        float distance = Vector3.Distance(transform.position, flashlightBattery.transform.position);
        playerInRange = distance <= detectionRange;

            // S'il s'�loigne pendant la recharge, arr�ter
        if (!playerInRange && isCharging)
        {
            StopCharging();
        }

        // Afficher ou non le texte 
        if (!isCharging)
        {
            if (playerInRange)
                GameUIManager.Instance?.ShowChargingMessage(lancerChargeMessage);
            else
                GameUIManager.Instance?.HideChargingMessage();
        }


        // D�clencher la recharge
        if (playerInRange && Input.GetKeyDown(KeyCode.R) && !isCharging)
        {
            chargingCoroutine = StartCoroutine(ChargeBattery());
        }
    }

    IEnumerator ChargeBattery()
    {
        isCharging = true;
        currentChargeAmount = 0f;

        // Afficher le texte de rechargement
        GameUIManager.Instance?.ShowChargingMessage(chargingMessage);

        // Calculer combien de batterie il faut recharger par seconde
        float currentBatteryPercentage = flashlightBattery.GetBatteryPercentage();
        float batteryToRecharge = 100f - currentBatteryPercentage; // % manquants
        float totalSecondsToRecharge = (240 * (batteryToRecharge / 100f)); // converti en secondes de batterie
        float rechargePerSecond = totalSecondsToRecharge / chargingTime;

        while (currentChargeAmount < chargingTime)
        {
            // V�rifier si le joueur s'est �loign�
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
        GameUIManager.Instance?.HideChargingMessage();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}