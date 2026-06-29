public static class VideoDialogueRequest
{
    public const string VideoSceneName = "VideoDialogueScene";

    public static string ReturnSceneName = "Police_Reception_Office_Day";

    public enum DialoguePhase
    {
        First,
        FollowUp
    }

    public static DialoguePhase CurrentPhase { get; set; } = DialoguePhase.First;

    public static bool HasWatchedFirstDialogue { get; private set; }
    public static bool HasWatchedFollowUpDialogue { get; private set; }

    public static bool CanStartFirstDialogue => !HasWatchedFirstDialogue;

    public static bool CanStartFollowUpDialogue => HasWatchedFirstDialogue && !HasWatchedFollowUpDialogue;

    public static void MarkPhaseComplete(DialoguePhase phase)
    {
        if (phase == DialoguePhase.First)
            HasWatchedFirstDialogue = true;
        else
            HasWatchedFollowUpDialogue = true;
    }

    public static string GetVideoFileName(DialoguePhase phase)
    {
        return phase == DialoguePhase.First
            ? "Kich_ban_Hoi_thoai.mp4"
            : "Untitled.mp4";
    }

    public static string GetAudioResourcesPath(DialoguePhase phase)
    {
        return phase == DialoguePhase.First
            ? "Audio/Dialogue/Kich_ban_Hoi_thoai"
            : "Audio/Dialogue/Untitled";
    }
}
