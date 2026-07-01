using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene-scoped object lookup. Avoids GameObject.Find which freezes Unity on large scenes.
/// </summary>
public static class SceneObjectLocator
{
    private static readonly Dictionary<int, Dictionary<string, GameObject>> Cache = new();

    public static void ClearCache(Scene scene)
    {
        if (scene.IsValid())
            Cache.Remove(scene.handle);
    }

    public static GameObject FindInScene(Scene scene, string objectName, int maxNodes = 8000)
    {
        if (!scene.IsValid() || string.IsNullOrEmpty(objectName))
            return null;

        if (TryGetCached(scene, objectName, out GameObject cached))
            return cached;

        var roots = scene.GetRootGameObjects();
        int visited = 0;
        for (int i = 0; i < roots.Length && visited < maxNodes; i++)
        {
            if (roots[i] == null)
                continue;

            if (roots[i].name == objectName)
                return CacheResult(scene, objectName, roots[i]);

            var found = SearchChildren(roots[i].transform, objectName, maxNodes, ref visited);
            if (found != null)
                return CacheResult(scene, objectName, found);
        }

        return null;
    }

    public static IEnumerator FindInSceneAsync(Scene scene, string objectName, System.Action<GameObject> onComplete, int nodesPerFrame = 400)
    {
        if (!scene.IsValid() || string.IsNullOrEmpty(objectName))
        {
            onComplete?.Invoke(null);
            yield break;
        }

        if (TryGetCached(scene, objectName, out GameObject cached))
        {
            onComplete?.Invoke(cached);
            yield break;
        }

        yield return null;

        var queue = new Queue<Transform>();
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null)
                queue.Enqueue(roots[i].transform);
        }

        int nodesThisFrame = 0;
        while (queue.Count > 0)
        {
            while (queue.Count > 0 && nodesThisFrame < nodesPerFrame)
            {
                Transform current = queue.Dequeue();
                nodesThisFrame++;

                if (current.name == objectName)
                {
                    onComplete?.Invoke(CacheResult(scene, objectName, current.gameObject));
                    yield break;
                }

                for (int c = 0; c < current.childCount; c++)
                    queue.Enqueue(current.GetChild(c));
            }

            nodesThisFrame = 0;
            yield return null;
        }

        onComplete?.Invoke(null);
    }

    private static GameObject SearchChildren(Transform parent, string objectName, int maxNodes, ref int visited)
    {
        for (int i = 0; i < parent.childCount && visited < maxNodes; i++)
        {
            Transform child = parent.GetChild(i);
            visited++;

            if (child.name == objectName)
                return child.gameObject;

            var found = SearchChildren(child, objectName, maxNodes, ref visited);
            if (found != null)
                return found;
        }

        return null;
    }

    private static bool TryGetCached(Scene scene, string objectName, out GameObject result)
    {
        result = null;
        if (!Cache.TryGetValue(scene.handle, out Dictionary<string, GameObject> sceneCache))
            return false;

        if (!sceneCache.TryGetValue(objectName, out result))
            return false;

        if (result == null)
            sceneCache.Remove(objectName);

        return result != null;
    }

    private static GameObject CacheResult(Scene scene, string objectName, GameObject go)
    {
        if (!Cache.TryGetValue(scene.handle, out Dictionary<string, GameObject> sceneCache))
        {
            sceneCache = new Dictionary<string, GameObject>();
            Cache[scene.handle] = sceneCache;
        }

        sceneCache[objectName] = go;
        return go;
    }
}
