#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MeshColliderCookingFix
{
    [MenuItem("Tools/Fix High-Poly Mesh Colliders")]
    public static void FixAllInOpenScenes()
    {
        int fixedCount = FixColliders(Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include));
        SaveIfNeeded(fixedCount, "open scene(s)");
    }

    [MenuItem("Tools/Fix High-Poly Mesh Colliders (All Project)")]
    public static void FixAllInProject()
    {
        int fixedCount = 0;

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            if (string.IsNullOrEmpty(path))
                continue;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
                continue;

            int fixedInPrefab = FixColliders(root.GetComponentsInChildren<MeshCollider>(true));
            if (fixedInPrefab > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                fixedCount += fixedInPrefab;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        string activeScenePath = SceneManager.GetActiveScene().path;

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            if (string.IsNullOrEmpty(scenePath))
                continue;

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int fixedInScene = FixColliders(Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include));
            if (fixedInScene > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                fixedCount += fixedInScene;
            }
        }

        if (!string.IsNullOrEmpty(activeScenePath) && File.Exists(activeScenePath))
            EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);

        AssetDatabase.SaveAssets();
        Debug.Log("[MeshColliderCookingFix] Project-wide fix updated " + fixedCount + " collider(s).");
    }

    private static int FixColliders(MeshCollider[] colliders)
    {
        int fixedCount = 0;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (FixCollider(colliders[i]))
                fixedCount++;
        }

        return fixedCount;
    }

    private static void SaveIfNeeded(int fixedCount, string scopeLabel)
    {
        if (fixedCount <= 0)
        {
            Debug.Log("[MeshColliderCookingFix] No colliders needed fixing in " + scopeLabel + ".");
            return;
        }

        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[MeshColliderCookingFix] Updated " + fixedCount + " collider(s) in " + scopeLabel + ".");
    }

    public static bool FixCollider(MeshCollider collider)
    {
        if (collider == null || collider.sharedMesh == null)
            return false;

        if (!MeshColliderHighPolyFix.ShouldDisableFastMidphase(collider.sharedMesh))
            return false;

        var current = collider.cookingOptions;
        var updated = current & ~MeshColliderCookingOptions.UseFastMidphase;
        if (current == updated)
            return false;

        Undo.RecordObject(collider, "Fix Mesh Collider Cooking");
        collider.cookingOptions = updated;
        EditorUtility.SetDirty(collider);

        var scene = collider.gameObject.scene;
        if (scene.IsValid())
            EditorSceneManager.MarkSceneDirty(scene);

        return true;
    }
}
#endif
