using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Mirror;

public class GameSelectionMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField joinInputField;
    
    [Header("Scene Configuration")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string lobbySceneName = "Lobby";
    
    private GameRoomValidator roomValidator;
    private SceneLoader sceneLoader;

    private void Awake()
    {
        roomValidator = new GameRoomValidator();
        sceneLoader = new SceneLoader();
    }

    public void OnBackButton()
    {
        if (NetworkClient.isConnected || NetworkServer.active)
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
            }
            else if (NetworkServer.active)
            {
                NetworkManager.singleton.StopServer();
            }
        }
        
        sceneLoader.LoadScene(mainMenuSceneName);
    }

    public void OnCreateButton()
    {
        Debug.Log("[GameSelectionMenu] Création de la partie...");
        
        if (NetworkManager.singleton == null)
        {
            Debug.LogError("[GameSelectionMenu] NetworkManager introuvable !");
            return;
        }

        NetworkManager.singleton.maxConnections = 5;
        NetworkManager.singleton.StartHost();
        
        Debug.Log("[GameSelectionMenu] Serveur démarré (Host) - En attente de joueurs");
    }

    public void OnJoinButton()
    {
        string ipAddress = joinInputField.text.Trim();
        
        if (!roomValidator.IsValid(ipAddress))
        {
            Debug.LogWarning("[GameSelectionMenu] L'adresse IP est invalide !");
            return;
        }

        Debug.Log($"[GameSelectionMenu] Connexion à : {ipAddress}");
        
        if (NetworkManager.singleton == null)
        {
            Debug.LogError("[GameSelectionMenu] NetworkManager introuvable !");
            return;
        }

        NetworkManager.singleton.networkAddress = ipAddress;
        NetworkManager.singleton.StartClient();
        
        Debug.Log("[GameSelectionMenu] Connexion en tant que client...");
    }
}

public class GameRoomValidator
{
    private const int MIN_ROOM_CODE_LENGTH = 7;
    private const int MAX_ROOM_CODE_LENGTH = 15;

    public bool IsValid(string roomCode)
    {
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("[GameRoomValidator] Le code de partie est vide.");
            return false;
        }

        if (roomCode.Length < MIN_ROOM_CODE_LENGTH || roomCode.Length > MAX_ROOM_CODE_LENGTH)
        {
            Debug.LogWarning($"[GameRoomValidator] Le code doit contenir entre {MIN_ROOM_CODE_LENGTH} et {MAX_ROOM_CODE_LENGTH} caractères (ex: 192.168.1.100).");
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