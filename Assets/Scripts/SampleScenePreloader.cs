using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SampleScenePreloader : MonoBehaviour
{
    public const string SceneName = "SampleScene";

    private static SampleScenePreloader instance;
    private static AsyncOperation preloadOperation;
    private static bool preloadStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SchedulePreload()
    {
        // Preload disabled: loading SampleScene in background freezes Editor on large maps.
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        var go = new GameObject(nameof(SampleScenePreloader));
        instance = go.AddComponent<SampleScenePreloader>();
        DontDestroyOnLoad(go);
    }

    private void Start()
    {
        BeginPreload();
    }

    public static void BeginPreload()
    {
        if (preloadStarted || !Application.CanStreamedLevelBeLoaded(SceneName))
            return;

        preloadStarted = true;
        EnsureInstance();
        instance.StartCoroutine(PreloadRoutine());
    }

    private static IEnumerator PreloadRoutine()
    {
        preloadOperation = SceneManager.LoadSceneAsync(SceneName);
        preloadOperation.allowSceneActivation = false;

        while (preloadOperation != null && preloadOperation.progress < 0.9f)
            yield return null;
    }

    public static bool TryActivatePreloaded()
    {
        if (preloadOperation == null || preloadOperation.progress < 0.9f)
            return false;

        preloadOperation.allowSceneActivation = true;
        preloadOperation = null;
        preloadStarted = false;

        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }

        return true;
    }
}
