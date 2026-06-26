using UnityEngine;
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

    void Start()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        sfxVolume   = PlayerPrefs.GetFloat("SFXVolume",   1.0f);

        ApplyMusicVolume();
        UpdateVolumeDisplays();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ── Main buttons ──────────────────────────────────────────────────────────
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
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
        if (backgroundMusic != null)
            backgroundMusic.volume = musicVolume;
    }

    private void UpdateVolumeDisplays()
    {
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(musicVolume * 100f) + "%";
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(sfxVolume * 100f) + "%";
    }
}
