using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneTransition : MonoBehaviour
{
    public const float InteriorCharacterScale = 2.9625432f;

    private static PlayerSceneTransition instance;
    private static GameObject playerRoot;

    public static GameObject GetPlayerRoot() => playerRoot;

    public static Transform FindPlayerTransform()
    {
        if (playerRoot != null)
        {
            var movement = playerRoot.GetComponentInChildren<RealMovement>(true);
            return movement != null ? movement.transform : playerRoot.transform;
        }

        var named = SceneObjectLocator.FindInScene(SceneManager.GetActiveScene(), "Unarmed Idle 01", 3000);
        if (named == null)
            return null;

        return named.transform.root != null ? named.transform.root : named.transform;
    }

    public static RealMovement FindPlayerMovement()
    {
        var playerTransform = FindPlayerTransform();
        if (playerTransform == null)
            return null;

        return playerTransform.GetComponent<RealMovement>()
            ?? playerTransform.GetComponentInChildren<RealMovement>(true);
    }
    private static Vector3 savedScale = Vector3.one;
    private static Vector3 savedPosition;
    private static Quaternion savedRotation;
    private static bool restoreSavedTransform;
    private static Camera cachedActiveCamera;

    public static Camera GetActiveCamera()
    {
        if (cachedActiveCamera != null && cachedActiveCamera.isActiveAndEnabled)
            return cachedActiveCamera;

        if (playerRoot != null && playerRoot.activeInHierarchy)
        {
            cachedActiveCamera = playerRoot.GetComponentInChildren<Camera>(true);
            if (cachedActiveCamera != null && cachedActiveCamera.enabled)
                return cachedActiveCamera;
        }

        return null;
    }

    public static void CachePlayer(RealMovement movement)
    {
        if (movement == null)
            return;

        playerRoot = movement.transform.root.gameObject;
    }

    private static void InvalidateCameraCache()
    {
        cachedActiveCamera = null;
    }

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

    public static void LoadMiniGameScene(RealMovement player)
    {
        if (player == null)
            return;

        EnsureInstance();
        PreservePlayer(player, true);
        SceneLoader.Load(AudioTuningRequest.MiniGameSceneName);
    }

    public static void ReturnFromMiniGame(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            sceneName = "Police_Reception_Office_Day";

        SceneLoader.Load(sceneName);
    }

    public static void LoadInteriorWithPlayer(RealMovement player, string sceneName)
    {
        if (player == null)
            return;

        EnsureInstance();
        if (sceneName == "Police_Reception_Office_Day")
        {
            PoliceReceptionGuideState.ShowIntroOnLoad = true;
            ObjectiveGuideUI.NotifyPoliceEntered();
        }
        PreservePlayer(player, false);
        SceneLoader.Load(sceneName);
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
        InvalidateCameraCache();

        if (playerRoot == null)
            return;

        StartCoroutine(DeferredSceneSetup(scene));
    }

    private IEnumerator DeferredSceneSetup(Scene scene)
    {
        yield return null;

        if (scene.name == "SampleScene")
            yield return null;

        if (playerRoot == null)
            yield break;

        switch (scene.name)
        {
            case "VideoDialogueScene":
            case ImageDialogueRequest.SceneName:
            case AudioTuningRequest.MiniGameSceneName:
                HidePlayerForOverlayScene();
                break;
            case "Police_Reception_Office_Day":
                RestorePlayerAfterOverlay();
                if (restoreSavedTransform)
                    SetupReceptionOfficeAt(savedPosition, savedRotation);
                else
                    SetupReceptionOffice();
                restoreSavedTransform = false;
                break;
            case "SampleScene":
                RestorePlayerAfterOverlay();
                if (restoreSavedTransform)
                {
                    playerRoot.transform.position = savedPosition;
                    playerRoot.transform.rotation = savedRotation;
                    playerRoot.transform.localScale = savedScale;
                    restoreSavedTransform = false;
                    EnablePlayerCamera();
                }
                else
                {
                    SetupSampleScene();
                }
                break;
        }
    }

    private static void RestorePlayerAfterOverlay()
    {
        if (playerRoot == null)
            return;

        playerRoot.SetActive(true);

        var sources = playerRoot.GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null)
                continue;

            sources[i].volume = 1f;
            sources[i].UnPause();
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
        playerRoot.SetActive(true);
        playerRoot.transform.localScale = savedScale;

        var spawn = SceneObjectLocator.FindInScene(SceneManager.GetActiveScene(), "SampleDoorSpawn");
        if (spawn != null)
            playerRoot.transform.position = spawn.transform.position;

        EnablePlayerCamera();
    }

    private static Transform FindDoorTransform()
    {
        var door = SceneObjectLocator.FindInScene(SceneManager.GetActiveScene(), "Door");
        return door != null ? door.transform : null;
    }

    private void DisableFixedSceneCamera()
    {
        var rig = GameObject.Find("MainCameraRig");
        if (rig != null)
            rig.SetActive(false);
    }

    private void EnablePlayerCamera()
    {
        if (playerRoot == null)
            return;

        Camera primary = null;
        foreach (var cam in playerRoot.GetComponentsInChildren<Camera>(true))
        {
            cam.gameObject.SetActive(true);
            cam.enabled = true;
            if (primary == null)
                primary = cam;
        }

        if (primary != null)
        {
            primary.tag = "MainCamera";
            primary.depth = 0;
            if (primary.clearFlags == CameraClearFlags.Nothing || primary.cullingMask == 0)
            {
                primary.clearFlags = CameraClearFlags.Skybox;
                primary.cullingMask = ~0;
            }
            cachedActiveCamera = primary;
        }

        EnablePlayerAudioListeners();
    }

    private static void HidePlayerForOverlayScene()
    {
        if (playerRoot == null)
            return;

        DisablePlayerAudioListeners();
        playerRoot.SetActive(false);
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
