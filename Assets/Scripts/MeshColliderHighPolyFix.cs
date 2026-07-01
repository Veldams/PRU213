using UnityEngine;

public static class MeshColliderHighPolyFix
{
    public static int FixAll(MeshCollider[] colliders)
    {
        int fixedCount = 0;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (FixCollider(colliders[i]))
                fixedCount++;
        }

        return fixedCount;
    }

    public static bool FixCollider(MeshCollider collider)
    {
        if (collider == null || collider.sharedMesh == null)
            return false;

        if (!ShouldDisableFastMidphase(collider.sharedMesh))
            return false;

        var current = collider.cookingOptions;
        var updated = current & ~MeshColliderCookingOptions.UseFastMidphase;
        if (current == updated)
            return false;

        collider.cookingOptions = updated;
        return true;
    }

    public static bool ShouldDisableFastMidphase(Mesh mesh)
    {
        if (mesh == null)
            return false;

        string meshName = mesh.name ?? string.Empty;
        if (meshName == "default" || meshName.StartsWith("tripo_node"))
            return true;

        long triangleCount = CountTriangles(mesh);
        return triangleCount > 500_000 || mesh.vertexCount > 500_000;
    }

    private static long CountTriangles(Mesh mesh)
    {
        long triangleCount = 0;
        int subMeshCount = Mathf.Max(1, mesh.subMeshCount);
        for (int i = 0; i < subMeshCount; i++)
            triangleCount += mesh.GetIndexCount(i) / 3;

        return triangleCount;
    }
}
