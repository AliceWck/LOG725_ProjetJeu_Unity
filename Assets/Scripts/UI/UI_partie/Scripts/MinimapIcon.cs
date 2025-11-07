using UnityEngine;
using Mirror;

public class MinimapIcon : NetworkBehaviour
{
    [Header("Icon Settings")]
    [SerializeField] private IconType iconType = IconType.Player;
    [SerializeField] private Color iconColor = Color.white;
    [SerializeField] private float iconSize = 10f;
    [SerializeField] private bool rotateWithObject = false;

    [SerializeField] private float iconHeight = 85f;

    private GameObject iconObject;
    private SpriteRenderer spriteRenderer;

    public enum IconType
    {
        Player,
        Key,
        Objective
    }

    void Start()
    {
        // Création icônes pour objets non joueurs
        if (iconType != IconType.Player)
            TryCreateIcon();
    }

    public override void OnStartLocalPlayer()
    {
        // Créer icône quand c'est le joueur local
        if (iconType == IconType.Player)
            TryCreateIcon();
    }

    void TryCreateIcon()
    {
        if (ShouldCreateIcon())
            CreateMinimapIcon();
    }

    bool ShouldCreateIcon()
    {
        switch (iconType)
        {
            case IconType.Player:
                return isLocalPlayer; // Montre que joueur local
            case IconType.Key:
                return true; // Visible pour tous
            case IconType.Objective:
                return true; // Visible pour tous
            default:
                return false;
        }
    }

    void CreateMinimapIcon()
    {
        // Créer un child GameObject pour l'icône
        iconObject = new GameObject($"MinimapIcon_{iconType}");
        iconObject.transform.SetParent(transform);
        iconObject.transform.localPosition = Vector3.zero;

        // Assignation layer minimap 
        int minimapLayer = LayerMask.NameToLayer("Minimap");
        if (minimapLayer == -1)
        {
            Debug.LogError("Layer 'Minimap' doesn't exist");
            return;
        }
        iconObject.layer = minimapLayer;

        // Ajout SpriteRenderer
        spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateIconSprite();
        spriteRenderer.color = iconColor;
        iconObject.transform.localScale = Vector3.one * iconSize;
        iconObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        Debug.Log($"Minimap icon created for {gameObject.name} on layer {iconObject.layer}");
    }

    Sprite CreateIconSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 3f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = false;

                switch (iconType)
                {
                    case IconType.Player:
                        inside = y > size / 4 && Vector2.Distance(new Vector2(x, y), center) < radius;
                        break;
                    case IconType.Key:
                    case IconType.Objective:
                        inside = Vector2.Distance(new Vector2(x, y), center) < radius;
                        break;
                }

                texture.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void LateUpdate()
    {
        if (iconObject != null)
        {
            // Décalage en hauteur pour être visible sur la minimap
            Vector3 targetPos = transform.position + Vector3.up * iconHeight;
            iconObject.transform.position = targetPos;

            iconObject.transform.localScale = Vector3.one * iconSize;

            if (rotateWithObject)
            {
                Vector3 rotation = transform.eulerAngles;
                iconObject.transform.rotation = Quaternion.Euler(90f, 0f, -rotation.y);
            }
        }
    }

    public void SetColor(Color newColor)
    {
        iconColor = newColor;
        if (spriteRenderer != null)
            spriteRenderer.color = newColor;
    }

    public void SetVisible(bool visible)
    {
        if (iconObject != null)
            iconObject.SetActive(visible);
    }
}
