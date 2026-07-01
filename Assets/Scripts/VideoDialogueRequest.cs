public static class VideoDialogueRequest
{
    public const string VideoSceneName = "VideoDialogueScene";

    public static string ReturnSceneName = "Police_Reception_Office_Day";

    public enum DialoguePhase
    {
        First,
        FollowUp,
        Investigation
    }

    public static DialoguePhase CurrentPhase { get; set; } = DialoguePhase.First;

    public static bool HasWatchedFirstDialogue { get; private set; }
    public static bool HasWatchedFollowUpDialogue { get; private set; }
    public static bool HasWatchedInvestigationVideo { get; private set; }

    public static bool CanStartFirstDialogue => !HasWatchedFirstDialogue;

    public static bool CanStartFollowUpDialogue => HasWatchedFirstDialogue && !HasWatchedFollowUpDialogue;

    public static bool CanStartInvestigation =>
        AudioTuningRequest.HasCompletedMiniGame && !HasWatchedInvestigationVideo;

    public static bool UsesEmbeddedAudio(DialoguePhase phase)
    {
        return phase == DialoguePhase.Investigation;
    }

    public static void MarkPhaseComplete(DialoguePhase phase)
    {
        switch (phase)
        {
            case DialoguePhase.First:
                HasWatchedFirstDialogue = true;
                break;
            case DialoguePhase.FollowUp:
                HasWatchedFollowUpDialogue = true;
                break;
            case DialoguePhase.Investigation:
                HasWatchedInvestigationVideo = true;
                break;
        }
    }

    public static string GetVideoFileName(DialoguePhase phase)
    {
        switch (phase)
        {
            case DialoguePhase.First:
                return "Kich_ban_Hoi_thoai.mp4";
            case DialoguePhase.FollowUp:
                return "Untitled.mp4";
            case DialoguePhase.Investigation:
                return "Van_dang_bi_hien_thi_logo_VTV.mp4";
            default:
                return string.Empty;
        }
    }

    public static string GetVideoResourcesPath(DialoguePhase phase)
    {
        switch (phase)
        {
            case DialoguePhase.First:
                return "Video/Kich_ban_Hoi_thoai";
            case DialoguePhase.FollowUp:
                return "Video/Untitled";
            case DialoguePhase.Investigation:
                return "Video/Van_dang_bi_hien_thi_logo_VTV";
            default:
                return string.Empty;
        }
    }

    public static string GetAudioResourcesPath(DialoguePhase phase)
    {
        if (UsesEmbeddedAudio(phase))
            return string.Empty;

        return phase == DialoguePhase.First
            ? "Audio/Dialogue/Kich_ban_Hoi_thoai"
            : "Audio/Dialogue/Untitled";
    }
}
