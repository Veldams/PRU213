using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared HUD helpers for objective UI scripts.
/// </summary>
public abstract class ObjectiveHudBase : MonoBehaviour
{
    protected Transform player;
    protected float nextHudRefresh;

    protected void ResolvePlayerOnce()
    {
        if (player != null)
            return;

        var movementTransform = PlayerSceneTransition.FindPlayerTransform();
        if (movementTransform != null)
            player = movementTransform;
    }

    protected bool ShouldRefreshHud()
    {
        if (Time.time < nextHudRefresh)
            return false;

        nextHudRefresh = Time.time + 0.15f;
        return true;
    }

    protected static Font GetUiFont()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    protected static Text CreateLabel(Transform parent, string name, Font font, int size, FontStyle style,
        Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor anchor, string text)
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
        label.alignment = anchor;
        label.color = Color.white;
        label.text = text;
        label.raycastTarget = false;
        return label;
    }

    protected void UpdateArrow(RectTransform arrowRect, Vector3 flatPlayer, Vector3 flatObjective)
    {
        if (arrowRect == null)
            return;

        var cam = PlayerSceneTransition.GetActiveCamera();
        if (cam == null)
            return;

        Vector3 toTarget = flatObjective - flatPlayer;
        if (toTarget.sqrMagnitude < 0.01f)
            return;

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude < 0.001f)
            return;

        camForward.Normalize();
        float angle = Vector3.SignedAngle(camForward, toTarget.normalized, Vector3.up);
        arrowRect.localRotation = Quaternion.Euler(0f, 0f, -angle);
    }
}
