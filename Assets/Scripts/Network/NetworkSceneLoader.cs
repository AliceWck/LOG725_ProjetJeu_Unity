using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "MainMenu";
    [SerializeField] private float delayBeforeLoad = 0.1f;

    private void Start()
    {
        Invoke(nameof(LoadFirstScene), delayBeforeLoad);
    }

    private void LoadFirstScene()
    {
        if (!string.IsNullOrEmpty(firstSceneName))
        {
            Debug.Log($"[NetworkSceneLoader] Chargement de : {firstSceneName}");
            SceneManager.LoadScene(firstSceneName);
        }
    }
}