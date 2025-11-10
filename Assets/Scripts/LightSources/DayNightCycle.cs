using UnityEngine;

public class DayNightCycleSimple : MonoBehaviour
{
    [Header("Sun Light")]
    [SerializeField] private Light sunLight;

    [Header("Evening Settings (0-33%)")]
    [SerializeField] private Color eveningColor = new Color(1f, 0.8f, 0.6f); // orange doux
    [SerializeField] private float eveningIntensity = 1.2f;

    [Header("Night Settings (33-66%)")]
    [SerializeField] private Color nightColor = new Color(0.5f, 0.6f, 1f); // bleu froid
    [SerializeField] private float nightIntensity = 0.5f;

    [Header("Dawn Settings (66-100%)")]
    [SerializeField] private Color dawnColor = new Color(1f, 0.6f, 0.3f); // orange vif
    [SerializeField] private float dawnIntensity = 1.5f;

    [Header("Animation")]
    [SerializeField] private float transitionSpeed = 2f;

    private Color targetColor;
    private float targetIntensity;

    void Start()
    {
        if (sunLight == null)
            sunLight = GetComponent<Light>();

        if (sunLight == null)
        {
            Debug.LogError("Aucune Light trouvée sur " + gameObject.name);
            enabled = false;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        float progress = GameManager.Instance.GameProgress;

        // Déterminer couleur/intensité cible selon la phase
        if (progress < 33f)
        {
            targetColor = eveningColor;
            targetIntensity = eveningIntensity;
        }
        else if (progress < 66f)
        {
            float t = (progress - 33f) / 33f;
            targetColor = Color.Lerp(eveningColor, nightColor, t);
            targetIntensity = Mathf.Lerp(eveningIntensity, nightIntensity, t);
        }
        else
        {
            float t = (progress - 66f) / 34f;
            targetColor = Color.Lerp(nightColor, dawnColor, t);
            targetIntensity = Mathf.Lerp(nightIntensity, dawnIntensity, t);
        }

        // Appliquer doucement
        sunLight.color = Color.Lerp(sunLight.color, targetColor, Time.deltaTime * transitionSpeed);
        sunLight.intensity = Mathf.Lerp(sunLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
    }
}
