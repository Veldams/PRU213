public static class ChatDialogueData
{
    public static ChatDialogueLine[] GetLines(VideoDialogueRequest.DialoguePhase phase)
    {
        switch (phase)
        {
            case VideoDialogueRequest.DialoguePhase.First:
                return FirstLines;
            case VideoDialogueRequest.DialoguePhase.FollowUp:
                return FollowUpLines;
            case VideoDialogueRequest.DialoguePhase.Investigation:
                return InvestigationLines;
            default:
                return FirstLines;
        }
    }

    public static ChatDialogueLine[] GetPoliceStationOpeningLines()
    {
        var combined = new ChatDialogueLine[FirstLines.Length + FollowUpLines.Length];
        FirstLines.CopyTo(combined, 0);
        FollowUpLines.CopyTo(combined, FirstLines.Length);
        return combined;
    }

    public static readonly ChatDialogueLine[] FirstLines =
    {
        new ChatDialogueLine
        {
            speaker = "Chiến sĩ công an",
            message = "Chào đồng chí đội trưởng. Báo cáo đội trưởng, vật chứng từ hiện trường xưởng gỗ đây ạ. Camera này bị giấu trên xà nhà, nhưng chúng đã phát hiện và đập nát nó.",
            audioResourcePath = "Audio/Dialogue/Kich_ban_Hoi_thoai_da_bo_sung"
        },
        new ChatDialogueLine
        {
            speaker = "Đội trưởng",
            message = "Đập nát rồi sao? Có khôi phục được gì không?"
        }
    };

    public static readonly ChatDialogueLine[] FollowUpLines =
    {
        new ChatDialogueLine
        {
            speaker = "Đội trưởng",
            message = "Khôi phục được phần âm thanh, nhưng hình ảnh thì mất. Cậu mang dữ liệu xuống phòng kỹ thuật, chỉnh tần số và lọc nhiễu kỹ — có thể vẫn còn manh mối quan trọng."
        },
        new ChatDialogueLine
        {
            speaker = "Trung úy",
            message = "Vâng ạ, em xuống phòng kỹ thuật ngay."
        }
    };

    public static readonly ChatDialogueLine[] InvestigationLines =
    {
        new ChatDialogueLine
        {
            speaker = "Phóng viên VTV",
            message = "Lực lượng CSHS đang tiến hành khám nghiệm hiện trường tại tòa nhà bỏ hoang. Vụ việc đang được điều tra sát sao."
        },
        new ChatDialogueLine
        {
            speaker = "Đội trưởng",
            message = "Bắt đầu thu thập vân tay và dấu vết. Ghi nhận mọi chi tiết, báo cáo ngay cho tôi."
        }
    };
}
