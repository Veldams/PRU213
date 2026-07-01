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

    private bool playbackStarted;
    private bool prepareFailed;
    private string statusMessage = string.Empty;
    private bool pendingPrepared;
    private bool pendingStarted;
    private bool pendingFinished;
    private bool pendingError;
    private string pendingErrorMessage;

    private GUIStyle skipLabelStyle;
    private GUIStyle errorLabelStyle;

    private void Awake()
    {
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

        string fileName = VideoDialogueRequest.GetVideoFileName(VideoDialogueRequest.CurrentPhase);
        string resourcePath = VideoDialogueRequest.GetVideoResourcesPath(VideoDialogueRequest.CurrentPhase);
        VideoClip clip = Resources.Load<VideoClip>(resourcePath);

        if (clip == null && dialogueClip != null)
            clip = dialogueClip;

        if (clip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = clip;
            Debug.Log("[VideoDialogue] Phase=" + VideoDialogueRequest.CurrentPhase + " Clip: " + clip.name);
        }
        else
        {
            string url = BuildVideoUrl(fileName);
            if (!string.IsNullOrEmpty(url))
            {
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = url;
                Debug.Log("[VideoDialogue] Phase=" + VideoDialogueRequest.CurrentPhase + " URL: " + url);
            }
            else
            {
                Debug.LogError("[VideoDialogue] Missing video file for phase " + VideoDialogueRequest.CurrentPhase);
                prepareFailed = true;
                statusMessage = "Không tìm thấy video hội thoại. Nhấn E / ESC để quay lại.";
                yield break;
            }
        }

        ConfigureAudioBeforePrepare();
        EnsureRenderTexture(1280, 720);

        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.Prepare();

        float timeout = 30f;
        while (!videoPlayer.isPrepared && timeout > 0f && !prepareFailed)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogError("[VideoDialogue] Prepare timeout.");
            prepareFailed = true;
            statusMessage = "Không thể phát video. Nhấn E / ESC để quay lại.";
            yield break;
        }
    }

    private static string BuildVideoUrl(string fileName)
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(streamingPath))
            return ToFileUrl(streamingPath);

        string assetPath = Path.Combine(Application.dataPath, "Video", fileName);
        if (File.Exists(assetPath))
            return ToFileUrl(assetPath);

        return null;
    }

    private static string ToFileUrl(string path)
    {
        return new System.Uri(Path.GetFullPath(path)).AbsoluteUri;
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

    private void SilenceForeignAudio()
    {
        var playerRoot = PlayerSceneTransition.GetPlayerRoot();
        if (playerRoot == null)
            return;

        var sources = playerRoot.GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < sources.Length; i++)
        {
            var source = sources[i];
            if (source == null || source == audioSource)
                continue;

            source.Pause();
            source.volume = 0f;
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
        if (VideoDialogueRequest.UsesEmbeddedAudio(VideoDialogueRequest.CurrentPhase))
        {
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);
            return;
        }

        videoPlayer.controlledAudioTrackCount = 0;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
    }

    private void PlayDialogueAudio()
    {
        if (VideoDialogueRequest.UsesEmbeddedAudio(VideoDialogueRequest.CurrentPhase))
        {
            EnsureSceneAudioListener();
            return;
        }

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

    private void Update()
    {
        ProcessPendingVideoEvents();

        if (isFinishing || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            ReturnToPreviousScene(playbackStarted);
    }

    private void ProcessPendingVideoEvents()
    {
        if (pendingPrepared)
        {
            pendingPrepared = false;
            HandlePrepared();
        }

        if (pendingStarted)
        {
            pendingStarted = false;
            HandleStarted();
        }

        if (pendingError)
        {
            pendingError = false;
            HandleVideoError(pendingErrorMessage);
            pendingErrorMessage = string.Empty;
        }

        if (pendingFinished)
        {
            pendingFinished = false;
            HandleVideoFinished();
        }
    }

    private void HandlePrepared()
    {
        if (videoPlayer == null || isFinishing)
            return;

        videoPlayer.prepareCompleted -= OnPrepared;

        uint width = videoPlayer.width > 0 ? videoPlayer.width : 1280;
        uint height = videoPlayer.height > 0 ? videoPlayer.height : 720;
        EnsureRenderTexture((int)width, (int)height);

        Debug.Log("[VideoDialogue] Prepared " + width + "x" + height + " len=" + videoPlayer.length);
        PlayDialogueAudio();
        videoPlayer.Play();
    }

    private void HandleStarted()
    {
        if (isFinishing)
            return;

        playbackStarted = true;
        isPlaying = true;

        if (videoImage != null && renderTexture != null)
            videoImage.texture = renderTexture;

        if (audioSource != null && dialogueAudio != null && !audioSource.isPlaying
            && !VideoDialogueRequest.UsesEmbeddedAudio(VideoDialogueRequest.CurrentPhase))
            PlayDialogueAudio();
    }

    private void HandleVideoFinished()
    {
        if (!isPlaying)
            return;

        ReturnToPreviousScene(true);
    }

    private void HandleVideoError(string message)
    {
        Debug.LogError("[VideoDialogue] " + message);
        prepareFailed = true;
        statusMessage = "Lỗi phát video. Nhấn E / ESC để quay lại.";
    }

    private void OnPrepared(VideoPlayer source)
    {
        pendingPrepared = true;
    }

    private void OnVideoStarted(VideoPlayer source)
    {
        pendingStarted = true;
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        pendingFinished = true;
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        pendingErrorMessage = message;
        pendingError = true;
    }

    private void ReturnToPreviousScene(bool markDialogueComplete)
    {
        if (isFinishing)
            return;

        isFinishing = true;
        isPlaying = false;

        var completedPhase = VideoDialogueRequest.CurrentPhase;
        var chainSuspectDialogue = markDialogueComplete
            && completedPhase == VideoDialogueRequest.DialoguePhase.Investigation;

        if (markDialogueComplete)
        {
            VideoDialogueRequest.MarkPhaseComplete(completedPhase);

            if (completedPhase == VideoDialogueRequest.DialoguePhase.FollowUp)
                PoliceReceptionGuideState.ShowControlPanelObjectiveOnLoad = true;
        }

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

        if (chainSuspectDialogue)
        {
            ImageDialogueRequest.ReturnSceneName = VideoDialogueRequest.ReturnSceneName;
            SceneLoader.Load(ImageDialogueRequest.SceneName);
            return;
        }

        var sceneName = VideoDialogueRequest.ReturnSceneName;
        if (string.IsNullOrWhiteSpace(sceneName))
            sceneName = "Police_Reception_Office_Day";

        SceneLoader.Load(sceneName);
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

        EnsureGuiStyles();

        const float width = 420f;
        const float height = 48f;
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 72f, width, height);

        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.Box(rect, GUIContent.none);
        GUI.Label(rect, skipPrompt, skipLabelStyle);

        if (!string.IsNullOrEmpty(statusMessage))
        {
            var errorRect = new Rect((Screen.width - 640f) * 0.5f, 72f, 640f, 40f);
            GUI.color = new Color(0.6f, 0.1f, 0.1f, 0.85f);
            GUI.Box(errorRect, GUIContent.none);
            GUI.Label(errorRect, statusMessage, errorLabelStyle);
        }

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

        errorLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        errorLabelStyle.normal.textColor = Color.white;
    }
}
