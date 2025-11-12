using Mirror;
using UnityEngine;
using System.Collections.Generic;


public class GamePlayer : NetworkBehaviour
{
    public static readonly List<GamePlayer> allPlayers = new List<GamePlayer>();

    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string playerName = "Joueur";

    [SyncVar(hook = nameof(OnRoleChanged))]
    private Role role = Role.Ombre;

    [Header("Références")]
    [SerializeField] private TMPro.TextMeshProUGUI nameTag;

    public string PlayerName => playerName;
    public Role PlayerRole => role;

    private GameUIManager uiManager;


    // Ajout gestion Ui liste joueurs
    public override void OnStartClient()
    {
        base.OnStartClient();
        allPlayers.Add(this);
        Debug.Log($"[GamePlayer] Client démarré - IsLocal: {isLocalPlayer}, IsOwned: {isOwned}, Nom: {playerName}, Rôle: {role}");
        UpdateNameTag();

        GameUIManager ui = FindObjectOfType<GameUIManager>();
        ui?.RefreshPlayersList();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        allPlayers.Remove(this);
        Debug.Log($"[GamePlayer] - Retiré de la liste : {playerName}");

        GameUIManager ui = FindObjectOfType<GameUIManager>();
        ui?.RefreshPlayersList();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"[GamePlayer] Serveur démarré pour {playerName}");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"[GamePlayer] ✓ Joueur local démarré: {playerName}, Rôle: {role}, IsOwned: {isOwned}");

        uiManager = FindObjectOfType<GameUIManager>();
        if (uiManager != null)
        {
            uiManager.SetPlayerRole(role == Role.Ombre);
            uiManager.SetPlayerHealth(100f);
            uiManager.RefreshPlayersList(); // Rafraîchir la liste pour afficher "(Vous)"
        }

        SetupLocalPlayer();

        // Forcer la configuration des composants selon le rôle
        ConfigureRoleComponents();

        // Connexion auto minimap
        MinimapSystem minimap = FindObjectOfType<MinimapSystem>();
        if (minimap != null)
        {
            minimap.SetPlayerToFollow(transform);
        }
    }

    /// <summary>
    /// Appelé quand ce client reçoit l'autorité
    /// Alternative à OnStartLocalPlayer en cas de problème
    /// </summary>
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log($"[GamePlayer] ✓ AUTORITÉ REÇUE - {playerName}, IsLocal: {isLocalPlayer}, IsOwned: {isOwned}");

        // Si isLocalPlayer est false mais on a l'autorité, c'est qu'on est le Host
        // Forcer le setup du joueur local
        if (!isLocalPlayer)
        {
            Debug.LogWarning($"[GamePlayer] ⚠️ Autorité sans isLocalPlayer - Mode Host détecté, forçage setup");

            uiManager = FindObjectOfType<GameUIManager>();
            if (uiManager != null)
            {
                uiManager.SetPlayerRole(role == Role.Ombre);
                uiManager.SetPlayerHealth(100f);
                uiManager.RefreshPlayersList(); // Rafraîchir la liste pour afficher "(Vous)"
            }

            SetupLocalPlayer();
            ConfigureRoleComponents();

            MinimapSystem minimap = FindObjectOfType<MinimapSystem>();
            if (minimap != null)
            {
                minimap.SetPlayerToFollow(transform);
            }
        }
    }

    // Hook appelé quand le nom change
    private void OnPlayerNameChanged(string oldName, string newName)
    {
        UpdateNameTag();
    }

    // Hook appelé quand le rôle change
    private void OnRoleChanged(Role oldRole, Role newRole)
    {
        Debug.Log($"[GamePlayer] Rôle changé: {oldRole} → {newRole}");
        ConfigureRoleComponents();
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

    // Définit le rôle du joueur
    public void SetPlayerRole(Role newRole)
    {
        if (isServer)
        {
            role = newRole;
            Debug.Log($"[GamePlayer] Rôle défini: {newRole}");
        }
    }

    // Met à jour le TextMeshPro affichant le nom
    private void UpdateNameTag()
    {
        if (nameTag != null)
            nameTag.text = playerName;
    }

    // Configure les composants selon le rôle
    private void ConfigureRoleComponents()
    {
        Debug.Log($"[GamePlayer] ConfigureRoleComponents appelé - Rôle actuel: {role}, IsLocal: {isLocalPlayer}");

        // Activer/désactiver ShadowPlayer pour les Ombres
        ShadowPlayer shadowPlayer = GetComponent<ShadowPlayer>();
        if (shadowPlayer != null)
        {
            bool shouldEnable = (role == Role.Ombre);
            shadowPlayer.enabled = shouldEnable;
            Debug.Log($"[GamePlayer] ShadowPlayer {(shouldEnable ? "activé" : "désactivé")} pour {role}");
        }
        else
        {
            Debug.Log($"[GamePlayer] Aucun ShadowPlayer trouvé sur ce GameObject");
        }

        // Activer/désactiver OffsetFlashLight pour les Gardiens
        OffsetFlashLight flashLight = GetComponent<OffsetFlashLight>();
        if (flashLight != null)
        {
            bool shouldEnable = (role == Role.Gardien);
            flashLight.enabled = shouldEnable;
            Debug.Log($"[GamePlayer] OffsetFlashLight {(shouldEnable ? "activé" : "désactivé")} pour {role}");
        }
        else
        {
            Debug.Log($"[GamePlayer] Aucun OffsetFlashLight trouvé sur ce GameObject");
        }
    }

    // Activation des éléments du joueur local
    private void SetupLocalPlayer()
    {
        Debug.Log($"[GamePlayer] SetupLocalPlayer appelé pour {playerName}");

        Camera cam = GetComponentInChildren<Camera>(true);
        if (cam != null)
        {
            cam.gameObject.SetActive(true);
            Debug.Log("[GamePlayer] Caméra activée pour le joueur local");
        }
        else
        {
            Debug.LogWarning("[GamePlayer] Aucune caméra trouvée pour le joueur local");
        }

        // Note: NetworkPlayerController gère l'activation du ThirdPersonController
        // On ne l'active plus ici pour éviter les conflits
        Debug.Log("[GamePlayer] Le ThirdPersonController sera géré par NetworkPlayerController");
    }

    private void Update()
    {

    }
}