using UnityEngine;
using UnityEngine.InputSystem;

public class NpcTalkTrigger : MonoBehaviour
{
    [SerializeField] private string firstPromptMessage = "E: Trò chuyện";
    [SerializeField] private string followUpPromptMessage = "E: Trò chuyện tiếp";
    [SerializeField] private float interactionRadius = 2.5f;

    private RealMovement player;
    private bool playerInRange;
    private string lastShownPrompt;
    private float nextCheckTime;

    private void Update()
    {
        if (ChatDialogueController.IsActive)
            return;

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

        if (player == null)
            player = PlayerSceneTransition.FindPlayerMovement();

        if (VideoDialogueRequest.CanStartFirstDialogue)
        {
            PoliceReceptionObjectiveUI.NotifyCaptainTalkStarted();
            ChatDialogueController.StartInScene(
                VideoDialogueRequest.DialoguePhase.First,
                player,
                chainPoliceFollowUp: true);
        }
        else if (VideoDialogueRequest.CanStartFollowUpDialogue)
        {
            ChatDialogueController.StartInScene(VideoDialogueRequest.DialoguePhase.FollowUp, player);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
