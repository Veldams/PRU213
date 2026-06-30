using UnityEngine;

/// <summary>
/// Floating world-space marker above an objective point so players can spot the destination.
/// </summary>
public class ObjectiveWorldMarker : MonoBehaviour
{
    [SerializeField] private float floatHeight = 6f;
    [SerializeField] private float bobAmplitude = 0.35f;
    [SerializeField] private float bobSpeed = 2.2f;
    [SerializeField] private float spinSpeed = 45f;

    private Vector3 anchor;
    private Transform beaconRoot;
    private Transform labelRoot;

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

        beaconRoot = new GameObject("Beacon").transform;
        beaconRoot.SetParent(transform, false);

        var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "Pillar";
        pillar.transform.SetParent(beaconRoot, false);
        pillar.transform.localScale = new Vector3(0.35f, 1.2f, 0.35f);
        pillar.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        ApplyMaterial(pillar, new Color(1f, 0.82f, 0.22f), 0.85f, new Color(0.35f, 0.25f, 0.05f));

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Ring";
        ring.transform.SetParent(beaconRoot, false);
        ring.transform.localScale = new Vector3(1.1f, 0.06f, 1.1f);
        ring.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        ApplyMaterial(ring, new Color(1f, 0.88f, 0.35f), 0.9f, new Color(0.4f, 0.3f, 0.08f));

        var lightGo = new GameObject("MarkerLight");
        lightGo.transform.SetParent(beaconRoot, false);
        lightGo.transform.localPosition = new Vector3(0f, 2.8f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.85f, 0.45f);
        light.intensity = 1.4f;
        light.range = 12f;
        light.shadows = LightShadows.None;

        labelRoot = new GameObject("Label").transform;
        labelRoot.SetParent(beaconRoot, false);
        labelRoot.localPosition = new Vector3(0f, 3.4f, 0f);

        var labelGo = new GameObject("Text");
        labelGo.transform.SetParent(labelRoot, false);
        labelGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var tm = labelGo.AddComponent<TextMesh>();
        tm.text = label;
        tm.characterSize = 0.12f;
        tm.fontSize = 64;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = new Color(1f, 0.92f, 0.55f);

        var mr = labelGo.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        transform.position = anchor + Vector3.up * floatHeight;
    }

    private static void ApplyMaterial(GameObject go, Color color, float smoothness, Color emission)
    {
        var col = go.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = color;
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);
        if (emission.maxColorComponent > 0.01f)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
        }

        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private void LateUpdate()
    {
        if (beaconRoot == null)
            return;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = anchor + Vector3.up * (floatHeight + bob);
        beaconRoot.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        var cam = Camera.main;
        if (cam != null && labelRoot != null)
        {
            var toCam = cam.transform.position - labelRoot.position;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.01f)
                labelRoot.rotation = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
        }
    }
}
