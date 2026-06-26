using UnityEngine;
using UnityEngine.SceneManagement;

public static class RiverEnvironmentRuntime
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SetupRiverEnvironment()
    {
        SetupForScene(SceneManager.GetActiveScene());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupForScene(scene);
    }

    private static void SetupForScene(Scene scene)
    {
        GameObject[] rivers = FindRiverObjects();
        for (int i = 0; i < rivers.Length; i++)
        {
            GameObject river = rivers[i];
            if (river.GetComponent<RiverWaveAnimator>() == null)
            {
                river.AddComponent<RiverWaveAnimator>();
            }
        }
    }

    private static void EnsurePlayerCharacterControllers()
    {
        RealMovement[] realMovers = Object.FindObjectsByType<RealMovement>(FindObjectsSortMode.None);
        foreach (RealMovement mover in realMovers)
        {
            EnsureCharacterController(mover.gameObject);
        }

        ProceduralMovement[] proceduralMovers = Object.FindObjectsByType<ProceduralMovement>(FindObjectsSortMode.None);
        foreach (ProceduralMovement mover in proceduralMovers)
        {
            EnsureCharacterController(mover.gameObject);
        }
    }

    private static void EnsureCharacterController(GameObject target)
    {
        if (target.GetComponent<CharacterController>() != null) return;

        CharacterController controller = target.AddComponent<CharacterController>();
        controller.slopeLimit = 60f;
        controller.stepOffset = 0.3f;
        controller.skinWidth = 0.08f;
        controller.minMoveDistance = 0f;

        Bounds bounds = EstimateCharacterBounds(target);
        float estimatedHeight = Mathf.Clamp(bounds.size.y, 1.4f, 2.4f);
        controller.height = estimatedHeight;
        controller.radius = Mathf.Max(0.25f, bounds.size.x * 0.25f);
        controller.center = new Vector3(0f, estimatedHeight * 0.5f, 0f);
    }

    private static Bounds EstimateCharacterBounds(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(target.transform.position, new Vector3(1f, 1.8f, 1f));
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static GameObject[] FindRiverObjects()
    {
        System.Collections.Generic.List<GameObject> rivers = new System.Collections.Generic.List<GameObject>();

        MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        foreach (MeshRenderer meshRenderer in renderers)
        {
            string lowerName = meshRenderer.gameObject.name.ToLower();
            if (lowerName.Contains("river"))
            {
                rivers.Add(meshRenderer.gameObject);
            }
        }

        return rivers.ToArray();
    }

    private static void EnsureRailings(GameObject river, int riverIndex)
    {
        string rootName = "RiverRailings_Runtime_" + riverIndex;
        if (GameObject.Find(rootName) != null) return;

        Renderer riverRenderer = river.GetComponent<Renderer>();
        Bounds bounds = riverRenderer != null
            ? riverRenderer.bounds
            : new Bounds(river.transform.position, new Vector3(40f, 1f, 200f));

        GameObject root = new GameObject(rootName);
        BuildRailPair(root.transform, bounds);
    }

    private static void BuildRailPair(Transform root, Bounds bounds)
    {
        float colliderHeight = 2f;
        float colliderThickness = 0.35f;
        float sideOffset = 0.25f;
        float lengthPadding = 1.5f;

        bool alongZ = bounds.size.z >= bounds.size.x;
        if (alongZ)
        {
            float y = bounds.max.y + colliderHeight * 0.5f;
            float zLen = bounds.size.z + lengthPadding;

            CreateRailCollider(
                root,
                "RiverRail_Left",
                new Vector3(bounds.min.x - sideOffset, y, bounds.center.z),
                new Vector3(colliderThickness, colliderHeight, zLen)
            );

            CreateRailCollider(
                root,
                "RiverRail_Right",
                new Vector3(bounds.max.x + sideOffset, y, bounds.center.z),
                new Vector3(colliderThickness, colliderHeight, zLen)
            );
        }
        else
        {
            float y = bounds.max.y + colliderHeight * 0.5f;
            float xLen = bounds.size.x + lengthPadding;

            CreateRailCollider(
                root,
                "RiverRail_Front",
                new Vector3(bounds.center.x, y, bounds.min.z - sideOffset),
                new Vector3(xLen, colliderHeight, colliderThickness)
            );

            CreateRailCollider(
                root,
                "RiverRail_Back",
                new Vector3(bounds.center.x, y, bounds.max.z + sideOffset),
                new Vector3(xLen, colliderHeight, colliderThickness)
            );
        }
    }

    private static void CreateRailCollider(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject rail = new GameObject(name);
        rail.name = name;
        rail.transform.SetParent(parent, true);
        rail.transform.position = position;
        rail.transform.rotation = Quaternion.identity;
        rail.transform.localScale = scale;
        BoxCollider boxCollider = rail.AddComponent<BoxCollider>();
        boxCollider.isTrigger = false;
        boxCollider.size = Vector3.one;
    }
}

[RequireComponent(typeof(MeshFilter))]
public class RiverWaveAnimator : MonoBehaviour
{
    [SerializeField] private float waveAmplitude = 0.08f;
    [SerializeField] private float waveFrequency = 1.4f;
    [SerializeField] private float waveSpeed = 1.2f;
    [SerializeField] private Vector2 uvPanSpeed = new Vector2(0.03f, 0.015f);

    private MeshFilter meshFilter;
    private Mesh runtimeMesh;
    private Vector3[] baseVertices;
    private Vector3[] animatedVertices;
    private Material runtimeMaterial;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        runtimeMesh = Instantiate(meshFilter.sharedMesh);
        runtimeMesh.name = meshFilter.sharedMesh.name + "_RuntimeWave";
        meshFilter.sharedMesh = runtimeMesh;

        baseVertices = runtimeMesh.vertices;
        animatedVertices = new Vector3[baseVertices.Length];

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            runtimeMaterial = meshRenderer.material;
            if (runtimeMaterial != null && runtimeMaterial.HasProperty("_BaseColor"))
            {
                runtimeMaterial.SetColor("_BaseColor", new Color(0.06f, 0.22f, 0.34f, 1f));
            }
        }
    }

    private void Update()
    {
        if (runtimeMesh == null || baseVertices == null) return;

        float t = Time.time * waveSpeed;
        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 vertex = baseVertices[i];
            float wave =
                Mathf.Sin((vertex.x + vertex.z) * waveFrequency + t) * waveAmplitude +
                Mathf.Cos(vertex.x * (waveFrequency * 0.6f) - t * 0.8f) * (waveAmplitude * 0.5f);

            animatedVertices[i] = new Vector3(vertex.x, vertex.y + wave, vertex.z);
        }

        runtimeMesh.vertices = animatedVertices;
        runtimeMesh.RecalculateNormals();

        if (runtimeMaterial != null && runtimeMaterial.mainTexture != null)
        {
            Vector2 offset = runtimeMaterial.mainTextureOffset;
            offset += uvPanSpeed * Time.deltaTime;
            runtimeMaterial.mainTextureOffset = offset;
            if (runtimeMaterial.HasProperty("_BaseMap"))
            {
                runtimeMaterial.SetTextureOffset("_BaseMap", offset);
            }
        }
    }
}
