using UnityEngine;
using UnityEngine.UIElements;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Game Settings")]
    [SerializeField] private int totalKeys = 5;
    [SerializeField] private bool isOmbreRole = true;

    [Header("Audio")]
    [SerializeField] private AudioSource backgroundMusic;

    // Références aux éléments UI
    private VisualElement root;

    // Bandeau supérieur
    private Button quitButton;
    private VisualElement healthContainer;
    private VisualElement healthBarFill;
    private VisualElement timerIndicator;
    private VisualElement timerArcFill;
    private Label phaseLabel;
    private VisualElement phaseIcon;
    private Label keyLabel;
    private Button soundButton;
    private Button settingsButton;

    // Game overlay
    private VisualElement minimapDisplay;
    private ScrollView playersList;

    // Popup de confirmation quitter
    private VisualElement quitPopup;
    private Button quitConfirmButton;
    private Button quitCancelButton;

    // État du jeu
    private int keysFound = 0;
    private float gameProgress = 0f; // 0-100
    private float playerHealth = 100f; // 0-100
    private bool isMuted = false;

    // Phases du jeu, débt à evening
    private enum GamePhase { Evening, Night, Dawn }
    private GamePhase currentPhase = GamePhase.Evening;

    public static GameUIManager Instance { get; private set; }

    private void Awake()
    {
        gameObject.SetActive(true); // Force le GameObject actif
        enabled = true; // Force le component actif
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    private void OnEnable()
    {
        // Récupérer le UIDocument sur ce GameObject
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("UIDocument introuvable sur " + gameObject.name);
            return;
        }

        root = uiDocument.rootVisualElement;

        if (root == null)
        {
            Debug.LogError("RootVisualElement est null, vérifier que le Source Asset est assigné");
            return;
        }

        InitializeUIElements();
        SetupEventHandlers();
        SetupArcFillPainter();
        UpdateUIForRole();
        UpdateAllUI();
    }

    private void InitializeUIElements()
    {
        // Bandeau supérieur - Gauche
        quitButton = root.Q<Button>("quit-button");
        healthContainer = root.Q<VisualElement>("health-container");
        healthBarFill = root.Q<VisualElement>("health-bar-fill");

        // Bandeau supérieur - Centre
        timerIndicator = root.Q<VisualElement>("timer-indicator");
        timerArcFill = root.Q<VisualElement>("timer-arc-fill");
        phaseLabel = root.Q<Label>("phase-label");
        phaseIcon = root.Q<VisualElement>("phase-icon");
        keyLabel = root.Q<Label>("key-label");

        // Bandeau supérieur - Droite
        soundButton = root.Q<Button>("sound-button");
        settingsButton = root.Q<Button>("settings-button");

        // Game overlay
        minimapDisplay = root.Q<VisualElement>("minimap-display");
        playersList = root.Q<ScrollView>("players-list");

        // Popup de confirmation
        quitPopup = root.Q<VisualElement>("quit-popup");
        quitConfirmButton = root.Q<Button>("quit-confirm-button");
        quitCancelButton = root.Q<Button>("quit-cancel-button");

    }

    private void SetupArcFillPainter()
    {
        if (timerArcFill == null) return;

        timerArcFill.generateVisualContent += OnGenerateArcVisualContent;
    }

    private void OnGenerateArcVisualContent(MeshGenerationContext ctx)
    {
        if (timerArcFill == null || gameProgress <= 0) return;

        float width = timerArcFill.contentRect.width;
        float height = timerArcFill.contentRect.height;

        if (width <= 0 || height <= 0) return;

        var painter = ctx.painter2D;
        painter.lineWidth = 8f;
        painter.strokeColor = new Color(0.93f, 0.54f, 0.21f, 1f); // Orange
        painter.lineCap = LineCap.Round;
        painter.lineJoin = LineJoin.Round;

        // Centre du demi-cercle (en bas pour arc supérieur)
        float centerX = width / 2f;
        float centerY = height;
        float radius = width / 2f - painter.lineWidth / 2f;

        // Nombre de segments pour dessiner l'arc (plus = plus lisse)
        int segments = 50;

        // Calculer combien de segments à dessiner selon la progression
        int segmentsToDraw = Mathf.RoundToInt(segments * (gameProgress / 100f));

        if (segmentsToDraw > 0)
        {
            painter.BeginPath();

            // Dessiner l'arc segment par segment de gauche (pi) vers droite (0)
            for (int i = 0; i <= segmentsToDraw; i++)
            {
                float t = (float)i / segments;
                float angle = Mathf.Lerp(Mathf.PI, 0f, t); // De 180° à 0°

                float x = centerX + Mathf.Cos(angle) * radius;
                float y = centerY - Mathf.Sin(angle) * radius;

                if (i == 0)
                    painter.MoveTo(new Vector2(x, y));
                else
                    painter.LineTo(new Vector2(x, y));
            }

            painter.Stroke();
        }
    }

    // Gestion event sur clic boutons
    private void SetupEventHandlers()
    {
        // Bouton Quitter
        quitButton?.RegisterCallback<ClickEvent>(evt => ShowQuitPopup());
        quitConfirmButton?.RegisterCallback<ClickEvent>(evt => ConfirmQuit());
        quitCancelButton?.RegisterCallback<ClickEvent>(evt => HideQuitPopup());


        // Bouton Son
        soundButton?.RegisterCallback<ClickEvent>(evt => ToggleSound());

        // Bouton Paramètres
        settingsButton?.RegisterCallback<ClickEvent>(evt => OnSettingsClicked());
    }

    #region UI Updates

    private void UpdateUIForRole()
    {
        // Afficher/masquer la health bar selon le rôle - à ajuster pour mettre finaleement flashlight Gardien 
        if (healthContainer != null)
        {
            healthContainer.style.display = isOmbreRole ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void UpdateAllUI()
    {
        UpdateHealthBar();
        UpdateTimer();
        UpdateKeyCounter();
        UpdatePlayersList();
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null && isOmbreRole)
        {
            healthBarFill.style.width = Length.Percent(playerHealth);
        }
    }

    private void UpdateTimer()
    {
        // Déterminer la phase time actuelle
        if (gameProgress < 33f)
            currentPhase = GamePhase.Evening;
        else if (gameProgress < 66f)
            currentPhase = GamePhase.Night;
        else
            currentPhase = GamePhase.Dawn;

        // Màj le label et l'icône
        string phaseText = "";
        Color phaseColor = Color.white;

        switch (currentPhase)
        {
            case GamePhase.Evening:
                phaseText = "Soirée";
                phaseColor = new Color(0.29f, 0.33f, 0.41f); // Gris/bleu
                break;
            case GamePhase.Night:
                phaseText = "Nuit";
                phaseColor = new Color(0.1f, 0.13f, 0.17f); // Noir/bleu
                break;
            case GamePhase.Dawn:
                phaseText = "Aube";
                phaseColor = new Color(0.93f, 0.54f, 0.21f); // Orange
                break;
        }

        if (phaseLabel != null)
            phaseLabel.text = phaseText;

        if (phaseIcon != null)
            phaseIcon.style.unityBackgroundImageTintColor = phaseColor;

        // Animer l'indicateur (position sur l'arc)
        UpdateTimerIndicatorPosition();
    }

    private void UpdateTimerIndicatorPosition()
    {
        if (timerIndicator == null || timerIndicator.parent == null) return;

        // Utiliser les MÊMES valeurs que dans OnGenerateArcVisualContent pour lier point orange à scroll bar
        float width = timerIndicator.parent.resolvedStyle.width;
        float height = timerIndicator.parent.resolvedStyle.height;

        float centerX = width / 2f;
        float centerY = height;
        float radius = width / 2f - 8f / 2f; // 8f = lineWidth de l'arc

        // Même calcul d'angle que l'arc : pi à 0
        float angle = Mathf.Lerp(Mathf.PI, 0f, gameProgress / 100f);

        // Position sur l'arc
        float x = centerX + Mathf.Cos(angle) * radius;
        float y = centerY - Mathf.Sin(angle) * radius;

        // Centrer le petit point
        timerIndicator.style.left = x - timerIndicator.resolvedStyle.width / 2f;
        timerIndicator.style.top = y - timerIndicator.resolvedStyle.height / 2f;
    }

    private void UpdateKeyCounter()
    {
        if (keyLabel != null)
        {
            keyLabel.text = $"{keysFound}/{totalKeys}";
        }
    }

    public void RefreshPlayersList()
    {
        UpdatePlayersList();
    }


    private void UpdatePlayersList()
    {
        if (playersList == null) return;

        playersList.Clear();

        // Récupération des joueurs depuis GamePlayer en réseau
        foreach (var gp in GamePlayer.allPlayers)
        {
            var playerItem = CreatePlayerItem(new PlayerData
            {
                playerName = gp.PlayerName,
                isOmbre = (gp.PlayerRole == Role.Ombre),
                isLocalPlayer = gp.isLocalPlayer
            });

            playersList.Add(playerItem);
        }
    }

    private VisualElement CreatePlayerItem(PlayerData player)
    {
        var item = new VisualElement();
        item.AddToClassList("player-item");

        if (player.isLocalPlayer)
            item.AddToClassList("player-item--you");

        // Nom du joueur
        var nameLabel = new Label(player.playerName + (player.isLocalPlayer ? " (Vous)" : ""));
        nameLabel.AddToClassList("player-name");
        if (player.isLocalPlayer)
            nameLabel.AddToClassList("player-name--you");

        // Rôle du joueur
        var roleLabel = new Label(player.isOmbre ? "Ombre" : "Gardien");
        roleLabel.AddToClassList("player-role");
        roleLabel.AddToClassList(player.isOmbre ? "player-role--ombre" : "player-role--gardien");

        item.Add(nameLabel);
        item.Add(roleLabel);

        return item;
    }

    #endregion

    #region Event Handlers



    private void ShowQuitPopup()
    {
        if (quitPopup != null)
            quitPopup.RemoveFromClassList("hidden");
    }

    private void HideQuitPopup()
    {
        if (quitPopup != null)
            quitPopup.AddToClassList("hidden");
    }

    private void ConfirmQuit()
    {
        Debug.Log("Quit confirmé, retour au menu principal");

        // Ferme la popup immédiatement
        HideQuitPopup();

        if (NetworkManager.singleton != null)
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

        // Détruit l’UI du jeu avant de changer de scène
        Destroy(gameObject);

        // Détruire le GameManager aussi
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        SceneManager.LoadScene("MainMenu");
    }

    private void ToggleSound()
    {
        isMuted = !isMuted;

        if (backgroundMusic != null)
            backgroundMusic.mute = isMuted;

        AudioListener.volume = isMuted ? 0f : 1f;

        Debug.Log($"Son {(isMuted ? "coupé" : "activé")}");
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Ouverture des paramètres");
        // TODO: Ouvrir le menu paramètres
    }

    #endregion

    #region Public Methods (appelés par le réseau ou gameplay)

    public void SetPlayerRole(bool isOmbre)
    {
        isOmbreRole = isOmbre;
        UpdateUIForRole();
    }

    public void SetPlayerHealth(float health)
    {
        playerHealth = Mathf.Clamp(health, 0f, 100f);
        UpdateHealthBar();
    }



    public void SetGameProgress(float progress)
    {
        gameProgress = Mathf.Clamp(progress, 0f, 100f);
        UpdateTimer();

        // Redessiner l'arc
        if (timerArcFill != null)
            timerArcFill.MarkDirtyRepaint();
    }

    public void AddKey()
    {
        if (keysFound < totalKeys)
        {
            keysFound++;
            UpdateKeyCounter();
        }
    }

    public void SetMiniMapTexture(RenderTexture texture)
    {
        if (minimapDisplay != null && texture != null)
        {
            minimapDisplay.style.backgroundImage = Background.FromRenderTexture(texture);
        }
    }

    #endregion


    // Classe de données joueur
    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public bool isOmbre;
        public bool isLocalPlayer;
    }

    #region Update (pour la démo)

    private void Update()
    {
        // Demo progression
        // A virer dans version finale, et tri sur ça

        // gameProgress += Time.deltaTime * 5f; // Progression automatique
        // if (gameProgress > 100f) gameProgress = 0f;
        // UpdateTimer();

        // Test touches clavier pour la démo, ajout clés et cgt rôle
        if (Input.GetKeyDown(KeyCode.K))
            AddKey();

        if (Input.GetKeyDown(KeyCode.L))
        {
            SetPlayerRole(!isOmbreRole);
            Debug.Log($"Rôle changé : {(isOmbreRole ? "Ombre" : "Gardien")}");
        }

        // "Echap" pour popup quitter
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowQuitPopup();
        }
    }

    #endregion
}