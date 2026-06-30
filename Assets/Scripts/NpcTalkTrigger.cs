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

    private void Update()
    {
        if (player == null)
            player = FindAnyObjectByType<RealMovement>();

        if (player == null)
            return;

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

        if (!playerInRange || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            StartTalk();
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

        VideoDialogueRequest.ReturnSceneName = SceneManager.GetActiveScene().name;
        PlayerSceneTransition.PreservePlayerForSceneChange(player);
        SceneManager.LoadScene(videoSceneName);
    }

    private void OnGUI()
    {
        string activePrompt = GetActivePrompt();
        if (!playerInRange || activePrompt == null)
            return;

        const float width = 360f;
        const float height = 64f;
        var rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 120f, width, height);

        GUI.color = new Color(0f, 0f, 0f, 0.75f);
        GUI.Box(rect, GUIContent.none);

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = Color.white;

        GUI.Label(rect, activePrompt, style);
        GUI.color = Color.white;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
