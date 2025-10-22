using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkSceneLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "MainMenu";
    [SerializeField] private float delayBeforeLoad = 0.1f;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "NetworkSetup")
        {
            Debug.Log("[NetworkSceneLoader] Scène NetworkSetup détectée");

            if (NetworkManager.singleton != null)
            {
                Debug.Log("[NetworkSceneLoader] NetworkManager trouvé, chargement du menu...");
                Invoke(nameof(LoadFirstScene), delayBeforeLoad);
            }
            else
            {
                Debug.LogError("[NetworkSceneLoader] NetworkManager introuvable");
            }
        }
        else
        {
            Debug.Log($"[NetworkSceneLoader] Scène active: {SceneManager.GetActiveScene().name}");
            Destroy(gameObject);
        }
    }

    private void LoadFirstScene()
    {
        if (!string.IsNullOrEmpty(firstSceneName))
            SceneManager.LoadScene(firstSceneName);
        else
            Debug.LogError("[NetworkSceneLoader] Nom de scène vide");
    }
}