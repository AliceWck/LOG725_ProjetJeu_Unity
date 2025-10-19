using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Jouer au jeu");
        SceneManager.LoadScene("OutdoorsScene");
    }

    public void OpenSettings()
    {
        Debug.Log("Ouvrir les paramètres");
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
