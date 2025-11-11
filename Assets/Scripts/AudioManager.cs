using UnityEngine;

// Gestionnaire audio global persistant entre les scènes (musique de fond +  volume des effets sonores)
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    // Clés pour PlayerPrefs
    private const string MUSIC_VOLUME_KEY = "Settings_MusicVolume";
    private const string SFX_VOLUME_KEY = "Settings_SFXVolume";
    private const string MUSIC_MUTE_KEY = "Settings_MusicMute";
    private const string SFX_MUTE_KEY = "Settings_SFXMute";

    // Valeurs par défaut
    private float musicVolume = 25f;
    private float sfxVolume = 95f;
    private bool musicMuted = false;
    private bool sfxMuted = false;

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton pattern avec DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialiser les AudioSources si non assignées
        InitializeAudioSources();

        // Charger les paramètres sauvegardés
        LoadSettings();
        ApplySettings();
    }

    private void Start()
    {
        // Jouer la musique du menu au démarrage
        PlayMenuMusic();
    }

    #endregion

    #region Initialization

    private void InitializeAudioSources()
    {
        // Créer les AudioSources s'ils n'existent pas
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    #endregion

    #region Settings Management

    private void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 25f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 95f);
        musicMuted = PlayerPrefs.GetInt(MUSIC_MUTE_KEY, 0) == 1;
        sfxMuted = PlayerPrefs.GetInt(SFX_MUTE_KEY, 0) == 1;
    }

    private void ApplySettings()
    {
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        SetMusicMute(musicMuted);
        SetSFXMute(sfxMuted);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.SetInt(MUSIC_MUTE_KEY, musicMuted ? 1 : 0);
        PlayerPrefs.SetInt(SFX_MUTE_KEY, sfxMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    #endregion

    #region Music Control

    // Joue la musique du menu principal
    public void PlayMenuMusic()
    {
        if (menuMusic != null && musicSource.clip != menuMusic)
        {
            musicSource.clip = menuMusic;
            musicSource.Play();
            Debug.Log("AudioManager: Musique menu lancée");
        }
    }

    // Joue la musique de la partie en jeu
    public void PlayGameMusic()
    {
        // Arrête la musique du menu
        musicSource.Stop();

        if (gameMusic != null)
        {
            // Il y a une musique de jeu assignée = la jouer
            if (musicSource.clip != gameMusic)
            {
                musicSource.clip = gameMusic;
            }
            musicSource.Play();
            Debug.Log("AudioManager : musique de jeu lancée");
        }
        else
        {
            // Pas de musique de jeu assignée = silence total
            musicSource.clip = null;
            Debug.Log("AudioManager: pas de musique de jeu assignée, musique stoppée");
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    #endregion

    #region Volume Control

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp(volume, 0f, 100f);
        musicSource.volume = musicVolume / 100f;
    }


    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp(volume, 0f, 100f);
        sfxSource.volume = sfxVolume / 100f;
    }

    public void SetMusicMute(bool muted)
    {
        musicMuted = muted;
        musicSource.mute = musicMuted;
    }


    public void SetSFXMute(bool muted)
    {
        sfxMuted = muted;
        sfxSource.mute = sfxMuted;
    }

    #endregion

    #region Getters

    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public bool IsMusicMuted => musicMuted;
    public bool IsSFXMuted => sfxMuted;

    public AudioSource MusicSource => musicSource;
    public AudioSource SFXSource => sfxSource;

    #endregion

    #region SFX Playback


    // Joue un effet sonore one-shot
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && !sfxMuted)
        {
            sfxSource.PlayOneShot(clip, sfxVolume / 100f);
        }
    }

    // Joue un effet sonore one-shot avec volume choisi perso (0-1)
    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip != null && !sfxMuted)
        {
            sfxSource.PlayOneShot(clip, (sfxVolume / 100f) * volumeScale);
        }
    }

    #endregion
}