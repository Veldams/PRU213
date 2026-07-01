using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChatDialogueSceneBuilder
{
    const string ScenePath = "Assets/Scenes/ChatDialogueScene.unity";

    [MenuItem("Tools/Build Chat Dialogue Scene")]
    public static void Build()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.07f, 0.1f, 1f);
        }

        var root = new GameObject("ChatDialogue");
        root.AddComponent<ChatDialogueController>();

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.SaveAssets();

        AddSceneToBuildSettings(ScenePath);
        Debug.Log("[ChatDialogue] Scene created: " + ScenePath);
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
