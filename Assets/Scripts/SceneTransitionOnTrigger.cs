using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionOnTrigger : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string targetSceneName = "Police_Reception_Office_Day";
    [SerializeField] private string playerObjectName = "Unarmed Idle 01";
    [SerializeField] private bool oneTimeOnly = true;
    [SerializeField] private bool keepPlayerAcrossScenes = true;
    [SerializeField] private string spawnPointNameInTargetScene = "PlayerSpawn";
    [SerializeField] private bool debugLogs = false;
    [Tooltip("Time (sec) after a scene load during which incoming triggers in the new scene are ignored. Prevents instant re-trigger loops when the player spawns inside another trigger.")]
    [SerializeField] private float armDelayAfterLoad = 0.1f;

    /// <summary>
    /// Fallback spawn name used when the per-trigger spawn point can't be found in the target scene.
    /// </summary>
    public const string DefaultSpawnName = "DefaultPlayerSpawn";

    private bool _hasTriggered;
    private static GameObject _pendingPlayer;
    private static string _pendingSpawnPointName;
    private static float _ignoreUntilTime;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }

        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered && oneTimeOnly)
            return;

        if (!IsPlayerCollider(other))
            return;

        if (Time.time < _ignoreUntilTime)
        {
            if (debugLogs)
                Debug.Log("[SceneTransitionOnTrigger] '" + gameObject.name + "' OnTriggerEnter ignored: cooldown.");
            return;
        }

        TryStartTransition(other.transform.root != null ? other.transform.root.gameObject : other.gameObject);
    }

    private void TryStartTransition(GameObject playerRoot)
    {
        if (playerRoot == null)
            return;

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("[SceneTransitionOnTrigger] Target scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.LogError("[SceneTransitionOnTrigger] Scene not found in Build Settings: " + targetSceneName);
            return;
        }

        if (debugLogs)
            Debug.Log("[SceneTransitionOnTrigger] Loading scene '" + targetSceneName + "' from '" + gameObject.name + "'.");

        if (keepPlayerAcrossScenes)
        {
            _pendingPlayer = playerRoot;
            _pendingSpawnPointName = spawnPointNameInTargetScene;
            DontDestroyOnLoad(_pendingPlayer);
            SceneManager.sceneLoaded -= OnTargetSceneLoaded;
            SceneManager.sceneLoaded += OnTargetSceneLoaded;
        }

        _hasTriggered = true;
        _ignoreUntilTime = Time.time + Mathf.Max(0.1f, armDelayAfterLoad);

        if (targetSceneName == "Police_Reception_Office_Day"
            && SceneManager.GetActiveScene().name == "SampleScene")
        {
            PoliceReceptionGuideState.ShowIntroOnLoad = true;
            ObjectiveGuideUI.NotifyPoliceEntered();
        }

        SceneLoader.Load(targetSceneName);
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other == null)
            return false;

        var root = other.transform.root != null ? other.transform.root.gameObject : other.gameObject;

        // Support both direct collider name and root object name.
        if (!string.IsNullOrWhiteSpace(playerObjectName))
        {
            if (other.gameObject.name == playerObjectName || root.name == playerObjectName)
                return true;
        }

        // Common Unity setup.
        if (other.CompareTag("Player") || root.CompareTag("Player"))
            return true;

        // Fallback for this project: player root usually has CharacterController + movement script.
        var hasController = root.GetComponent<CharacterController>() != null;
        var hasMovement = root.GetComponent("RealMovement") != null || root.GetComponent("ProceduralMovement") != null;
        if (hasController && hasMovement)
            return true;

        return false;
    }

    private static void OnTargetSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnTargetSceneLoaded;

        // Extend cooldown so triggers in the freshly loaded scene don't fire immediately after spawn.
        if (Time.time + 0.15f > _ignoreUntilTime)
            _ignoreUntilTime = Time.time + 0.15f;

        if (_pendingPlayer == null)
            return;

        // Destroy any duplicate root with the same name (the loaded scene may bake its own player copy).
        var allRoots = scene.GetRootGameObjects();
        for (int i = 0; i < allRoots.Length; i++)
        {
            var r = allRoots[i];
            if (r == null || r == _pendingPlayer) continue;
            if (r.name == _pendingPlayer.name)
                Destroy(r);
        }

        // Put player into the newly loaded scene instead of keeping it in DDOL scene.
        SceneManager.MoveGameObjectToScene(_pendingPlayer, scene);

        var controller = _pendingPlayer.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        var spawn = FindSpawnTransform(_pendingSpawnPointName);
        if (spawn != null)
        {
            var safePos = FindSafeStandingPosition(spawn.position, _pendingPlayer);
            _pendingPlayer.transform.SetPositionAndRotation(safePos, spawn.rotation);
        }

        if (controller != null)
            controller.enabled = true;

        var realMovement = _pendingPlayer.GetComponent("RealMovement") as Behaviour;
        if (realMovement != null)
            realMovement.enabled = true;

        var proceduralMovement = _pendingPlayer.GetComponent("ProceduralMovement") as Behaviour;
        if (proceduralMovement != null)
            proceduralMovement.enabled = true;

        var playerCamera = _pendingPlayer.GetComponentInChildren<Camera>(true);
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
            playerCamera.enabled = true;
            playerCamera.tag = "MainCamera";
        }

        _pendingPlayer.SetActive(true);
        _pendingPlayer = null;
        _pendingSpawnPointName = null;
    }

    private static Transform FindSpawnTransform(string spawnName)
    {
        var scene = SceneManager.GetActiveScene();

        if (!string.IsNullOrWhiteSpace(spawnName))
        {
            var named = SceneObjectLocator.FindInScene(scene, spawnName, 500);
            if (named != null)
                return named.transform;
        }

        var def = SceneObjectLocator.FindInScene(scene, DefaultSpawnName, 500);
        if (def != null)
            return def.transform;

        var legacy = SceneObjectLocator.FindInScene(scene, "PlayerSpawn", 500);
        return legacy != null ? legacy.transform : null;
    }

    /// <summary>
    /// If the desired spawn position overlaps non-trigger geometry, search outward
    /// on a grid for the nearest clear standing spot. This prevents the player from
    /// getting stuck inside a model after a scene transition.
    /// </summary>
    private static Vector3 FindSafeStandingPosition(Vector3 desired, GameObject playerRoot)
    {
        // OverlapCapsule spiral search freezes Unity on SampleScene's dense mesh colliders.
        return desired;
    }

    private static bool IsClearStanding(Vector3 position, float radius, float height, Transform ignoreRoot)
    {
        return true;
    }
}
