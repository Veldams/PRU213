#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MeshColliderCookingFix
{
    private const int TriangleLimit = 2_097_152;
    private const int UseFastMidphaseFlag = 16;

    [MenuItem("Tools/Fix High-Poly Mesh Colliders")]
    public static void FixAllInOpenScenes()
    {
        int fixedCount = 0;
        var colliders = Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (FixCollider(colliders[i]))
                fixedCount++;
        }

        Debug.Log("[MeshColliderCookingFix] Updated " + fixedCount + " collider(s).");
    }

    public static bool FixCollider(MeshCollider collider)
    {
        if (collider == null || collider.sharedMesh == null)
            return false;

        long triangleCount = collider.sharedMesh.GetIndexCount(0) / 3;
        if (triangleCount <= TriangleLimit)
            return false;

        int current = (int)collider.cookingOptions;
        int updated = current & ~UseFastMidphaseFlag;
        if (updated == current)
            return false;

        Undo.RecordObject(collider, "Fix Mesh Collider Cooking");
        collider.cookingOptions = (MeshColliderCookingOptions)updated;
        EditorUtility.SetDirty(collider);
        return true;
    }
}
#endif
