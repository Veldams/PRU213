using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-200)]
public class ChatDialogueController : MonoBehaviour
{
    [SerializeField] private string advancePrompt = "E / Click: Tiếp tục";
    [SerializeField] private string finishPrompt = "E / Click: Kết thúc";

    private static ChatDialogueController activeInstance;
    private static RealMovement pendingMovementLock;
    private static bool pendingOverlayInScene;
    private static bool pendingChainPoliceFollowUp;

    private ChatDialogueLine[] dialogueLines;
    private CanvasGroup chatPanelGroup;
    private Text speakerText;
    private Text messageText;
    private Text promptText;
    private Image panelBackground;
    private int currentLineIndex = -1;
    private bool isFinishing;
    private bool isShowingLine;
    private bool overlayInCurrentScene;
    private RealMovement lockedMovement;
    private AudioSource dialogueAudioSource;
    private bool chainPoliceFollowUp;
    private Coroutine autoAdvanceCoroutine;
    private float advanceInputBlockedUntil;

    public static bool IsActive => activeInstance != null;

    public static void StartInScene(
        VideoDialogueRequest.DialoguePhase phase,
        RealMovement lockMovement = null,
        bool chainPoliceFollowUp = false)
    {
        if (activeInstance != null)
            return;

        pendingOverlayInScene = true;
        pendingMovementLock = lockMovement;
        pendingChainPoliceFollowUp = chainPoliceFollowUp;
        VideoDialogueRequest.CurrentPhase = phase;

        var go = new GameObject("InSceneChatDialogue");
        go.AddComponent<ChatDialogueController>();
    }

    private void Awake()
    {
        if (activeInstance != null)
        {
            Destroy(gameObject);
            return;
        }

        activeInstance = this;
        overlayInCurrentScene = pendingOverlayInScene;
        lockedMovement = pendingMovementLock;
        chainPoliceFollowUp = pendingChainPoliceFollowUp;
        pendingOverlayInScene = false;
        pendingMovementLock = null;
        pendingChainPoliceFollowUp = false;

        dialogueLines = chainPoliceFollowUp
            ? ChatDialogueData.GetPoliceStationOpeningLines()
            : ChatDialogueData.GetLines(VideoDialogueRequest.CurrentPhase);

        if (!overlayInCurrentScene)
            ConfigureMainCamera();

        if (lockedMovement != null)
            lockedMovement.enabled = false;

        SetupDialogueAudio();
        BuildUi();
    }

    private void SetupDialogueAudio()
    {
        var audioGo = new GameObject("ChatDialogueAudio");
        audioGo.transform.SetParent(transform);

        dialogueAudioSource = audioGo.AddComponent<AudioSource>();
        dialogueAudioSource.playOnAwake = false;
        dialogueAudioSource.spatialBlend = 0f;
        dialogueAudioSource.volume = 1f;
    }

    private void OnDestroy()
    {
        if (activeInstance == this)
            activeInstance = null;

        if (lockedMovement != null)
            lockedMovement.enabled = true;

        StopLineAudio();
    }

    private void Start()
    {
        StartCoroutine(BeginDialogueRoutine());
    }

    private IEnumerator BeginDialogueRoutine()
    {
        yield return null;
        ShowNextLine();
    }

    private void Update()
    {
        if (isFinishing || !isShowingLine)
            return;

        if (Time.unscaledTime < advanceInputBlockedUntil)
            return;

        if (Keyboard.current == null && Mouse.current == null)
            return;

        bool advance = Keyboard.current != null
            && (Keyboard.current.eKey.wasPressedThisFrame
                || Keyboard.current.spaceKey.wasPressedThisFrame
                || Keyboard.current.enterKey.wasPressedThisFrame);

        if (!advance && Mouse.current != null)
            advance = Mouse.current.leftButton.wasPressedThisFrame;

        if (!advance)
            return;

        if (currentLineIndex >= dialogueLines.Length - 1)
            FinishDialogue();
        else
            ShowNextLine();
    }

    private void ShowNextLine()
    {
        StopAutoAdvance();

        currentLineIndex++;

        if (currentLineIndex < 0 || currentLineIndex >= dialogueLines.Length)
        {
            FinishDialogue();
            return;
        }

        var line = dialogueLines[currentLineIndex];
        speakerText.text = line.speaker;
        messageText.text = line.message;
        promptText.text = currentLineIndex >= dialogueLines.Length - 1 ? finishPrompt : advancePrompt;

        ApplySpeakerStyle(line.speaker);
        isShowingLine = true;
        advanceInputBlockedUntil = Time.unscaledTime + 0.35f;

        if (chatPanelGroup != null)
            chatPanelGroup.alpha = 1f;

        PlayLineAudio(line);
        TryScheduleAutoAdvance(line);
    }

    private void TryScheduleAutoAdvance(ChatDialogueLine line)
    {
        if (currentLineIndex >= dialogueLines.Length - 1)
            return;

        if (string.IsNullOrWhiteSpace(line.audioResourcePath))
            return;

        if (dialogueAudioSource == null || dialogueAudioSource.clip == null)
            return;

        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterAudio());
    }

    private IEnumerator AutoAdvanceAfterAudio()
    {
        while (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
            yield return null;

        if (isFinishing || !isShowingLine)
            yield break;

        if (currentLineIndex >= dialogueLines.Length - 1)
            yield break;

        yield return new WaitForSecondsRealtime(0.25f);

        if (isFinishing || !isShowingLine)
            yield break;

        ShowNextLine();
    }

    private void StopAutoAdvance()
    {
        if (autoAdvanceCoroutine == null)
            return;

        StopCoroutine(autoAdvanceCoroutine);
        autoAdvanceCoroutine = null;
    }

    private void PlayLineAudio(ChatDialogueLine line)
    {
        if (dialogueAudioSource == null)
            return;

        dialogueAudioSource.Stop();

        if (string.IsNullOrWhiteSpace(line.audioResourcePath))
            return;

        var clip = Resources.Load<AudioClip>(line.audioResourcePath);
        if (clip == null)
        {
            Debug.LogWarning("[ChatDialogue] Missing audio: " + line.audioResourcePath);
            return;
        }

        dialogueAudioSource.clip = clip;
        dialogueAudioSource.Play();
    }

    private void StopLineAudio()
    {
        if (dialogueAudioSource != null)
            dialogueAudioSource.Stop();
    }

    private void ApplySpeakerStyle(string speaker)
    {
        if (panelBackground == null)
            return;

        bool isCaptain = speaker.Contains("Đội trưởng") || speaker.Contains("Trung úy");
        if (isCaptain)
        {
            panelBackground.color = new Color(0.12f, 0.16f, 0.24f, 0.94f);
            speakerText.color = new Color(1f, 0.86f, 0.45f, 1f);
        }
        else
        {
            panelBackground.color = new Color(0.08f, 0.18f, 0.32f, 0.94f);
            speakerText.color = new Color(0.72f, 0.88f, 1f, 1f);
        }
    }

    private void FinishDialogue()
    {
        if (isFinishing)
            return;

        isFinishing = true;
        isShowingLine = false;
        StopAutoAdvance();
        StopLineAudio();

        var completedPhase = VideoDialogueRequest.CurrentPhase;
        var chainSuspectDialogue = completedPhase == VideoDialogueRequest.DialoguePhase.Investigation;

        if (chainPoliceFollowUp)
        {
            VideoDialogueRequest.MarkPhaseComplete(VideoDialogueRequest.DialoguePhase.First);
            VideoDialogueRequest.MarkPhaseComplete(VideoDialogueRequest.DialoguePhase.FollowUp);
        }
        else
        {
            VideoDialogueRequest.MarkPhaseComplete(completedPhase);
        }

        if (chainPoliceFollowUp || completedPhase == VideoDialogueRequest.DialoguePhase.FollowUp)
        {
            PoliceReceptionGuideState.ShowControlPanelObjectiveOnLoad = true;
            if (overlayInCurrentScene)
                ControlPanelObjectiveUI.TryCreateForScene(SceneManager.GetActiveScene());
        }

        var canvas = GameObject.Find("ChatDialogueCanvas");
        if (canvas != null)
            Destroy(canvas);

        if (overlayInCurrentScene && !chainSuspectDialogue)
        {
            Destroy(gameObject);
            return;
        }

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

    private void ConfigureMainCamera()
    {
        var cam = FindSceneCamera();
        if (cam == null)
            return;

        cam.enabled = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.07f, 0.1f, 1f);
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

    private void BuildUi()
    {
        var canvasGo = new GameObject("ChatDialogueCanvas");

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        if (!overlayInCurrentScene)
            CreateOverlay(canvasGo.transform);

        CreateChatPanel(canvasGo.transform);
    }

    private static void CreateOverlay(Transform parent)
    {
        var overlayGo = new GameObject("Overlay");
        overlayGo.transform.SetParent(parent, false);

        var image = overlayGo.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.55f);
        image.raycastTarget = false;

        var rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void CreateChatPanel(Transform parent)
    {
        var panelGo = new GameObject("ChatPanel");
        panelGo.transform.SetParent(parent, false);

        chatPanelGroup = panelGo.AddComponent<CanvasGroup>();
        panelBackground = panelGo.AddComponent<Image>();
        panelBackground.color = new Color(0.08f, 0.18f, 0.32f, 0.94f);
        panelBackground.raycastTarget = false;

        var outline = panelGo.AddComponent<Outline>();
        outline.effectColor = new Color(0.35f, 0.55f, 0.85f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        var panelRect = panelBackground.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(1180f, 260f);
        panelRect.anchoredPosition = new Vector2(0f, 48f);

        speakerText = CreateText(panelGo.transform, "SpeakerText", 30, FontStyle.Bold,
            new Vector2(0.04f, 0.72f), new Vector2(0.96f, 0.96f), TextAnchor.UpperLeft);

        messageText = CreateText(panelGo.transform, "MessageText", 26, FontStyle.Normal,
            new Vector2(0.04f, 0.22f), new Vector2(0.96f, 0.72f), TextAnchor.UpperLeft);
        messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
        messageText.verticalOverflow = VerticalWrapMode.Overflow;

        promptText = CreateText(panelGo.transform, "PromptText", 22, FontStyle.Italic,
            new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.18f), TextAnchor.MiddleRight);
        promptText.color = new Color(0.82f, 0.88f, 0.95f, 0.95f);
    }

    private static Text CreateText(Transform parent, string name, int fontSize, FontStyle fontStyle,
        Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.supportRichText = true;
        text.raycastTarget = false;

        var rect = text.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return text;
    }
}
