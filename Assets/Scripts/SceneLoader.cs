using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private static SceneLoadRunner runner;

    public static void Load(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        if (sceneName == SampleScenePreloader.SceneName && SampleScenePreloader.TryActivatePreloaded())
            return;

        EnsureRunner();
        runner.BeginLoad(sceneName);
    }

    private static void EnsureRunner()
    {
        if (runner != null)
            return;

        var go = new GameObject(nameof(SceneLoadRunner));
        Object.DontDestroyOnLoad(go);
        runner = go.AddComponent<SceneLoadRunner>();
    }
}

public class SceneLoadRunner : MonoBehaviour
{
    private bool isLoading;

    public void BeginLoad(string sceneName)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        isLoading = true;

        var op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            SceneManager.LoadScene(sceneName);
            isLoading = false;
            yield break;
        }

        op.allowSceneActivation = true;
        while (!op.isDone)
            yield return null;

        isLoading = false;
    }
}
