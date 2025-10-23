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
        SetupLocalPlayer();
    }

    // Définit le nom du joueur
    public void SetPlayerName(string newName)
    {
        if (isServer)
        {
            playerName = newName;
            Debug.Log($"[GamePlayer] Nom défini: {newName}");
        }
    }

    // Hook appelé quand le nom change
    private void OnPlayerNameChanged(string oldName, string newName)
    {
        UpdateNameTag();
    }

    // Met à jour le TextMeshPro affichant le nom
    private void UpdateNameTag()
    {
        if (nameTag != null)
            nameTag.text = playerName;
    }

    // Activation des éléments du joueur local
    private void SetupLocalPlayer()
    {
        Camera cam = GetComponentInChildren<Camera>(true);
        if (cam != null)
        {
            cam.gameObject.SetActive(true);
            Debug.Log("[GamePlayer] Caméra activée pour le joueur local");
        }

        // activer des scripts de mouvement ou changer l'apparence
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            if (rend.material != null)
                rend.material.color = Color.green; // Marque visuelle du joueur local
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        HandleMovement();
    }

    // Exemple simple de mouvement
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * 5f * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}