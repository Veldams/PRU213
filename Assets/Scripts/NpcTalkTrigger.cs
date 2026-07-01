using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NpcTalkTrigger : MonoBehaviour
{
    [SerializeField] private string firstPromptMessage = "E: Trò chuyện";
    [SerializeField] private string followUpPromptMessage = "E: Trò chuyện tiếp";
    [SerializeField] private float interactionRadius = 2.5f;
    [SerializeField] private string videoSceneName = VideoDialogueRequest.VideoSceneName;

    private RealMovement player;
    private bool playerInRange;
    private string lastShownPrompt;
    private float nextCheckTime;

    private void Update()
    {
        if (player == null)
            player = PlayerSceneTransition.FindPlayerMovement();

        if (ProximityUtil.ShouldRun(ref nextCheckTime) && player != null)
            UpdateProximityState();

        if (!playerInRange || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            StartTalk();
    }

    private void UpdateProximityState()
    {
        string activePrompt = GetActivePrompt();
        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool inRange = activePrompt != null && distance <= interactionRadius;

        if (inRange != playerInRange || (inRange && activePrompt != lastShownPrompt))
        {
            playerInRange = inRange;
            lastShownPrompt = inRange ? activePrompt : null;

            if (playerInRange)
                InteractionPromptUI.Show(activePrompt);
            else
                InteractionPromptUI.Hide();
        }
    }

    private string GetActivePrompt()
    {
        if (VideoDialogueRequest.CanStartFirstDialogue)
            return firstPromptMessage;

        if (VideoDialogueRequest.CanStartFollowUpDialogue)
            return followUpPromptMessage;

        return null;
    }

    private void StartTalk()
    {
        InteractionPromptUI.Hide();

        if (string.IsNullOrWhiteSpace(videoSceneName))
        {
            Debug.LogWarning("[NpcTalkTrigger] Video scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(videoSceneName))
        {
            Debug.LogError("[NpcTalkTrigger] Scene not in Build Settings: " + videoSceneName);
            return;
        }

        VideoDialogueRequest.CurrentPhase = VideoDialogueRequest.CanStartFirstDialogue
            ? VideoDialogueRequest.DialoguePhase.First
            : VideoDialogueRequest.DialoguePhase.FollowUp;

        if (VideoDialogueRequest.CurrentPhase == VideoDialogueRequest.DialoguePhase.First)
            PoliceReceptionObjectiveUI.NotifyCaptainTalkStarted();

        VideoDialogueRequest.ReturnSceneName = SceneManager.GetActiveScene().name;
        PlayerSceneTransition.PreservePlayerForSceneChange(player);
        SceneLoader.Load(videoSceneName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
