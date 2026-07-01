using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// E-key entrance at the police building on SampleScene (ground-level, uses mesh bounds).
/// </summary>
public class PoliceStationEntranceTrigger : MonoBehaviour
{
    public const string BuildingObjectName = "tripo_convert_4af7160d-b155-4df9-9afc-9f6cba929dda";

    [SerializeField] private string targetSceneName = "Police_Reception_Office_Day";
    [SerializeField] private string promptMessage = "E: Vào trụ sở công an";
    [SerializeField] private float interactionRadius = 28f;

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
        if (SampleSceneGuideState.PoliceObjectiveCompleted)
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
            EnterPoliceStation();
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

    private void EnterPoliceStation()
    {
        InteractionPromptUI.Hide();

        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.LogError("[PoliceStationEntranceTrigger] Scene not in Build Settings: " + targetSceneName);
            return;
        }

        PlayerSceneTransition.LoadInteriorWithPlayer(player, targetSceneName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.55f, 1f, 0.85f);
        var center = buildingCollider != null ? buildingCollider.bounds.center : transform.position;
        Gizmos.DrawWireSphere(new Vector3(center.x, transform.position.y, center.z), interactionRadius);
    }
}
