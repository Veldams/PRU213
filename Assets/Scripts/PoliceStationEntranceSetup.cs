using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PoliceStationEntranceSetup
{
    public static IEnumerator TrySetupForScene(Scene scene)
    {
        if (scene.name != "SampleScene")
            yield break;

        if (SampleSceneGuideState.PoliceObjectiveCompleted)
            yield break;

        GameObject building = null;
        yield return SceneObjectLocator.FindInSceneAsync(
            scene,
            PoliceStationEntranceTrigger.BuildingObjectName,
            go => building = go);

        if (building == null)
        {
            Debug.LogWarning("[PoliceStationEntranceSetup] Police building not found.");
            yield break;
        }

        EnsureEntranceTrigger(building);
        FixWalkInTrigger(building);
    }

    private static void EnsureEntranceTrigger(GameObject building)
    {
        if (building.GetComponent<PoliceStationEntranceTrigger>() != null)
            return;

        if (building.GetComponentInChildren<PoliceStationEntranceTrigger>(true) != null)
            return;

        building.AddComponent<PoliceStationEntranceTrigger>();
    }

    private static void FixWalkInTrigger(GameObject building)
    {
        var box = building.GetComponent<BoxCollider>();
        var walkIn = building.GetComponent<SceneTransitionOnTrigger>();
        if (box == null || walkIn == null || !box.isTrigger)
            return;

        if (!TryGetWorldBounds(building, out var bounds))
            return;

        float width = Mathf.Clamp(bounds.size.x, 16f, 48f);
        float depth = Mathf.Clamp(bounds.size.z * 0.45f, 12f, 36f);
        float height = 10f;

        var worldCenter = new Vector3(
            bounds.center.x,
            bounds.min.y + height * 0.5f,
            bounds.min.z + depth * 0.55f);

        var localCenter = building.transform.InverseTransformPoint(worldCenter);
        var lossy = building.transform.lossyScale;
        box.center = localCenter;
        box.size = new Vector3(
            width / Mathf.Max(lossy.x, 0.001f),
            height / Mathf.Max(lossy.y, 0.001f),
            depth / Mathf.Max(lossy.z, 0.001f));
    }

    private static bool TryGetWorldBounds(GameObject root, out Bounds bounds)
    {
        bounds = default;
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
            return false;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.size.sqrMagnitude > 0.01f;
    }
}
