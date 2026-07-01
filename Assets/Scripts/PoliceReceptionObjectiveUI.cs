using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Police reception intro: brief guide (3s) + mission to talk to the captain.
/// Shown when entering from SampleScene.
/// </summary>
public class PoliceReceptionObjectiveUI : ObjectiveHudBase
{
    private const string PoliceSceneName = "Police_Reception_Office_Day";
    private const string CaptainObjectName = "DoiTruong";

    [SerializeField] private string objectiveTitle = "NHIỆM VỤ";
    [SerializeField] private string objectiveMessage = "Hãy nói chuyện với đội trưởng";
    [SerializeField] private float guideDisplayDuration = 3f;

    private static PoliceReceptionObjectiveUI activeInstance;

    private Transform target;
    private CanvasGroup missionGroup;
    private CanvasGroup guideGroup;
    private GameObject guideCanvasGo;
    private Text distanceText;
    private RectTransform arrowRect;
    private bool isMissionVisible;
    private bool isCompleted;

    public static void TryCreateForScene(Scene scene)
    {
        if (scene.name != PoliceSceneName)
            return;

        if (!PoliceReceptionGuideState.ShowIntroOnLoad)
            return;

        if (VideoDialogueRequest.HasWatchedFirstDialogue)
        {
            PoliceReceptionGuideState.ShowIntroOnLoad = false;
            return;
        }

        PoliceReceptionGuideState.ShowIntroOnLoad = false;

        if (activeInstance != null)
            return;

        var root = new GameObject("PoliceReceptionObjectiveUI");
        root.AddComponent<PoliceReceptionObjectiveUI>();
    }

    public static void NotifyCaptainTalkStarted()
    {
        if (activeInstance != null)
            activeInstance.CompleteObjective();
    }

    private void Awake()
    {
        activeInstance = this;
    }

    private void OnDestroy()
    {
        if (activeInstance == this)
            activeInstance = null;
    }

    private void Start()
    {
        ResolveTarget();
        BuildMissionUi();
        BuildGuideUi();
        StartCoroutine(RunIntroSequence());
    }

    private void Update()
    {
        if (isCompleted || target == null || !isMissionVisible)
            return;

        ResolvePlayerOnce();
        if (player == null)
            return;

        if (!ShouldRefreshHud())
            return;

        UpdateDistanceAndArrow();
    }

    private void CompleteObjective()
    {
        if (isCompleted)
            return;

        isCompleted = true;
        StartCoroutine(HideMissionAndDestroy());
    }

    private void ResolveTarget()
    {
        var go = SceneObjectLocator.FindInScene(SceneManager.GetActiveScene(), CaptainObjectName, 2000);
        if (go == null)
            Debug.LogWarning("[PoliceReceptionObjectiveUI] Target not found: " + CaptainObjectName);
        else
            target = go.transform;
    }

    private void BuildMissionUi()
    {
        var canvasGo = new GameObject("MissionCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 85;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var panelGo = new GameObject("MissionPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -24f);
        panelRect.sizeDelta = new Vector2(820f, 110f);

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.10f, 0.18f, 0.88f);
        panelImage.raycastTarget = false;

        missionGroup = panelGo.AddComponent<CanvasGroup>();
        missionGroup.alpha = 0f;

        var font = GetUiFont();

        CreateLabel(panelGo.transform, "Title", font, 24, FontStyle.Bold,
            new Vector2(-270f, -18f), new Vector2(220f, 30f), TextAnchor.MiddleLeft, objectiveTitle);

        CreateLabel(panelGo.transform, "Message", font, 28, FontStyle.Normal,
            new Vector2(20f, -18f), new Vector2(520f, 34f), TextAnchor.MiddleLeft, objectiveMessage);

        distanceText = CreateLabel(panelGo.transform, "Distance", font, 22, FontStyle.Bold,
            new Vector2(0f, -62f), new Vector2(760f, 28f), TextAnchor.MiddleCenter, string.Empty);

        var arrowGo = new GameObject("DirectionArrow");
        arrowGo.transform.SetParent(panelGo.transform, false);

        arrowRect = arrowGo.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(0f, 0.5f);
        arrowRect.anchorMax = new Vector2(0f, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        arrowRect.anchoredPosition = new Vector2(34f, -18f);
        arrowRect.sizeDelta = new Vector2(36f, 36f);

        var arrowLabel = arrowGo.AddComponent<Text>();
        arrowLabel.font = font;
        arrowLabel.fontSize = 30;
        arrowLabel.fontStyle = FontStyle.Bold;
        arrowLabel.alignment = TextAnchor.MiddleCenter;
        arrowLabel.color = new Color(1f, 0.86f, 0.35f);
        arrowLabel.text = ">";
        arrowLabel.raycastTarget = false;
    }

    private void BuildGuideUi()
    {
        guideCanvasGo = new GameObject("GuideCanvas");
        guideCanvasGo.transform.SetParent(transform, false);

        var canvas = guideCanvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;

        var scaler = guideCanvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var backdropGo = new GameObject("Backdrop");
        backdropGo.transform.SetParent(guideCanvasGo.transform, false);

        var backdropRect = backdropGo.AddComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;

        var backdropImage = backdropGo.AddComponent<Image>();
        backdropImage.color = new Color(0f, 0f, 0f, 0.45f);
        backdropImage.raycastTarget = false;

        var panelGo = new GameObject("GuidePanel");
        panelGo.transform.SetParent(guideCanvasGo.transform, false);

        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(720f, 220f);

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.06f, 0.08f, 0.12f, 0.94f);
        panelImage.raycastTarget = false;

        guideGroup = panelGo.AddComponent<CanvasGroup>();
        guideGroup.alpha = 0f;
        guideGroup.blocksRaycasts = false;
        guideGroup.interactable = false;

        var font = GetUiFont();

        CreateLabel(panelGo.transform, "GuideTitle", font, 34, FontStyle.Bold,
            new Vector2(0f, 52f), new Vector2(640f, 44f), TextAnchor.MiddleCenter, "HƯỚNG DẪN");

        CreateLabel(panelGo.transform, "GuideLine1", font, 28, FontStyle.Normal,
            new Vector2(0f, 0f), new Vector2(640f, 40f), TextAnchor.MiddleCenter,
            "Tìm đội trưởng ở bàn bên trái");

        CreateLabel(panelGo.transform, "GuideLine2", font, 28, FontStyle.Normal,
            new Vector2(0f, -46f), new Vector2(640f, 40f), TextAnchor.MiddleCenter,
            "Nhấn E khi đến gần để trò chuyện");
    }

    private IEnumerator RunIntroSequence()
    {
        yield return null;

        for (int i = 0; i < 30 && target == null; i++)
        {
            ResolveTarget();
            if (target != null)
                break;
            yield return null;
        }

        yield return ShowGuideThenHide();

        if (target == null)
        {
            Destroy(gameObject);
            yield break;
        }

        yield return ShowMissionPanel();
    }

    private IEnumerator ShowMissionPanel()
    {
        const float fadeIn = 0.35f;
        float elapsed = 0f;
        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            missionGroup.alpha = Mathf.Clamp01(elapsed / fadeIn);
            yield return null;
        }

        missionGroup.alpha = 1f;
        isMissionVisible = true;
        UpdateDistanceAndArrow();
    }

    private IEnumerator ShowGuideThenHide()
    {
        if (guideGroup == null)
            yield break;

        const float fadeIn = 0.25f;
        const float fadeOut = 0.35f;
        float elapsed = 0f;

        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            guideGroup.alpha = Mathf.Clamp01(elapsed / fadeIn);
            yield return null;
        }

        guideGroup.alpha = 1f;
        yield return new WaitForSeconds(Mathf.Max(0f, guideDisplayDuration - fadeIn - fadeOut));

        elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed += Time.deltaTime;
            guideGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOut);
            yield return null;
        }

        guideGroup.alpha = 0f;
        if (guideCanvasGo != null)
            Destroy(guideCanvasGo);
    }

    private IEnumerator HideMissionAndDestroy()
    {
        if (missionGroup == null)
        {
            Destroy(gameObject);
            yield break;
        }

        const float fadeOut = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed += Time.deltaTime;
            missionGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOut);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void UpdateDistanceAndArrow()
    {
        if (player == null || target == null)
            return;

        var flatPlayer = new Vector3(player.position.x, 0f, player.position.z);
        var flatObjective = new Vector3(target.position.x, 0f, target.position.z);
        float distance = Vector3.Distance(flatPlayer, flatObjective);

        if (distanceText != null)
            distanceText.text = "Khoảng cách: " + Mathf.RoundToInt(distance) + " m";

        UpdateArrow(arrowRect, flatPlayer, flatObjective);
    }
}
