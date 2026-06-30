using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Objective HUD for SampleScene: guides the player toward the police building
/// (tripo_convert_4af7160d-b155-4df9-9afc-9f6cba929dda) and hides once they arrive.
/// </summary>
public class ObjectiveGuideUI : MonoBehaviour
{
    private const string TargetObjectName = "tripo_convert_4af7160d-b155-4df9-9afc-9f6cba929dda";
    private const string SampleSceneName = "SampleScene";

    [SerializeField] private string objectiveTitle = "NHIỆM VỤ";
    [SerializeField] private string objectiveMessage = "Hãy đi đến công sở công an trên con đường";
    [SerializeField] private string worldMarkerLabel = "CÔNG AN";
    [SerializeField] private float showDelay = 8.5f;
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.6f;
    [SerializeField] private float completeDistance = 6f;

    private Transform target;
    private Collider targetTrigger;
    private Transform player;
    private CanvasGroup canvasGroup;
    private Text titleText;
    private Text messageText;
    private Text distanceText;
    private RectTransform arrowRect;
    private ObjectiveWorldMarker worldMarker;
    private bool isVisible;
    private bool isCompleted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapInSampleScene()
    {
        if (SceneManager.GetActiveScene().name != SampleSceneName)
            return;

        if (FindFirstObjectByType<ObjectiveGuideUI>() != null)
            return;

        var root = new GameObject("ObjectiveGuideUI");
        root.AddComponent<ObjectiveGuideUI>();
    }

    private void Start()
    {
        ResolveTarget();
        if (target != null)
            worldMarker = ObjectiveWorldMarker.Create(transform, GetObjectivePoint(), worldMarkerLabel);

        BuildUi();
        StartCoroutine(ShowAfterDelay());
    }

    private void Update()
    {
        if (isCompleted || target == null)
            return;

        ResolvePlayer();
        if (player == null)
            return;

        if (HasReachedObjective())
        {
            isCompleted = true;
            StartCoroutine(HideAndDestroy());
            return;
        }

        if (!isVisible)
            return;

        UpdateDistanceAndArrow();
    }

    private void ResolveTarget()
    {
        var go = GameObject.Find(TargetObjectName);
        if (go == null)
        {
            Debug.LogWarning("[ObjectiveGuideUI] Target not found: " + TargetObjectName);
            return;
        }

        target = go.transform;
        targetTrigger = go.GetComponent<Collider>();
    }

    private void ResolvePlayer()
    {
        if (player != null)
            return;

        var movement = FindFirstObjectByType<RealMovement>();
        if (movement != null)
            player = movement.transform;
    }

    private Vector3 GetObjectivePoint()
    {
        if (targetTrigger != null)
            return targetTrigger.bounds.center;

        return target != null ? target.position : Vector3.zero;
    }

    private bool HasReachedObjective()
    {
        if (player == null || target == null)
            return false;

        var cc = player.GetComponent<CharacterController>();
        if (targetTrigger != null && cc != null && targetTrigger.bounds.Intersects(cc.bounds))
            return true;

        var objective = GetObjectivePoint();
        var flatPlayer = new Vector3(player.position.x, 0f, player.position.z);
        var flatObjective = new Vector3(objective.x, 0f, objective.z);
        return Vector3.Distance(flatPlayer, flatObjective) <= completeDistance;
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
        canvas.sortingOrder = 85;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var panelGo = new GameObject("ObjectivePanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -24f);
        panelRect.sizeDelta = new Vector2(820f, 110f);

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.10f, 0.18f, 0.88f);

        canvasGroup = panelGo.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        var font = GetUiFont();

        titleText = CreateLabel(panelGo.transform, "Title", font, 24, FontStyle.Bold,
            new Vector2(-270f, -18f), new Vector2(220f, 30f), TextAnchor.MiddleLeft, objectiveTitle);

        messageText = CreateLabel(panelGo.transform, "Message", font, 28, FontStyle.Normal,
            new Vector2(20f, -18f), new Vector2(460f, 34f), TextAnchor.MiddleLeft, objectiveMessage);

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
    }

    private static Text CreateLabel(Transform parent, string name, Font font, int size, FontStyle style,
        Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor anchor, string text)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        var label = go.AddComponent<Text>();
        label.font = font;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = anchor;
        label.color = Color.white;
        label.text = text;
        return label;
    }

    private IEnumerator ShowAfterDelay()
    {
        if (target == null)
        {
            Destroy(gameObject);
            yield break;
        }

        if (HasReachedObjective())
        {
            Destroy(gameObject);
            yield break;
        }

        yield return new WaitForSeconds(showDelay);

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        isVisible = true;
        UpdateDistanceAndArrow();
    }

    private IEnumerator HideAndDestroy()
    {
        if (worldMarker != null)
            Destroy(worldMarker.gameObject);

        if (canvasGroup == null)
        {
            Destroy(gameObject);
            yield break;
        }

        if (messageText != null)
            messageText.text = "Đã đến công sở công an!";

        if (distanceText != null)
            distanceText.text = string.Empty;

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        Destroy(gameObject);
    }

    private void UpdateDistanceAndArrow()
    {
        if (player == null || target == null)
            return;

        var objective = GetObjectivePoint();
        var flatPlayer = new Vector3(player.position.x, 0f, player.position.z);
        var flatObjective = new Vector3(objective.x, 0f, objective.z);
        float distance = Vector3.Distance(flatPlayer, flatObjective);

        if (distanceText != null)
            distanceText.text = "Khoảng cách: " + Mathf.RoundToInt(distance) + " m";

        var cam = Camera.main;
        if (cam == null || arrowRect == null)
            return;

        Vector3 toTarget = flatObjective - flatPlayer;
        if (toTarget.sqrMagnitude < 0.01f)
            return;

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        float angle = Vector3.SignedAngle(camForward, toTarget.normalized, Vector3.up);
        arrowRect.localRotation = Quaternion.Euler(0f, 0f, -angle);
    }
}
