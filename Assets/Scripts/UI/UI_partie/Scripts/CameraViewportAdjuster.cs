using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraViewportAdjuster : MonoBehaviour
{
    [Header("UI Top Banner")]
    [SerializeField] private float topBannerHeightPixels = 160f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustViewport();
    }

    void AdjustViewport()
    {
        float screenHeight = Screen.height;
        float topBarPercent = topBannerHeightPixels / screenHeight;

        // commence à 0 en bas, va jusqu'à (1 - topBarPercent) en haut
        cam.rect = new Rect(0, 0, 1, 1 - topBarPercent);

        Debug.Log($"Viewport : Hauteur bandeau = {topBannerHeightPixels}px ({topBarPercent * 100}%)");
    }

    // Recalculer si la résolution change
    void OnRectTransformDimensionsChange()
    {
        if (cam != null)
            AdjustViewport();
    }
}