using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Single deferred sceneLoaded hook — UI bootstrap runs after the scene settles (prevents freezes).
/// </summary>
public static class GameSceneBootstrap
{
    private static bool registered;
    private static BootstrapHost host;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (registered)
            return;

        registered = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureHost();
        host.StartBootstrap(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!Application.isPlaying)
            return;

        EnsureHost();
        host.StartBootstrap(scene);
    }

    private static void EnsureHost()
    {
        if (host != null)
            return;

        var go = new GameObject(nameof(BootstrapHost));
        Object.DontDestroyOnLoad(go);
        host = go.AddComponent<BootstrapHost>();
    }

    private sealed class BootstrapHost : MonoBehaviour
    {
        public void StartBootstrap(Scene scene)
        {
            StopAllCoroutines();
            StartCoroutine(BootstrapRoutine(scene));
        }

        private static IEnumerator BootstrapRoutine(Scene scene)
        {
            SceneObjectLocator.ClearCache(scene);

            yield return null;

            if (scene.name == SampleScenePreloader.SceneName || scene.name == "SampleScene")
            {
                yield return null;
                yield return null;
                yield return null;
            }

            PoliceReceptionObjectiveUI.TryCreateForScene(scene);
            ControlPanelObjectiveUI.TryCreateForScene(scene);
            MovementTutorialUI.TryCreateForScene(scene);
            AbandonedBuildingObjectiveUI.TryCreateForScene(scene);
            ObjectiveGuideUI.TryCreateForScene(scene);
            yield return PoliceStationEntranceSetup.TrySetupForScene(scene);
            yield return AbandonedBuildingInvestigateSetup.TrySetupForScene(scene);
        }
    }
}

public static class ProximityUtil
{
    public static bool ShouldRun(ref float nextTime, float interval = 0.1f)
    {
        if (Time.time < nextTime)
            return false;

        nextTime = Time.time + interval;
        return true;
    }
}
