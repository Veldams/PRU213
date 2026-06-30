using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    private static InteractionPromptUI instance;

    private CanvasGroup canvasGroup;
    private Text promptText;

    public static void Show(string message)
    {
        GetOrCreate().SetPrompt(message, true);
    }

    public static void Hide()
    {
        if (instance == null)
            return;

        instance.SetPrompt(string.Empty, false);
    }

    private static InteractionPromptUI GetOrCreate()
    {
        if (instance != null)
            return instance;

        var existing = FindFirstObjectByType<InteractionPromptUI>();
        if (existing != null)
        {
            instance = existing;
            return instance;
        }

        var root = new GameObject("InteractionPromptUI");
        instance = root.AddComponent<InteractionPromptUI>();
        instance.BuildUi();
        return instance;
    }

    private void BuildUi()
    {
        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var panelGo = new GameObject("PromptPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 80f);
        panelRect.sizeDelta = new Vector2(320f, 56f);

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.07f, 0.07f, 0.12f, 0.88f);

        canvasGroup = panelGo.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(panelGo.transform, false);

        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        promptText = textGo.AddComponent<Text>();
        promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        promptText.fontSize = 32;
        promptText.fontStyle = FontStyle.Bold;
        promptText.supportRichText = false;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.color = Color.white;
        promptText.text = string.Empty;
    }

    private void SetPrompt(string message, bool visible)
    {
        if (promptText == null)
            BuildUi();

        promptText.text = message;
        canvasGroup.alpha = visible ? 1f : 0f;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
