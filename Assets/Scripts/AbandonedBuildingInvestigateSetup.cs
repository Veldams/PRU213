using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AbandonedBuildingInvestigateSetup
{
    public static IEnumerator TrySetupForScene(Scene scene)
    {
        if (scene.name != "SampleScene")
            yield break;

        if (!ShouldHaveTrigger())
            yield break;

        GameObject building = null;
        yield return SceneObjectLocator.FindInSceneAsync(
            scene,
            AbandonedBuildingInvestigateTrigger.BuildingObjectName,
            go => building = go);

        if (building == null)
        {
            Debug.LogWarning("[AbandonedBuildingInvestigateSetup] Building not found.");
            yield break;
        }

        EnsureOnBuilding(building);
    }

    public static void EnsureOnBuilding(GameObject building)
    {
        if (building == null || !ShouldHaveTrigger())
            return;

        if (building.GetComponent<AbandonedBuildingInvestigateTrigger>() != null)
            return;

        if (building.GetComponentInChildren<AbandonedBuildingInvestigateTrigger>(true) != null)
            return;

        building.AddComponent<AbandonedBuildingInvestigateTrigger>();
    }

    private static bool ShouldHaveTrigger()
    {
        return VideoDialogueRequest.CanStartInvestigation
            || PoliceReceptionGuideState.PendingAbandonedBuildingObjective;
    }
}
