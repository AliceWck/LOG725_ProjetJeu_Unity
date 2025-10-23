using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideInstant();
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
        StartCoroutine(FadeIn());
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float t = 0;
        while (t < 1f)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            t += Time.unscaledDeltaTime; // unaffected by timeScale
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    public void HideInstant()
    {
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }

    public void ShowInstant()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1;
    }
}