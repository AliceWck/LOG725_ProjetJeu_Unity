using UI.MainMenu;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private SettingsPanelManager settingsPanel;
    [SerializeField] private RulesPanelManager rulesPanel;
    public void PlayGame()
    {
        Debug.Log("Jouer au jeu");
        SceneManager.LoadScene("GameSelectionMenu");
    }

    public void OpenSettings()
    {
        Debug.Log("Ouvrir les paramètres");
        if (settingsPanel != null) {
            settingsPanel.Show();
        }
        else {
            Debug.LogWarning("SettingsPanel non assigné dans l'inspector");
        }
    }

    public void OpenRules()
    {
        Debug.Log("Ouvrir les règles");
        if (rulesPanel != null)
        {
            rulesPanel.Show();
        }
        else
        {
            Debug.LogWarning("RulesPanel non assigné dans l'inspector");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
