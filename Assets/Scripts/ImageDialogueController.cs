using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DefaultExecutionOrder(-200)]
public class ImageDialogueController : MonoBehaviour
{
    [SerializeField] private string backgroundResourcePath = ImageDialogueRequest.BackgroundResourcePath;
    [SerializeField] private string audioResourcePath = ImageDialogueRequest.AudioResourcePath;
    [SerializeField] private string skipPrompt = "E / ESC: Thoát";
    [SerializeField] private float audioVolume = 1f;

    private AudioSource audioSource;
    private AudioListener sceneListener;
    private AudioClip dialogueAudio;
    private bool isFinishing;
    private bool playbackStarted;

    private GUIStyle skipLabelStyle;

    private void Awake()
    {
        ConfigureMainCamera();
        SetupDisplay();
        SetupAudio();
        LoadDialogueAudio();
    }

    private void Start()
    {
        StartCoroutine(BeginPlaybackRoutine());
    }

    private IEnumerator BeginPlaybackRoutine()
    {
        yield return null;

        SilenceForeignAudio();
        EnsureSceneAudioListener();
        PlayDialogueAudio();
        playbackStarted = dialogueAudio != null;
    }

    private void LoadDialogueAudio()
    {
        if (dialogueAudio != null)
            return;

        if (!string.IsNullOrWhiteSpace(audioResourcePath))
            dialogueAudio = Resources.Load<AudioClip>(audioResourcePath);
    }

    private void ConfigureMainCamera()
    {
        var cam = FindSceneCamera();
        if (cam == null)
            return;

        cam.enabled = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.cullingMask = 0;
        cam.depth = -10;
    }

    private Camera FindSceneCamera()
    {
        var scene = gameObject.scene;
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var cam = roots[i].GetComponentInChildren<Camera>(true);
            if (cam != null)
                return cam;
        }

        return null;
    }

    private void SetupDisplay()
    {
        var tex = Resources.Load<Texture2D>(backgroundResourcePath);
        if (tex == null)
        {
            Debug.LogError("[ImageDialogue] Missing background: " + backgroundResourcePath);
            return;
        }

        var canvasGo = new GameObject("ImageDialogueCanvas");

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var imageGo = new GameObject("BackgroundImage");
        imageGo.transform.SetParent(canvasGo.transform, false);

        var image = imageGo.AddComponent<Image>();
        image.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        image.preserveAspect = true;
        image.color = Color.white;
        image.raycastTarget = false;

        var rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void SetupAudio()
    {
        var audioGo = new GameObject("ImageDialogueAudio");
        audioGo.transform.SetParent(transform, false);
        audioSource = audioGo.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;
        audioSource.volume = audioVolume;
        audioSource.mute = false;
        audioSource.priority = 0;
    }

    private void SilenceForeignAudio()
    {
        var playerRoot = PlayerSceneTransition.GetPlayerRoot();
        if (playerRoot == null)
            return;

        var sources = playerRoot.GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null || sources[i] == audioSource)
                continue;

            sources[i].Pause();
            sources[i].volume = 0f;
        }
    }

    private void EnsureSceneAudioListener()
    {
        var playerRoot = PlayerSceneTransition.GetPlayerRoot();
        if (playerRoot != null)
        {
            var playerListeners = playerRoot.GetComponentsInChildren<AudioListener>(true);
            for (int i = 0; i < playerListeners.Length; i++)
                playerListeners[i].enabled = false;
        }

        sceneListener = null;
        var sceneCamera = FindSceneCamera();
        if (sceneCamera != null)
        {
            sceneListener = sceneCamera.GetComponent<AudioListener>();
            if (sceneListener == null)
                sceneListener = sceneCamera.gameObject.AddComponent<AudioListener>();
        }

        if (sceneListener == null)
            sceneListener = gameObject.GetComponent<AudioListener>() ?? gameObject.AddComponent<AudioListener>();

        sceneListener.enabled = true;
        AudioListener.pause = false;
        AudioListener.volume = 1f;
    }

    private void PlayDialogueAudio()
    {
        LoadDialogueAudio();
        EnsureSceneAudioListener();

        if (dialogueAudio == null)
        {
            Debug.LogError("[ImageDialogue] Missing audio clip.");
            return;
        }

        if (dialogueAudio.loadState == AudioDataLoadState.Unloaded)
            dialogueAudio.LoadAudioData();

        audioSource.Stop();
        audioSource.clip = dialogueAudio;
        audioSource.volume = audioVolume;
        audioSource.mute = false;
        audioSource.spatialBlend = 0f;
        audioSource.Play();
    }

    private void Update()
    {
        if (isFinishing || Keyboard.current == null)
            return;

        if (playbackStarted && dialogueAudio != null && audioSource != null && !audioSource.isPlaying)
        {
            ReturnToGame(true);
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            ReturnToGame(playbackStarted);
    }

    private void ReturnToGame(bool markComplete)
    {
        if (isFinishing)
            return;

        isFinishing = true;

        if (markComplete)
            ImageDialogueRequest.MarkComplete();

        if (audioSource != null)
            audioSource.Stop();

        var canvas = GameObject.Find("ImageDialogueCanvas");
        if (canvas != null)
            Destroy(canvas);

        var sceneName = ImageDialogueRequest.ReturnSceneName;
        if (string.IsNullOrWhiteSpace(sceneName))
            sceneName = "SampleScene";

        SceneLoader.Load(sceneName);
    }

    private void OnGUI()
    {
        if (isFinishing)
            return;

        EnsureGuiStyles();

        const float width = 420f;
        const float height = 48f;
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 72f, width, height);

        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.Box(rect, GUIContent.none);
        GUI.Label(rect, skipPrompt, skipLabelStyle);
        GUI.color = Color.white;
    }

    private void EnsureGuiStyles()
    {
        if (skipLabelStyle != null)
            return;

        skipLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        skipLabelStyle.normal.textColor = Color.white;
    }
}
