using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// At the abandoned building: E to start the investigation (VTV) video.
/// </summary>
public class AbandonedBuildingInvestigateTrigger : MonoBehaviour
{
    public const string BuildingObjectName = "tripo_convert_cac2e472-cd38-4f25-9dce-93f90e2f91b1 1";

    [SerializeField] private string promptMessage = "E: Tiến hành truy quét";
    [SerializeField] private float interactionRadius = 20f;
    [SerializeField] private string videoSceneName = VideoDialogueRequest.VideoSceneName;

    private RealMovement player;
    private Collider buildingCollider;
    private bool playerInRange;
    private float nextCheckTime;

    private void Awake()
    {
        buildingCollider = GetComponent<Collider>();
        if (buildingCollider == null)
            buildingCollider = GetComponentInChildren<Collider>(true);
    }

    private void Update()
    {
        if (!VideoDialogueRequest.CanStartInvestigation)
        {
            if (playerInRange)
            {
                playerInRange = false;
                InteractionPromptUI.Hide();
            }

            return;
        }

        if (player == null)
            player = PlayerSceneTransition.FindPlayerMovement();

        if (ProximityUtil.ShouldRun(ref nextCheckTime) && player != null)
            UpdateProximityState();

        if (!playerInRange || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            StartInvestigation();
    }

    private void UpdateProximityState()
    {
        bool inRange = IsPlayerInRange();

        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            if (playerInRange)
                InteractionPromptUI.Show(promptMessage);
            else
                InteractionPromptUI.Hide();
        }
    }

    private bool IsPlayerInRange()
    {
        if (player == null)
            return false;

        if (buildingCollider != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null && buildingCollider.bounds.Intersects(cc.bounds))
                return true;

            var flatPlayer = new Vector3(player.transform.position.x, 0f, player.transform.position.z);
            var center = buildingCollider.bounds.center;
            var flatCenter = new Vector3(center.x, 0f, center.z);
            return Vector3.Distance(flatPlayer, flatCenter) <= interactionRadius;
        }

        var flatSelf = new Vector3(transform.position.x, 0f, transform.position.z);
        var flatPlayerPos = new Vector3(player.transform.position.x, 0f, player.transform.position.z);
        return Vector3.Distance(flatSelf, flatPlayerPos) <= interactionRadius;
    }

    private void StartInvestigation()
    {
        InteractionPromptUI.Hide();

        if (!Application.CanStreamedLevelBeLoaded(videoSceneName))
        {
            Debug.LogError("[AbandonedBuildingInvestigateTrigger] Scene not in Build Settings: " + videoSceneName);
            return;
        }

        VideoDialogueRequest.CurrentPhase = VideoDialogueRequest.DialoguePhase.Investigation;
        VideoDialogueRequest.ReturnSceneName = SceneManager.GetActiveScene().name;
        AbandonedBuildingObjectiveUI.NotifyInvestigationStarted();
        PlayerSceneTransition.PreservePlayerForSceneChange(player);
        SceneLoader.Load(videoSceneName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.35f, 0.1f, 0.85f);
        var center = buildingCollider != null ? buildingCollider.bounds.center : transform.position;
        Gizmos.DrawWireSphere(new Vector3(center.x, transform.position.y, center.z), interactionRadius);
    }
}
