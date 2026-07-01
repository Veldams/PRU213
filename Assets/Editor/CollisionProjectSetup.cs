using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CollisionProjectSetup
{
    [MenuItem("Tools/Collision/Apply Project-Wide Collision Setup")]
    public static void ApplyProjectWideCollisionSetup()
    {
        int importersUpdated = EnableModelImportColliders();
        int scenesUpdated = 0;
        int collidersAdded = 0;
        int collidersHardened = 0;

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(scenePath))
            {
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int addedInScene = AddMissingEnvironmentColliders(scene);
            int hardenedInScene = HardenLargeMeshColliders(scene);
            if (addedInScene > 0)
            {
                scenesUpdated++;
                collidersAdded += addedInScene;
            }
            if (hardenedInScene > 0)
            {
                scenesUpdated++;
                collidersHardened += hardenedInScene;
            }
            if (addedInScene > 0 || hardenedInScene > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[CollisionSetup] Done. Model importers updated: {importersUpdated}, scenes updated: {scenesUpdated}, colliders added: {collidersAdded}, colliders hardened: {collidersHardened}.");
    }

    static int EnableModelImportColliders()
    {
        int updated = 0;
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
        foreach (string guid in modelGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                continue;
            }

            if (!importer.addCollider)
            {
                importer.addCollider = true;
                importer.SaveAndReimport();
                updated++;
            }
        }

        return updated;
    }

    static int AddMissingEnvironmentColliders(Scene scene)
    {
        int added = 0;
        var meshFilters = Object.FindObjectsByType<MeshFilter>();
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf == null || mf.sharedMesh == null)
            {
                continue;
            }

            GameObject go = mf.gameObject;
            if (!go.scene.IsValid() || go.scene != scene)
            {
                continue;
            }

            if (ShouldSkipObject(go))
            {
                continue;
            }

            if (go.GetComponent<Collider>() != null)
            {
                continue;
            }

            var meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mf.sharedMesh;
            meshCollider.convex = false;
            HardenMeshColliderIfNeeded(meshCollider);
            added++;
        }

        return added;
    }

    static int HardenLargeMeshColliders(Scene scene)
    {
        int hardened = 0;
        var meshColliders = Object.FindObjectsByType<MeshCollider>();
        foreach (var mc in meshColliders)
        {
            if (mc == null || mc.sharedMesh == null)
            {
                continue;
            }
            if (!mc.gameObject.scene.IsValid() || mc.gameObject.scene != scene)
            {
                continue;
            }
            if (HardenMeshColliderIfNeeded(mc))
            {
                hardened++;
            }
        }
        return hardened;
    }

    static bool HardenMeshColliderIfNeeded(MeshCollider mc)
    {
        return MeshColliderCookingFix.FixCollider(mc);
    }

    static bool ShouldSkipObject(GameObject go)
    {
        if (go.GetComponent<CharacterController>() != null)
        {
            return true;
        }

        if (go.GetComponent<Animator>() != null)
        {
            return true;
        }

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            return true;
        }

        string n = go.name.ToLowerInvariant();
        if (n.Contains("player") || n.Contains("character") || n.Contains("villain"))
        {
            return true;
        }

        return false;
    }
}
