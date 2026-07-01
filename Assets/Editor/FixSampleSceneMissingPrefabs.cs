#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FixSampleSceneMissingPrefabs
{
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    private const string WorkingModelPath =
        "Assets/abandoned+brick+building+3d+model/tripo_convert_cac2e472-cd38-4f25-9dce-93f90e2f91b1 1.obj";
    private const string BrokenModelPath =
        "Assets/abandoned+brick+building+3d+model/tripo_convert_cac2e472-cd38-4f25-9dce-93f90e2f91b1.obj";

    [MenuItem("Tools/Sample Scene/Fix Missing Abandoned Building Prefabs")]
    public static void FixMissingAbandonedBuildingPrefabs()
    {
        EnsureModelImported(WorkingModelPath);
        AssetDatabase.ImportAsset(BrokenModelPath, ImportAssetOptions.ForceUpdate);

        var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        int removed = RemoveDuplicateAbandonedBuildings(scene);
        int reconnected = ReconnectMissingPrefabInstances(WorkingModelPath);

        SampleSceneCollisionLightener.SetupSampleSceneCollisions();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[FixSampleSceneMissingPrefabs] Removed " + removed
            + " duplicate building(s), reconnected " + reconnected + " missing prefab instance(s).");
    }

    private static void EnsureModelImported(string modelPath)
    {
        var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError("[FixSampleSceneMissingPrefabs] Model not found: " + modelPath);
            return;
        }

        importer.addCollider = false;
        importer.meshCompression = ModelImporterMeshCompression.Medium;
        importer.SaveAndReimport();
    }

    private static int RemoveDuplicateAbandonedBuildings(Scene scene)
    {
        int removed = 0;
        var roots = scene.GetRootGameObjects();

        for (int i = roots.Length - 1; i >= 0; i--)
        {
            var root = roots[i];
            if (root == null)
                continue;

            string name = root.name;
            if (!name.Contains("cac2e472-cd38-4f25-9dce-93f90e2f91b1"))
                continue;

            if (name.Contains("(2)") || name.Contains("(3)") || name.EndsWith("(1)"))
            {
                Object.DestroyImmediate(root);
                removed++;
            }
        }

        return removed;
    }

    private static int ReconnectMissingPrefabInstances(string modelPath)
    {
        int fixedCount = 0;
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (source == null)
            return 0;

        var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            var go = transforms[i].gameObject;
            if (go == null || !go.scene.IsValid())
                continue;

            if (!PrefabUtility.IsPartOfPrefabInstance(go))
                continue;

            if (!PrefabUtility.IsPrefabAssetMissing(go))
                continue;

            if (!go.name.Contains("cac2e472-cd38-4f25-9dce-93f90e2f91b1"))
                continue;

            var root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (root == null)
                continue;

            var position = root.transform.position;
            var rotation = root.transform.rotation;
            var scale = root.transform.localScale;
            var scene = root.scene;

            Object.DestroyImmediate(root);

            var replacement = (GameObject)PrefabUtility.InstantiatePrefab(source, scene);
            replacement.transform.SetPositionAndRotation(position, rotation);
            replacement.transform.localScale = scale;
            fixedCount++;
        }

        return fixedCount;
    }
}
#endif
