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
    [SerializeField] private float armDelayAfterLoad = 1.5f;

    /// <summary>
    /// Fallback spawn name used when the per-trigger spawn point can't be found in the target scene.
    /// </summary>
    public const string DefaultSpawnName = "DefaultPlayerSpawn";

    private bool _hasTriggered;
    private static GameObject _pendingPlayer;
    private static string _pendingSpawnPointName;
    private static float _ignoreUntilTime;
    private Collider _triggerCollider;
    private bool _wasPlayerInside;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }

        col.isTrigger = true;
    }

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (_hasTriggered && oneTimeOnly)
            return;

        if (!Application.isPlaying)
            return;

        var player = ResolvePlayerRoot();
        if (player == null)
        {
            _wasPlayerInside = false;
            return;
        }

        bool inside = IsPlayerInsideTrigger(player);

        if (Time.time < _ignoreUntilTime)
        {
            // While cooling down (e.g. just spawned inside trigger), treat current state as baseline.
            _wasPlayerInside = inside;
            return;
        }

        if (inside && !_wasPlayerInside)
        {
            TryStartTransition(player);
        }

        _wasPlayerInside = inside;
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
        SceneManager.LoadScene(targetSceneName);
    }

    private GameObject ResolvePlayerRoot()
    {
        if (!string.IsNullOrWhiteSpace(playerObjectName))
        {
            var named = GameObject.Find(playerObjectName);
            if (named != null)
                return named.transform.root != null ? named.transform.root.gameObject : named;
        }

        var controllers = FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
        GameObject fallbackRoot = null;
        for (int i = 0; i < controllers.Length; i++)
        {
            var cc = controllers[i];
            if (cc == null || !cc.gameObject.activeInHierarchy)
                continue;

            var root = cc.transform.root != null ? cc.transform.root.gameObject : cc.gameObject;
            if (root.CompareTag("Player"))
                return root;

            if (root.GetComponent("RealMovement") != null || root.GetComponent("ProceduralMovement") != null)
                return root;

            if (fallbackRoot == null)
                fallbackRoot = root;
        }

        return fallbackRoot;
    }

    private bool IsPlayerInsideTrigger(GameObject playerRoot)
    {
        if (_triggerCollider == null)
            _triggerCollider = GetComponent<Collider>();

        if (_triggerCollider == null)
            return false;

        var cc = playerRoot.GetComponent<CharacterController>();
        if (cc != null)
            return _triggerCollider.bounds.Intersects(cc.bounds);

        var col = playerRoot.GetComponentInChildren<Collider>();
        if (col != null)
            return _triggerCollider.bounds.Intersects(col.bounds);

        return _triggerCollider.bounds.Contains(playerRoot.transform.position);
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
        if (Time.time + 1f > _ignoreUntilTime)
            _ignoreUntilTime = Time.time + 1f;

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
        if (!string.IsNullOrWhiteSpace(spawnName))
        {
            var named = GameObject.Find(spawnName);
            if (named != null)
                return named.transform;
        }

        // Project-wide default fallback first.
        var def = GameObject.Find(DefaultSpawnName);
        if (def != null)
            return def.transform;

        var legacy = GameObject.Find("PlayerSpawn");
        return legacy != null ? legacy.transform : null;
    }

    /// <summary>
    /// If the desired spawn position overlaps non-trigger geometry, search outward
    /// on a grid for the nearest clear standing spot. This prevents the player from
    /// getting stuck inside a model after a scene transition.
    /// </summary>
    private static Vector3 FindSafeStandingPosition(Vector3 desired, GameObject playerRoot)
    {
        float radius = 0.6f;
        float height = 2.0f;

        var cc = playerRoot != null ? playerRoot.GetComponent<CharacterController>() : null;
        if (cc != null)
        {
            radius = Mathf.Max(0.3f, cc.radius * playerRoot.transform.lossyScale.x * 1.05f);
            height = Mathf.Max(1.0f, cc.height * playerRoot.transform.lossyScale.y);
        }

        Transform playerTr = playerRoot != null ? playerRoot.transform : null;

        if (IsClearStanding(desired, radius, height, playerTr))
            return desired;

        // Spiral outwards over a grid of candidate offsets.
        const float step = 0.5f;
        const float maxRange = 6.0f;
        for (float r = step; r <= maxRange; r += step)
        {
            int rings = Mathf.Max(8, Mathf.CeilToInt(r * 6f));
            for (int i = 0; i < rings; i++)
            {
                float a = (i / (float)rings) * Mathf.PI * 2f;
                var candidate = desired + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                if (IsClearStanding(candidate, radius, height, playerTr))
                {
                    return candidate;
                }
            }
        }

        // Last resort: just return the desired position; better than infinite loop.
        return desired;
    }

    private static bool IsClearStanding(Vector3 position, float radius, float height, Transform ignoreRoot)
    {
        // Lift the capsule a touch so the bottom sphere does not graze the walking surface.
        float bottomY = position.y + radius + 0.05f;
        float topY = position.y + Mathf.Max(radius + 0.06f, height - radius);
        var bottom = new Vector3(position.x, bottomY, position.z);
        var top = new Vector3(position.x, topY, position.z);
        var hits = Physics.OverlapCapsule(bottom, top, radius, ~0, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
            return true;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h == null)
                continue;
            if (ignoreRoot != null && h.transform.root == ignoreRoot)
                continue;
            // Treat anything whose top is at or below the player's feet as walkable floor, not a blocker.
            if (h.bounds.max.y <= position.y + 0.05f)
                continue;
            return false;
        }

        return true;
    }
}
