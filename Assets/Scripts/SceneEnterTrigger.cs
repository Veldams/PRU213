using UnityEngine;
using UnityEngine.InputSystem;

public class SceneEnterTrigger : MonoBehaviour
{
    private const string DefaultTargetScene = "Police_Reception_Office_Day";

    [SerializeField] private string targetSceneName = DefaultTargetScene;
    [SerializeField] private string promptMessage = "E: Đi vào";
    [SerializeField] private float interactionRadius = 18f;

    private RealMovement player;
    private bool playerInRange;
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
        {
            InteractionPromptUI.Hide();
            PlayerSceneTransition.LoadInteriorWithPlayer(player, targetSceneName);
        }
    }

    private void UpdateProximityState()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool inRange = distance <= interactionRadius;

        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            if (playerInRange)
                InteractionPromptUI.Show(promptMessage);
            else
                InteractionPromptUI.Hide();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
