using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Shows brief movement controls when SampleScene loads, then fades out and removes itself.
/// </summary>
public class MovementTutorialUI : MonoBehaviour
{
    [SerializeField] private float displayDuration = 8f;
    [SerializeField] private float fadeOutDuration = 0.8f;

    private CanvasGroup canvasGroup;

    private static MovementTutorialUI activeInstance;

    public static void TryCreateForScene(Scene scene)
    {
        if (scene.name != "SampleScene")
            return;

        if (SampleSceneGuideState.HasShownMovementTutorial)
            return;

        if (activeInstance != null)
            return;

        var root = new GameObject("MovementTutorialUI");
        root.AddComponent<MovementTutorialUI>();
    }

    private void Awake()
    {
        activeInstance = this;
        BuildUi();
        StartCoroutine(ShowThenHide());
    }

    private void OnDestroy()
    {
        if (activeInstance == this)
            activeInstance = null;
    }

    private static Font GetUiFont()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private void BuildUi()
    {
        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var panelGo = new GameObject("TutorialPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, 40f);
        panelRect.sizeDelta = new Vector2(560f, 300f);

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.06f, 0.08f, 0.12f, 0.88f);

        canvasGroup = panelGo.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        var font = GetUiFont();

        CreateLabel(panelGo.transform, "Title", font, 34, FontStyle.Bold,
            new Vector2(0f, 108f), new Vector2(520f, 44f), "HƯỚNG DẪN DI CHUYỂN");

        CreateLabel(panelGo.transform, "Line1", font, 26, FontStyle.Normal,
            new Vector2(0f, 52f), new Vector2(520f, 36f), "W / Mũi tên lên   : Tiến");

        CreateLabel(panelGo.transform, "Line2", font, 26, FontStyle.Normal,
            new Vector2(0f, 12f), new Vector2(520f, 36f), "S / Mũi tên xuống : Lùi");

        CreateLabel(panelGo.transform, "Line3", font, 26, FontStyle.Normal,
            new Vector2(0f, -28f), new Vector2(520f, 36f), "A / Mũi tên trái  : Xoay trái");

        CreateLabel(panelGo.transform, "Line4", font, 26, FontStyle.Normal,
            new Vector2(0f, -68f), new Vector2(520f, 36f), "D / Mũi tên phải  : Xoay phải");

        CreateLabel(panelGo.transform, "Line5", font, 26, FontStyle.Normal,
            new Vector2(0f, -108f), new Vector2(520f, 36f), "V                 : Đổi góc nhìn (FPS/TPS)");
    }

    private static void CreateLabel(Transform parent, string name, Font font, int size, FontStyle style,
        Vector2 anchoredPos, Vector2 sizeDelta, string text)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        var label = go.AddComponent<Text>();
        label.font = font;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.text = text;
    }

    private IEnumerator ShowThenHide()
    {
        float fadeInDuration = 0.35f;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(Mathf.Max(0f, displayDuration - fadeInDuration - fadeOutDuration));

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        SampleSceneGuideState.HasShownMovementTutorial = true;
        Destroy(gameObject);
    }
}
