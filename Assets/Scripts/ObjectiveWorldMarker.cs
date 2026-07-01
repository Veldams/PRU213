using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight world-space label above an objective. No lights, no mesh primitives.
/// </summary>
public class ObjectiveWorldMarker : MonoBehaviour
{
    [SerializeField] private float floatHeight = 3.5f;

    private Vector3 anchor;
    private float nextBillboard;

    public static ObjectiveWorldMarker Create(Transform parent, Vector3 worldAnchor, string label)
    {
        var go = new GameObject("ObjectiveWorldMarker");
        go.transform.SetParent(parent, false);
        var marker = go.AddComponent<ObjectiveWorldMarker>();
        marker.Initialize(worldAnchor, label);
        return marker;
    }

    private void Initialize(Vector3 worldAnchor, string label)
    {
        anchor = worldAnchor;
        transform.position = anchor + Vector3.up * floatHeight;

        var canvasGo = new GameObject("LabelCanvas");
        canvasGo.transform.SetParent(transform, false);
        canvasGo.transform.localPosition = Vector3.zero;

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rect = canvasGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320f, 80f);
        rect.localScale = Vector3.one * 0.012f;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(canvasGo.transform, false);

        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 28;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.92f, 0.55f);
        text.text = label;
        text.raycastTarget = false;
    }

    private void LateUpdate()
    {
        if (!ProximityUtil.ShouldRun(ref nextBillboard, 0.15f))
            return;

        var cam = PlayerSceneTransition.GetActiveCamera();
        if (cam == null)
            return;

        var toCam = cam.transform.position - transform.position;
        if (toCam.sqrMagnitude < 0.01f)
            return;

        // World-space UI faces -Z; rotate so the label reads toward the camera.
        transform.rotation = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
    }
}
