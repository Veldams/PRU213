using UnityEngine;
using UnityEngine.InputSystem;

public class AudioTuningTrigger : MonoBehaviour
{
    [SerializeField] private string promptMessage = "E: Bắt đầu điều chỉnh";
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private string miniGameSceneName = AudioTuningRequest.MiniGameSceneName;

    private RealMovement player;
    private bool isInRange;
    private float nextCheckTime;

    private void Update()
    {
        if (player == null)
            player = PlayerSceneTransition.FindPlayerMovement();

        if (ProximityUtil.ShouldRun(ref nextCheckTime) && player != null)
            UpdateProximityState();

        if (!isInRange || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            StartMiniGame();
    }

    private void UpdateProximityState()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool nextInRange = distance <= interactionRadius;

        if (nextInRange != isInRange)
        {
            isInRange = nextInRange;
            if (isInRange)
                InteractionPromptUI.Show(promptMessage);
            else
                InteractionPromptUI.Hide();
        }
    }

    private void StartMiniGame()
    {
        InteractionPromptUI.Hide();

        if (string.IsNullOrWhiteSpace(miniGameSceneName))
        {
            Debug.LogWarning("[AudioTuningTrigger] Minigame scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(miniGameSceneName))
        {
            Debug.LogError("[AudioTuningTrigger] Scene not in Build Settings: " + miniGameSceneName);
            return;
        }

        AudioTuningRequest.ReturnSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        AudioTuningRequest.HasCompletedMiniGame = false;
        ControlPanelObjectiveUI.NotifyMiniGameStarted();
        PlayerSceneTransition.LoadMiniGameScene(player);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
