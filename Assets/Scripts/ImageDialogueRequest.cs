public static class ImageDialogueRequest
{
    public const string SceneName = "SuspectDialogueScene";

    public static string ReturnSceneName = "SampleScene";

    public static bool HasWatchedSuspectDialogue { get; private set; }

    public const string BackgroundResourcePath = "Images/Suspect_interrogation";
    public const string AudioResourcePath = "Audio/Dialogue/Duong_v2";

    public static void MarkComplete()
    {
        HasWatchedSuspectDialogue = true;
    }
}
