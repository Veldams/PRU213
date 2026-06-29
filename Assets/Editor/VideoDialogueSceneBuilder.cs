using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public static class VideoDialogueSceneBuilder
{
    const string ScenePath = "Assets/Scenes/VideoDialogueScene.unity";
    const string VideoPath = "Assets/Resources/Video/Kich_ban_Hoi_thoai.mp4";

    [MenuItem("Tools/Build Video Dialogue Scene")]
    public static void Build()
    {
        var clip = AssetDatabase.LoadAssetAtPath<VideoClip>(VideoPath);
        if (clip == null)
        {
            Debug.LogError("[VideoDialogue] Missing video at: " + VideoPath);
            return;
        }

        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        var root = new GameObject("VideoDialogue");
        var player = root.AddComponent<VideoPlayer>();
        root.AddComponent<VideoDialogueController>();

        player.clip = clip;
        player.playOnAwake = false;

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.SaveAssets();

        AddSceneToBuildSettings(ScenePath);
        Debug.Log("[VideoDialogue] Scene created: " + ScenePath);
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var entry in scenes)
        {
            if (entry.path == scenePath)
                return;
        }

        var updated = new EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++)
            updated[i] = scenes[i];

        updated[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = updated;
    }
}
