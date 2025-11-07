using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public Button returnToLobbyButton;

    private void Awake()
    {
        // Setup button listeners
        if (returnToLobbyButton != null)
            returnToLobbyButton.onClick.AddListener(OnReturnToLobbyClicked);

        // Montrer le curseur pour l'UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Récupérer les infos de fin de partie
        bool won = PlayerPrefs.GetInt("GameOver_Win", 0) == 1;
        string subtitle = PlayerPrefs.GetString("GameOver_Subtitle", "");
        ShowGameOver(won, subtitle);
    }

    public void ShowGameOver(bool playerWon, string subtitle = "")
    {
        string title = playerWon ? "GAGNÉ !" : "PERDU !";
        if (titleText != null)
            titleText.text = title;
        if (subtitleText != null)
            subtitleText.text = subtitle;

        gameObject.SetActive(true);
    }

    public void HideInstant()
    {
        gameObject.SetActive(false);
    }

    public void ShowInstant()
    {
        gameObject.SetActive(true);
    }

    private void OnReturnToLobbyClicked()
    {
        Debug.Log("Retour au menu de sélection (quitter la partie)");
        if (CustomNetworkRoomManager.Instance != null)
        {
            CustomNetworkRoomManager.Instance.ReturnToGameSelection();
        }
        else
        {
            SceneManager.LoadScene("GameSelectionMenu");
        }
    }

    private void OnDestroy()
    {
        if (returnToLobbyButton != null)
            returnToLobbyButton.onClick.RemoveListener(OnReturnToLobbyClicked);
    }
}