#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayModeSceneSaveGuard
{
    static PlayModeSceneSaveGuard()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        if (EditorApplication.isPlaying)
            return;

        try
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorApplication.isPlaying = false;
        }
        catch (System.InvalidOperationException)
        {
            // Unity 6 may already be transitioning into Play mode here.
        }
    }
}
#endif
