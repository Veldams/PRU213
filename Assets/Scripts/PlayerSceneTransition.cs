using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneTransition : MonoBehaviour
{
    public const float InteriorCharacterScale = 2.9625432f;

    private static PlayerSceneTransition instance;
    private static GameObject playerRoot;
    private static Vector3 savedScale = Vector3.one;
    private static Vector3 savedPosition;
    private static Quaternion savedRotation;
    private static bool restoreSavedTransform;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        var go = new GameObject(nameof(PlayerSceneTransition));
        instance = go.AddComponent<PlayerSceneTransition>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void LoadInteriorWithPlayer(RealMovement player, string sceneName)
    {
        if (player == null)
            return;

        EnsureInstance();
        PreservePlayer(player, false);
        SceneManager.LoadScene(sceneName);
    }

    public static void PreservePlayerForSceneChange(RealMovement player)
    {
        if (player == null)
            return;

        EnsureInstance();
        PreservePlayer(player, true);
    }

    static void PreservePlayer(RealMovement player, bool rememberTransform)
    {
        playerRoot = player.transform.root.gameObject;
        savedScale = playerRoot.transform.localScale;

        if (rememberTransform)
        {
            savedPosition = playerRoot.transform.position;
            savedRotation = playerRoot.transform.rotation;
            restoreSavedTransform = true;
        }

        DontDestroyOnLoad(playerRoot);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (playerRoot == null)
            return;

        switch (scene.name)
        {
            case "VideoDialogueScene":
                playerRoot.SetActive(false);
                DisablePlayerAudioListeners();
                break;
            case "Police_Reception_Office_Day":
                if (restoreSavedTransform)
                    SetupReceptionOfficeAt(savedPosition, savedRotation);
                else
                    SetupReceptionOffice();
                restoreSavedTransform = false;
                break;
            case "SampleScene":
                SetupSampleScene();
                break;
        }
    }

    private void SetupReceptionOffice()
    {
        DisableFixedSceneCamera();

        Transform door = FindDoorTransform();
        Vector3 spawnPos = door != null
            ? door.position + door.forward * 2f
            : new Vector3(5.7f, 0f, -3.2f);
        spawnPos.y = 0f;

        SetupReceptionOfficeAt(spawnPos, door != null
            ? Quaternion.LookRotation(door.forward, Vector3.up)
            : Quaternion.identity);
    }

    private void SetupReceptionOfficeAt(Vector3 position, Quaternion rotation)
    {
        DisableFixedSceneCamera();
        DestroyDuplicatePlayers();

        playerRoot.SetActive(true);
        playerRoot.transform.position = position;
        playerRoot.transform.rotation = rotation;

        if (savedScale.x < InteriorCharacterScale * 0.75f)
            savedScale = Vector3.one * InteriorCharacterScale;

        playerRoot.transform.localScale = savedScale;

        EnsureCharacterController();
        EnablePlayerCamera();
    }

    private void DestroyDuplicatePlayers()
    {
        if (playerRoot == null)
            return;

        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var root = roots[i];
            if (root == null || root == playerRoot)
                continue;

            if (root.name == playerRoot.name)
                Object.Destroy(root);
        }

        if (playerRoot.scene != scene)
            SceneManager.MoveGameObjectToScene(playerRoot, scene);
    }

    private void SetupSampleScene()
    {
        playerRoot.transform.localScale = savedScale;

        var entrance = FindFirstObjectByType<SceneEnterTrigger>();
        if (entrance != null)
            playerRoot.transform.position = entrance.transform.position + Vector3.back * 4f;

        EnablePlayerCamera();
    }

    private static Transform FindDoorTransform()
    {
        var door = GameObject.Find("Door");
        return door != null ? door.transform : null;
    }

    private void DisableFixedSceneCamera()
    {
        var rig = GameObject.Find("MainCameraRig");
        if (rig != null)
            rig.SetActive(false);

        foreach (var cam in FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (cam.transform.IsChildOf(playerRoot.transform))
                continue;

            if (cam.CompareTag("MainCamera"))
                cam.gameObject.SetActive(false);
        }
    }

    private void EnablePlayerCamera()
    {
        foreach (var cam in playerRoot.GetComponentsInChildren<Camera>(true))
            cam.gameObject.SetActive(true);

        EnablePlayerAudioListeners();
    }

    private static void DisablePlayerAudioListeners()
    {
        if (playerRoot == null)
            return;

        foreach (var listener in playerRoot.GetComponentsInChildren<AudioListener>(true))
            listener.enabled = false;
    }

    private static void EnablePlayerAudioListeners()
    {
        if (playerRoot == null)
            return;

        var listeners = playerRoot.GetComponentsInChildren<AudioListener>(true);
        for (int i = 0; i < listeners.Length; i++)
            listeners[i].enabled = i == 0;
    }

    private void EnsureCharacterController()
    {
        var movement = playerRoot.GetComponentInChildren<RealMovement>();
        if (movement == null)
            return;

        if (movement.GetComponent<CharacterController>() != null)
            return;

        var controller = movement.gameObject.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.35f;
        controller.center = new Vector3(0f, 1f, 0f);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
