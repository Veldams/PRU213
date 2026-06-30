using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MainMenuBackgroundFit : MonoBehaviour
{
    [Header("Composition")]
    [SerializeField] [Range(0.75f, 1f)] private float widthCoverage = 0.89f;

    [Header("Backdrop")]
    [SerializeField] private Color backdropColor = new Color(0.04f, 0.05f, 0.07f, 1f);

    private RectTransform rectTransform;
    private Image image;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
        image = GetComponent<Image>();
        image.type = Image.Type.Simple;
        image.preserveAspect = false;

        EnsureBackdrop();
    }

    private void Start()
    {
        ApplyComposition();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (rectTransform == null || image == null)
            return;

        ApplyComposition();
    }

    private void EnsureBackdrop()
    {
        var parent = rectTransform.parent as RectTransform;
        if (parent == null)
            return;

        Transform existing = null;
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == "MainMenuBackdrop")
            {
                existing = child;
                break;
            }
        }

        GameObject backdropGo;
        if (existing != null)
        {
            backdropGo = existing.gameObject;
        }
        else
        {
            backdropGo = new GameObject("MainMenuBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backdropGo.transform.SetParent(parent, false);
            backdropGo.transform.SetSiblingIndex(0);
        }

        var backdropImage = backdropGo.GetComponent<Image>();
        backdropImage.color = backdropColor;
        backdropImage.raycastTarget = false;

        var backdropRect = (RectTransform)backdropGo.transform;
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.pivot = new Vector2(0.5f, 0.5f);
        backdropRect.anchoredPosition = Vector2.zero;
        backdropRect.sizeDelta = Vector2.zero;
    }

    private void ApplyComposition()
    {
        if (image.sprite == null)
            return;

        var parent = rectTransform.parent as RectTransform;
        if (parent == null)
            return;

        float parentWidth = parent.rect.width;
        float parentHeight = parent.rect.height;
        if (parentWidth <= 0f || parentHeight <= 0f)
            return;

        float spriteWidth = image.sprite.rect.width;
        float spriteHeight = image.sprite.rect.height;
        if (spriteWidth <= 0f || spriteHeight <= 0f)
            return;

        float imageAspect = spriteWidth / spriteHeight;

        float displayWidth = parentWidth * widthCoverage;
        float displayHeight = displayWidth / imageAspect;

        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.sizeDelta = new Vector2(displayWidth, displayHeight);

        // Ưu tiên còng tay: luôn neo đáy ảnh sát đáy màn hình khi ảnh cao hơn khung.
        float yPos = displayHeight > parentHeight
            ? 0f
            : (parentHeight - displayHeight) * 0.5f;

        rectTransform.anchoredPosition = new Vector2(0f, yPos);
    }
}
