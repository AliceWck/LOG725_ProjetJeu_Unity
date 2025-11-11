using UnityEngine;
using UnityEngine.UIElements;

namespace UI.MainMenu
{
    public class SettingsPanelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument uiDocument;

        private VisualElement root;
        private VisualElement settingsOverlay;

        // Boutons
        private Button backButton;
        private Button saveButton;

        // Son
        private Slider musicVolumeSlider;
        private Label musicVolumeValue;
        private Toggle musicMuteToggle;
        private Slider sfxVolumeSlider;
        private Label sfxVolumeValue;
        private Toggle sfxMuteToggle;

        // Contrôles (readonly pour l'instant, à voir sur future màj)
        private TextField forwardKey;
        private TextField backwardKey;
        private TextField leftKey;
        private TextField rightKey;
        private TextField jumpKey;
        private TextField interactKey;

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            root = uiDocument.rootVisualElement;
            InitializeElements();
            SetupEventHandlers();
            LoadSettings();

            // Masquer par défaut
            Hide();
        }

        private void InitializeElements()
        {
            settingsOverlay = root.Q<VisualElement>("settings-overlay");

            // Boutons
            backButton = root.Q<Button>("back-button");
            saveButton = root.Q<Button>("save-button");

            // Son
            musicVolumeSlider = root.Q<Slider>("music-volume-slider");
            musicVolumeValue = root.Q<Label>("music-volume-value");
            musicMuteToggle = root.Q<Toggle>("music-mute-toggle");

            sfxVolumeSlider = root.Q<Slider>("sfx-volume-slider");
            sfxVolumeValue = root.Q<Label>("sfx-volume-value");
            sfxMuteToggle = root.Q<Toggle>("sfx-mute-toggle");

            // Contrôles
            forwardKey = root.Q<TextField>("forward-key");
            backwardKey = root.Q<TextField>("backward-key");
            leftKey = root.Q<TextField>("left-key");
            rightKey = root.Q<TextField>("right-key");
            jumpKey = root.Q<TextField>("jump-key");
            interactKey = root.Q<TextField>("interact-key");
        }

        private void SetupEventHandlers()
        {
            // Boutons
            backButton?.RegisterCallback<ClickEvent>(evt =>
            {
                Hide();
                // Reprendre le jeu si on est en jeu
                if (GameUIManager.Instance != null)
                {
                    GameUIManager.Instance.ResumeGame();
                }
            });

            saveButton?.RegisterCallback<ClickEvent>(evt => SaveSettings());

            // Màj des valeurs de sliders
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.pageSize = 5f;
                musicVolumeSlider.RegisterValueChangedCallback(evt =>
                {
                    musicVolumeValue.text = $"{Mathf.RoundToInt(evt.newValue)}%";
                    ApplyMusicVolume(evt.newValue);
                });
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.pageSize = 5f; // Vitesse de défilement
                sfxVolumeSlider.RegisterValueChangedCallback(evt =>
                {
                    sfxVolumeValue.text = $"{Mathf.RoundToInt(evt.newValue)}%";
                    ApplySFXVolume(evt.newValue);
                });
            }

            // Toggles
            musicMuteToggle?.RegisterValueChangedCallback(evt => ApplyMusicMute(evt.newValue));
            sfxMuteToggle?.RegisterValueChangedCallback(evt => ApplySFXMute(evt.newValue));
        }

        #region Public Methods

        public void Show()
        {
            if (settingsOverlay != null)
            {
                settingsOverlay.RemoveFromClassList("hidden");
                settingsOverlay.style.display = DisplayStyle.Flex;
            }

            // Recharger les valeurs actuelles depuis AudioManager
            LoadSettings();
        }

        public void Hide()
        {
            if (settingsOverlay != null)
            {
                settingsOverlay.AddToClassList("hidden");
                settingsOverlay.style.display = DisplayStyle.None;
            }
        }

        #endregion

        #region Settings Management

        private void LoadSettings()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("AudioManager n'est pas initialisé");
                return;
            }

            // Charger les valeurs depuis AudioManager
            float musicVolume = AudioManager.Instance.MusicVolume;
            float sfxVolume = AudioManager.Instance.SFXVolume;
            bool musicMute = AudioManager.Instance.IsMusicMuted;
            bool sfxMute = AudioManager.Instance.IsSFXMuted;

            // Appliquer aux UI
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = musicVolume;
                musicVolumeValue.text = $"{Mathf.RoundToInt(musicVolume)}%";
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = sfxVolume;
                sfxVolumeValue.text = $"{Mathf.RoundToInt(sfxVolume)}%";
            }

            if (musicMuteToggle != null)
                musicMuteToggle.value = musicMute;

            if (sfxMuteToggle != null)
                sfxMuteToggle.value = sfxMute;
        }

        private void SaveSettings()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("AudioManager n'est pas initialisé");
                return;
            }

            // Sauvegarder via AudioManager
            AudioManager.Instance.SaveSettings();

            Debug.Log("Paramètres sauvegardés");

            // Feedback visuel
            saveButton.text = "Sauvegardé !";
            Invoke(nameof(ResetSaveButtonText), 2f);
        }


        private void ResetSaveButtonText()
        {
            if (saveButton != null)
                saveButton.text = "Sauvegarder";
        }

        #endregion

        #region Audio Application

        private void ApplyMusicVolume(float volume)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(volume);
            }
        }

        private void ApplyMusicMute(bool muted)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicMute(muted);
            }
        }

        private void ApplySFXVolume(float volume)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(volume);
            }
        }

        private void ApplySFXMute(bool muted)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXMute(muted);
            }
        }

        #endregion
    }
}