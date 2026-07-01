#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SampleSceneCollisionLightener
{
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    private const int HeavyVertexThreshold = 100_000;

    [MenuItem("Tools/Sample Scene/Fast Load Collision Setup")]
    public static void SetupSampleSceneCollisions()
    {
        Scene scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);

        int removed = RemoveHeavyMeshColliders(scene);
        int boxes = EnsureBuildingBoxColliders(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[SampleSceneCollisionLightener] Removed " + removed + " heavy mesh collider(s), ensured " + boxes + " building box collider(s).");
    }

    private static int RemoveHeavyMeshColliders(Scene scene)
    {
        int removed = 0;
        var meshColliders = Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include);

        for (int i = 0; i < meshColliders.Length; i++)
        {
            var collider = meshColliders[i];
            if (collider == null || !collider.gameObject.scene.IsValid() || collider.gameObject.scene != scene)
                continue;

            if (!IsHeavyCollider(collider))
                continue;

            Object.DestroyImmediate(collider);
            removed++;
        }

        return removed;
    }

    private static bool IsHeavyCollider(MeshCollider collider)
    {
        Mesh mesh = collider.sharedMesh;
        if (mesh == null)
            return false;

        string meshName = mesh.name ?? string.Empty;
        if (meshName == "default" || meshName.StartsWith("tripo_node"))
            return true;

        return mesh.vertexCount >= HeavyVertexThreshold;
    }

    private static int EnsureBuildingBoxColliders(Scene scene)
    {
        int added = 0;
        var roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root == null || !IsBuildingRoot(root.name))
                continue;

            if (TryAddBuildingBoxCollider(root))
                added++;
        }

        return added;
    }

    private static bool IsBuildingRoot(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return false;

        return objectName.Contains("tripo_convert")
            || objectName.Contains("building")
            || objectName.Contains("shopfront")
            || objectName.Contains("street+scene");
    }

    public static bool TryAddBuildingBoxCollider(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return false;

        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            worldBounds.Encapsulate(renderers[i].bounds);

        var box = root.GetComponent<BoxCollider>();
        if (box == null)
            box = root.AddComponent<BoxCollider>();

        Vector3 lossyScale = root.transform.lossyScale;
        box.center = root.transform.InverseTransformPoint(worldBounds.center);
        box.size = new Vector3(
            SafeDivide(worldBounds.size.x, lossyScale.x),
            SafeDivide(worldBounds.size.y, lossyScale.y),
            SafeDivide(worldBounds.size.z, lossyScale.z));

        return true;
    }

    private static float SafeDivide(float value, float scale)
    {
        if (Mathf.Approximately(scale, 0f))
            return value;

        return value / Mathf.Abs(scale);
    }
}
#endif
