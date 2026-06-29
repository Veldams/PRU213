using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

[DefaultExecutionOrder(-200)]
[RequireComponent(typeof(VideoPlayer))]
public class VideoDialogueController : MonoBehaviour
{
    [SerializeField] private VideoClip dialogueClip;
    [SerializeField] private AudioClip dialogueAudio;
    [SerializeField] private string resourcesAudioPath = "";
    [SerializeField] private string skipPrompt = "E / ESC: Thoát";
    [SerializeField] private float audioVolume = 1f;

    private VideoPlayer videoPlayer;
    private RawImage videoImage;
    private RenderTexture renderTexture;
    private AudioSource audioSource;
    private AudioListener sceneListener;
    private bool isFinishing;
    private bool isPlaying;

    private void Awake()
    {
        dialogueClip = null;
        dialogueAudio = null;

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.Stop();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.started += OnVideoStarted;

        ConfigureMainCamera();
        SetupDisplay();
        SetupAudioOutputObject();
        LoadDialogueAudio();
    }

    private void LoadDialogueAudio()
    {
        if (dialogueAudio != null)
            return;

        string path = !string.IsNullOrWhiteSpace(resourcesAudioPath)
            ? resourcesAudioPath
            : VideoDialogueRequest.GetAudioResourcesPath(VideoDialogueRequest.CurrentPhase);

        if (!string.IsNullOrWhiteSpace(path))
            dialogueAudio = Resources.Load<AudioClip>(path);
    }

    private void Start()
    {
        StartCoroutine(BeginPlaybackRoutine());
    }

    private IEnumerator BeginPlaybackRoutine()
    {
        // PlayerSceneTransition hides DDOL player on sceneLoaded — wait one frame first.
        yield return null;

        SilenceForeignAudio();
        EnsureSceneAudioListener();

        string url = BuildVideoUrl(VideoDialogueRequest.GetVideoFileName(VideoDialogueRequest.CurrentPhase));
        if (!string.IsNullOrEmpty(url))
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = url;
            Debug.Log("[VideoDialogue] Phase=" + VideoDialogueRequest.CurrentPhase + " URL: " + url);
        }
        else if (dialogueClip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = dialogueClip;
            Debug.Log("[VideoDialogue] Clip: " + dialogueClip.name);
        }
        else
        {
            Debug.LogError("[VideoDialogue] Missing video file for phase " + VideoDialogueRequest.CurrentPhase);
            ReturnToPreviousScene();
            yield break;
        }

        ConfigureAudioBeforePrepare();
        EnsureRenderTexture(1280, 720);

        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.Prepare();

        float timeout = 10f;
        while (!videoPlayer.isPrepared && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogError("[VideoDialogue] Prepare timeout.");
            ReturnToPreviousScene();
        }
    }

    private static string BuildVideoUrl(string fileName)
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(streamingPath))
            return new System.Uri(streamingPath).AbsoluteUri;

        string assetPath = Path.Combine(Application.dataPath, "Video", fileName);
        if (File.Exists(assetPath))
            return new System.Uri(assetPath).AbsoluteUri;

        return null;
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
        var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            var cam = cameras[i];
            if (cam != null && cam.gameObject.scene == scene)
                return cam;
        }

        return Camera.main;
    }

    private void SilenceForeignAudio()
    {
        var sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < sources.Length; i++)
        {
            var source = sources[i];
            if (source == null || source == audioSource)
                continue;

            if (source.gameObject.scene.name == "DontDestroyOnLoad")
            {
                source.Pause();
                source.volume = 0f;
            }
        }
    }

    private void EnsureSceneAudioListener()
    {
        var scene = gameObject.scene;
        var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);

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

        for (int i = 0; i < listeners.Length; i++)
        {
            var listener = listeners[i];
            if (listener == null)
                continue;

            listener.enabled = listener == sceneListener;
        }

        sceneListener.enabled = true;
        AudioListener.pause = false;
        AudioListener.volume = 1f;

        Debug.Log("[VideoDialogue] AudioListener on: " + sceneListener.gameObject.name);
    }

    private void SetupDisplay()
    {
        var canvasGo = new GameObject("VideoCanvas");

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var imageGo = new GameObject("VideoImage");
        imageGo.transform.SetParent(canvasGo.transform, false);

        videoImage = imageGo.AddComponent<RawImage>();
        videoImage.color = Color.white;
        videoImage.raycastTarget = false;

        var rect = videoImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void SetupAudioOutputObject()
    {
        var audioGo = new GameObject("VideoAudioOutput");
        audioGo.transform.SetParent(transform, false);
        audioSource = audioGo.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;
        audioSource.volume = audioVolume;
        audioSource.mute = false;
        audioSource.priority = 0;
    }

    private void ConfigureAudioBeforePrepare()
    {
        videoPlayer.controlledAudioTrackCount = 0;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
    }

    private void PlayDialogueAudio()
    {
        LoadDialogueAudio();
        EnsureSceneAudioListener();

        if (dialogueAudio == null)
        {
            Debug.LogError("[VideoDialogue] Missing separate audio clip.");
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

        Debug.Log("[VideoDialogue] Audio playing=" + audioSource.isPlaying
            + " clip=" + dialogueAudio.name
            + " len=" + dialogueAudio.length
            + " listener=" + (sceneListener != null && sceneListener.enabled));
    }

    private void EnsureRenderTexture(int width, int height)
    {
        width = Mathf.Max(640, width);
        height = Mathf.Max(360, height);

        if (renderTexture != null && renderTexture.width == width && renderTexture.height == height)
            return;

        if (renderTexture != null)
        {
            videoPlayer.targetTexture = null;
            renderTexture.Release();
            Destroy(renderTexture);
        }

        renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        renderTexture.Create();
        videoPlayer.targetTexture = renderTexture;

        if (videoImage != null)
            videoImage.texture = renderTexture;
    }

    private void OnPrepared(VideoPlayer source)
    {
        source.prepareCompleted -= OnPrepared;

        uint width = source.width > 0 ? source.width : 1280;
        uint height = source.height > 0 ? source.height : 720;
        EnsureRenderTexture((int)width, (int)height);

        Debug.Log("[VideoDialogue] Prepared " + width + "x" + height + " len=" + source.length);
        PlayDialogueAudio();
        source.Play();
    }

    private void OnVideoStarted(VideoPlayer source)
    {
        isPlaying = true;

        if (videoImage != null && renderTexture != null)
            videoImage.texture = renderTexture;

        if (audioSource != null && dialogueAudio != null && !audioSource.isPlaying)
            PlayDialogueAudio();
    }

    private void Update()
    {
        if (isFinishing || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            ReturnToPreviousScene();
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        if (!isPlaying)
            return;

        ReturnToPreviousScene();
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError("[VideoDialogue] " + message);
        ReturnToPreviousScene();
    }

    private void ReturnToPreviousScene()
    {
        if (isFinishing)
            return;

        isFinishing = true;
        isPlaying = false;

        VideoDialogueRequest.MarkPhaseComplete(VideoDialogueRequest.CurrentPhase);

        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.started -= OnVideoStarted;
        videoPlayer.Stop();

        if (audioSource != null)
            audioSource.Stop();

        var canvas = GameObject.Find("VideoCanvas");
        if (canvas != null)
            Destroy(canvas);

        var sceneName = VideoDialogueRequest.ReturnSceneName;
        if (string.IsNullOrWhiteSpace(sceneName))
            sceneName = "Police_Reception_Office_Day";

        SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    private void OnGUI()
    {
        if (isFinishing)
            return;

        const float width = 420f;
        const float height = 48f;
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 72f, width, height);

        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.Box(rect, GUIContent.none);

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = Color.white;
        GUI.Label(rect, skipPrompt, style);
        GUI.color = Color.white;
    }
}
