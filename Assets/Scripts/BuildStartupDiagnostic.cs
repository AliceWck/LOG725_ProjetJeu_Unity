using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// Script de diagnostic qui s'exécute AVANT tout autre script
/// Permet de diagnostiquer les problèmes de chargement de scènes dans les builds
/// DÉSACTIVÉ - Build fonctionne correctement. Décommentez pour déboguer.
/// </summary>
public static class BuildStartupDiagnostic
{
    // Logs désactivés - Décommentez les méthodes ci-dessous pour réactiver le diagnostic

    /*
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        Debug.Log("========== BUILD STARTUP DIAGNOSTIC ==========");
        Debug.Log($"Nombre total de scènes dans Build Settings: {SceneManager.sceneCountInBuildSettings}");

        Debug.Log("\n----- Liste des scènes dans Build Settings -----");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"  [{i}] {sceneName} ({scenePath})");
        }

        Debug.Log("\n----- Vérifications critiques -----");
        CheckSceneExists("NetworkSetup");
        CheckSceneExists("MainMenu");
        CheckSceneExists("Lobby");
        CheckSceneExists("OutdoorsScene");

        Debug.Log("================================================\n");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterSceneLoad()
    {
        Debug.Log($"[BuildDiagnostic] Scène chargée: {SceneManager.GetActiveScene().name} (buildIndex: {SceneManager.GetActiveScene().buildIndex})");

        if (SceneManager.GetActiveScene().name == "NetworkSetup")
        {
            Debug.Log("[BuildDiagnostic] Dans NetworkSetup - Recherche de CustomNetworkRoomManager...");
            var networkManager = Object.FindObjectOfType<CustomNetworkRoomManager>();
            if (networkManager != null)
            {
                Debug.Log("[BuildDiagnostic] ✓ CustomNetworkRoomManager trouvé");
                Debug.Log($"[BuildDiagnostic]   GameObject actif: {networkManager.gameObject.activeInHierarchy}");
                Debug.Log($"[BuildDiagnostic]   Auto Load Main Menu: {networkManager.AutoLoadMainMenu}");
            }
            else
            {
                Debug.LogError("[BuildDiagnostic] ❌ CustomNetworkRoomManager NON trouvé dans NetworkSetup!");
            }
        }
    }

    private static void CheckSceneExists(string sceneName)
    {
        bool found = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (System.IO.Path.GetFileNameWithoutExtension(scenePath) == sceneName)
            {
                Debug.Log($"✅ Scène '{sceneName}' trouvée à l'index {i}");
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogError($"❌ Scène '{sceneName}' NON trouvée dans Build Settings!");
        }
    }
    */
}