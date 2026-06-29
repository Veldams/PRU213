using UnityEngine;
using UnityEngine.SceneManagement;

public static class SampleSceneEntranceSetup
{
    private const string BuildingNameToken = "4af7160d";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SetupEntranceTrigger()
    {
        if (SceneManager.GetActiveScene().name != "SampleScene")
            return;

        var trigger = Object.FindFirstObjectByType<SceneEnterTrigger>();
        if (trigger == null)
        {
            var triggerObject = new GameObject("BuildingEntranceTrigger");
            trigger = triggerObject.AddComponent<SceneEnterTrigger>();
        }

        AlignToBuildingEntrance(trigger.transform);
    }

    private static void AlignToBuildingEntrance(Transform trigger)
    {
        Transform building = FindPoliceBuilding();
        if (building == null)
            return;

        trigger.position = new Vector3(
            building.position.x,
            0.5f,
            building.position.z + 2.5f);
    }

    private static Transform FindPoliceBuilding()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name.Contains(BuildingNameToken))
                return root.transform;
        }

        return null;
    }
}
