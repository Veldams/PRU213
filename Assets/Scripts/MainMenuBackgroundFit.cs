using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MainMenuBackgroundFit : MonoBehaviour
{
    [Header("Backdrop")]
    [SerializeField] private Color backdropColor = new Color(0.07f, 0.065f, 0.06f, 1f);
    [SerializeField] private Color sideFadeColor = new Color(0.05f, 0.045f, 0.04f, 1f);

    private const int FadeTextureWidth = 256;
    private const string SideFadeLeftName = "MainMenuSideFadeLeft";
    private const string SideFadeRightName = "MainMenuSideFadeRight";

    private static Sprite fadeLeftSprite;
    private static Sprite fadeRightSprite;

    private RectTransform rectTransform;
    private Image image;
    private RectTransform sideFadeLeft;
    private RectTransform sideFadeRight;

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

        RemoveLegacyFillLayer(parent);
        EnsureFadeSprites();

        var backdropGo = FindOrCreateChild(parent, "MainMenuBackdrop");
        var backdropImage = backdropGo.GetComponent<Image>();
        backdropImage.color = backdropColor;
        backdropImage.raycastTarget = false;
        backdropImage.sprite = null;
        StretchFull(backdropGo.GetComponent<RectTransform>());

        sideFadeLeft = SetupSideFade(parent, SideFadeLeftName, fadeLeftSprite, true);
        sideFadeRight = SetupSideFade(parent, SideFadeRightName, fadeRightSprite, false);

        backdropGo.transform.SetAsFirstSibling();
        sideFadeLeft.SetSiblingIndex(1);
        sideFadeRight.SetSiblingIndex(2);
        if (rectTransform.GetSiblingIndex() != 3)
            rectTransform.SetSiblingIndex(3);
    }

    private static void RemoveLegacyFillLayer(RectTransform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child.name == "MainMenuBackdropFill")
                Object.Destroy(child.gameObject);
        }
    }

    private static void EnsureFadeSprites()
    {
        if (fadeLeftSprite != null && fadeRightSprite != null)
            return;

        fadeLeftSprite = CreateHorizontalFadeSprite(false);
        fadeRightSprite = CreateHorizontalFadeSprite(true);
    }

    private static Sprite CreateHorizontalFadeSprite(bool mirror)
    {
        var texture = new Texture2D(FadeTextureWidth, 1, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        for (int x = 0; x < FadeTextureWidth; x++)
        {
            float t = x / (FadeTextureWidth - 1f);
            if (mirror)
                t = 1f - t;

            float alpha = t * t * (3f - 2f * t);
            texture.SetPixel(x, 0, new Color(1f, 1f, 1f, alpha));
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, FadeTextureWidth, 1f), new Vector2(0.5f, 0.5f), 100f);
    }

    private RectTransform SetupSideFade(RectTransform parent, string name, Sprite sprite, bool anchorLeft)
    {
        var go = FindOrCreateChild(parent, name);
        var fadeImage = go.GetComponent<Image>();
        fadeImage.sprite = sprite;
        fadeImage.color = sideFadeColor;
        fadeImage.raycastTarget = false;
        fadeImage.type = Image.Type.Simple;
        fadeImage.preserveAspect = false;

        var rect = go.GetComponent<RectTransform>();
        if (anchorLeft)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }
        else
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }

        rect.sizeDelta = new Vector2(0f, 0f);
        return rect;
    }

    private static GameObject FindOrCreateChild(RectTransform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == name)
                return child.gameObject;
        }

        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
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
        float parentAspect = parentWidth / parentHeight;

        ComputeContainSize(imageAspect, parentAspect, parentWidth, parentHeight, out float displayWidth, out float displayHeight);

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(displayWidth, displayHeight);
        rectTransform.anchoredPosition = Vector2.zero;

        UpdateSideFades(parentWidth, displayWidth);
    }

    private void UpdateSideFades(float parentWidth, float displayWidth)
    {
        float margin = Mathf.Max(0f, (parentWidth - displayWidth) * 0.5f);
        bool showFades = margin > 4f;

        if (sideFadeLeft != null)
        {
            sideFadeLeft.gameObject.SetActive(showFades);
            if (showFades)
                sideFadeLeft.sizeDelta = new Vector2(margin, 0f);
        }

        if (sideFadeRight != null)
        {
            sideFadeRight.gameObject.SetActive(showFades);
            if (showFades)
                sideFadeRight.sizeDelta = new Vector2(margin, 0f);
        }
    }

    private static void ComputeContainSize(
        float imageAspect,
        float parentAspect,
        float parentWidth,
        float parentHeight,
        out float displayWidth,
        out float displayHeight)
    {
        if (imageAspect > parentAspect)
        {
            displayWidth = parentWidth;
            displayHeight = displayWidth / imageAspect;
            return;
        }

        displayHeight = parentHeight;
        displayWidth = displayHeight * imageAspect;
    }
}
