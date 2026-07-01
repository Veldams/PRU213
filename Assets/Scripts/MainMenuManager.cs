using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("Volume Display")]
    public Text musicVolumeText;
    public Text sfxVolumeText;

    [Header("Audio")]
    public AudioSource backgroundMusic;

    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    void Awake()
    {
        EnsureUiInputModule();
        LoadVolumePrefs();
        StartBackgroundMusic();
    }

    void Start()
    {
        UpdateVolumeDisplays();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void EnsureUiInputModule()
    {
        var eventSystem = GetComponent<EventSystem>();
        if (eventSystem == null)
            return;

        var standaloneModule = GetComponent<StandaloneInputModule>();
        if (standaloneModule == null)
            standaloneModule = gameObject.AddComponent<StandaloneInputModule>();

        standaloneModule.enabled = true;
    }

    void LoadVolumePrefs()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
    }

    void StartBackgroundMusic()
    {
        if (backgroundMusic == null)
            return;

        backgroundMusic.loop = true;
        backgroundMusic.playOnAwake = true;
        ApplyMusicVolume();
    }

    // ── Main buttons ──────────────────────────────────────────────────────────
    public void StartGame()
    {
        SceneLoader.Load("SampleScene");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        PlayerPrefs.Save();
    }

    public void QuitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── Music volume ──────────────────────────────────────────────────────────
    public void MusicVolumeDown()
    {
        musicVolume = Mathf.Max(0f, musicVolume - 0.1f);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        ApplyMusicVolume();
        UpdateVolumeDisplays();
    }

    public void MusicVolumeUp()
    {
        musicVolume = Mathf.Min(1f, musicVolume + 0.1f);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        ApplyMusicVolume();
        UpdateVolumeDisplays();
    }

    // ── SFX volume ────────────────────────────────────────────────────────────
    public void SFXVolumeDown()
    {
        sfxVolume = Mathf.Max(0f, sfxVolume - 0.1f);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        UpdateVolumeDisplays();
    }

    public void SFXVolumeUp()
    {
        sfxVolume = Mathf.Min(1f, sfxVolume + 0.1f);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        UpdateVolumeDisplays();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void ApplyMusicVolume()
    {
        if (backgroundMusic == null)
            return;

        backgroundMusic.volume = musicVolume;

        if (backgroundMusic.clip != null && !backgroundMusic.isPlaying)
            backgroundMusic.Play();
    }

    private void UpdateVolumeDisplays()
    {
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(musicVolume * 100f) + "%";
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(sfxVolume * 100f) + "%";
    }
}
