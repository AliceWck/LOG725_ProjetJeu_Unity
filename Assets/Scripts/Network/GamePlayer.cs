using Mirror;
using UnityEngine;

public class GamePlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string playerName = "Joueur";

    [Header("Références")]
    [SerializeField] private TMPro.TextMeshProUGUI nameTag;

    public string PlayerName => playerName;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Debug.Log($"[GamePlayer] Client démarré - IsLocal: {isLocalPlayer}, Nom: {playerName}");
        
        UpdateNameTag();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"[GamePlayer] Serveur démarré pour {playerName}");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        
        Debug.Log($"[GamePlayer] Joueur local démarré: {playerName}");
        
        // Ici tu peux activer la caméra, les contrôles, etc.
        SetupLocalPlayer();
    }

    /// <summary>
    /// Définit le nom du joueur (appelé par le NetworkRoomManager)
    /// </summary>
    public void SetPlayerName(string newName)
    {
        if (isServer)
        {
            playerName = newName;
            Debug.Log($"[GamePlayer] Nom défini: {newName}");
        }
    }

    /// <summary>
    /// Hook appelé quand le nom change
    /// </summary>
    private void OnPlayerNameChanged(string oldName, string newName)
    {
        UpdateNameTag();
    }

    /// <summary>
    /// Met à jour le tag de nom au-dessus du joueur
    /// </summary>
    private void UpdateNameTag()
    {
        if (nameTag != null)
        {
            nameTag.text = playerName;
        }
    }

    /// <summary>
    /// Configure le joueur local (caméra, contrôles, etc.)
    /// </summary>
    private void SetupLocalPlayer()
    {
        // Active la caméra pour le joueur local
        Camera cam = GetComponentInChildren<Camera>(true);
        if (cam != null)
        {
            cam.gameObject.SetActive(true);
            Debug.Log("[GamePlayer] Caméra activée pour le joueur local");
        }

        // Active les contrôles
        // Par exemple, si tu as un script de mouvement:
        // GetComponent<PlayerMovement>()?.enabled = true;

        // Change la couleur du joueur local pour le distinguer (optionnel)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            if (rend.material != null)
            {
                rend.material.color = Color.green; // Vert pour le joueur local
            }
        }
    }

    private void Update()
    {
        // Tes contrôles de jeu ici
        if (!isLocalPlayer) return;

        // Exemple de mouvement basique (à adapter selon ton jeu)
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Exemple simple - remplace par ta propre logique
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * 5f * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}