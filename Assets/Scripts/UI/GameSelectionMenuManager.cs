using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSelectionMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField joinInputField;
    
    [Header("Scene Configuration")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "GameScene";
    
    private GameRoomValidator roomValidator;
    private SceneLoader sceneLoader;

    private void Awake()
    {
        roomValidator = new GameRoomValidator();
        sceneLoader = new SceneLoader();
    }

    public void OnBackButton()
    {
        sceneLoader.LoadScene(mainMenuSceneName);
    }

    public void OnCreateButton()
    {
        Debug.Log("[GameSelectionMenu] Création de la partie...");
        // TODO: Ajouter la logique réseau
        // sceneLoader.LoadScene(gameSceneName);
    }

    public void OnJoinButton()
    {
        string roomCode = joinInputField.text;
        
        if (roomValidator.IsValid(roomCode))
        {
            Debug.Log($"[GameSelectionMenu] Rejoindre la partie : {roomCode}");
            // TODO: Ajouter la logique réseau pour rejoindre la partie
            // sceneLoader.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("[GameSelectionMenu] Le code de partie est invalide !");
        }
    }
}

public class GameRoomValidator
{
    private const int MIN_ROOM_CODE_LENGTH = 4;
    private const int MAX_ROOM_CODE_LENGTH = 8;

    public bool IsValid(string roomCode)
    {
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("[GameRoomValidator] Le code de partie est vide.");
            return false;
        }

        if (roomCode.Length < MIN_ROOM_CODE_LENGTH || roomCode.Length > MAX_ROOM_CODE_LENGTH)
        {
            Debug.LogWarning($"[GameRoomValidator] Le code doit contenir entre {MIN_ROOM_CODE_LENGTH} et {MAX_ROOM_CODE_LENGTH} caractères.");
            return false;
        }

        return true;
    }
}

public class SceneLoader
{
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] Le nom de la scène est vide !");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log($"[SceneLoader] Chargement de la scène : {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[SceneLoader] La scène '{sceneName}' n'existe pas dans les Build Settings !");
        }
    }

    public void LoadSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] Le nom de la scène est vide !");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log($"[SceneLoader] Chargement asynchrone de la scène : {sceneName}");
            SceneManager.LoadSceneAsync(sceneName);
        }
        else
        {
            Debug.LogError($"[SceneLoader] La scène '{sceneName}' n'existe pas dans les Build Settings !");
        }
    }
}
