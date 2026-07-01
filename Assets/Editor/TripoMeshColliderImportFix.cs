#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class TripoMeshColliderImportFix : AssetPostprocessor
{
    private void OnPostprocessModel(GameObject root)
    {
        if (!ShouldProcess(assetPath))
            return;

        RemoveHeavyMeshColliders(root);
        SampleSceneCollisionLightener.TryAddBuildingBoxCollider(root);
    }

    private static bool ShouldProcess(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return path.Contains("tripo")
            || path.Contains("building")
            || path.Contains("shopfront")
            || path.Contains("street+scene");
    }

    private static void RemoveHeavyMeshColliders(GameObject root)
    {
        var colliders = root.GetComponentsInChildren<MeshCollider>(true);
        for (int i = colliders.Length - 1; i >= 0; i--)
        {
            var collider = colliders[i];
            if (collider == null || collider.sharedMesh == null)
                continue;

            string meshName = collider.sharedMesh.name ?? string.Empty;
            if (meshName == "default" || meshName.StartsWith("tripo_node") || collider.sharedMesh.vertexCount >= 100_000)
                Object.DestroyImmediate(collider);
        }
    }
}

public static class TripoMeshColliderReimport
{
    [MenuItem("Tools/Reimport Tripo / High-Poly Models")]
    public static void ReimportTripoModels()
    {
        int reimported = 0;
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
        for (int i = 0; i < modelGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(modelGuids[i]);
            if (string.IsNullOrEmpty(path))
                continue;

            if (!path.Contains("tripo") && !path.Contains("building"))
                continue;

            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null || !importer.addCollider)
                continue;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            reimported++;
        }

        Debug.Log("[TripoMeshColliderReimport] Reimported " + reimported + " model(s).");
    }
}
#endif
