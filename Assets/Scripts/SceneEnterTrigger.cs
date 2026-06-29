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

    private void Update()
    {
        if (player == null)
            player = FindFirstObjectByType<RealMovement>();

        if (player == null)
            return;

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

        if (!playerInRange || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            InteractionPromptUI.Hide();
            PlayerSceneTransition.LoadInteriorWithPlayer(player, targetSceneName);
        }
    }

    private void OnGUI()
    {
        if (!playerInRange)
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

        GUI.Label(rect, promptMessage, style);
        GUI.color = Color.white;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
