using UnityEngine;

public class MinimapSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameUIManager uiManager;
    [SerializeField] private Transform playerTransform;

    [Header("Minimap Camera Settings")]
    [SerializeField] private float cameraHeight = 50f;
    [SerializeField] private float orthographicSize = 20f;
    [SerializeField] private LayerMask minimapLayers; // Savoir ce que montre la caméra de la minimap, mais un peu obsolète avec le format de duplication de la map


    private Camera minimapCamera;
    private RenderTexture minimapRenderTexture;
    private GameObject minimapCameraObject;



    void Start()
    {
        SetupMinimapCamera();
        SetupRenderTexture();

        if (uiManager != null)
        {
            uiManager.SetMiniMapTexture(minimapRenderTexture);
        }
        else
        {
            Debug.LogWarning("GameUIManager non assigné sur MinimapSystem");
        }
    }

    void SetupMinimapCamera()
    {
        // Créer un GameObject pour la caméra minimap
        minimapCameraObject = new GameObject("MinimapCamera");
        minimapCameraObject.transform.SetParent(transform);

        // Ajout et config la caméra
        minimapCamera = minimapCameraObject.AddComponent<Camera>();
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = orthographicSize;
        minimapCamera.cullingMask = minimapLayers;
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = new Color(0.1f, 0.15f, 0.2f); // Fond gris-bleu foncé
        minimapCamera.depth = -10; // Render avant la caméra principale

        // Rotation pour vue de dessus
        minimapCameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Debug.Log("Caméra minimap créée et configurée");
    }

    private void SetupRenderTexture()
    {
        minimapRenderTexture = new RenderTexture(256, 256, 16);
        minimapRenderTexture.name = "MinimapRenderTexture";
        minimapCamera.targetTexture = minimapRenderTexture;// Assigner à la caméra
    }


    void LateUpdate()
    {
        // Suivre le joueur si assigné
        if (playerTransform != null && minimapCameraObject != null)
        {
            Vector3 newPosition = playerTransform.position;
            newPosition.y = cameraHeight; // Hauteur fixe au-dessus du joueur
            minimapCameraObject.transform.position = newPosition;
        }
    }

    // Méthode publique pour changer le joueur suivi
    public void SetPlayerToFollow(Transform player)
    {
        playerTransform = player;
        Debug.Log($"Minimap suit maintenant: {player.name}");
    }

    // Ajuster le zoom de la minimap
    public void SetZoom(float zoom)
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = zoom;
        }
    }

    void OnDestroy()
    {
        // Nettoyer le RenderTexture
        if (minimapRenderTexture != null)
        {
            minimapRenderTexture.Release();
            Destroy(minimapRenderTexture);
        }
    }

    #region Gizmos (pour debug dans la Scene view)

    void OnDrawGizmos()
    {
        if (minimapCameraObject != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 pos = minimapCameraObject.transform.position;

            // Dessiner la zone de vue de la minimap
            float size = orthographicSize;
            Gizmos.DrawWireCube(pos, new Vector3(size * 2, 0.1f, size * 2));
        }
    }

    #endregion
}