using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneTransition : MonoBehaviour
{
    private static PlayerSceneTransition instance;
    private static GameObject playerRoot;
    private static Vector3 savedScale = Vector3.one;

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

        playerRoot = player.transform.root.gameObject;
        savedScale = playerRoot.transform.localScale;
        DontDestroyOnLoad(playerRoot);

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (playerRoot == null)
            return;

        switch (scene.name)
        {
            case "Police_Reception_Office_Day":
                SetupReceptionOffice();
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

        playerRoot.transform.position = spawnPos;
        playerRoot.transform.rotation = door != null
            ? Quaternion.LookRotation(door.forward, Vector3.up)
            : Quaternion.identity;
        playerRoot.transform.localScale = Vector3.one;

        EnsureCharacterController();
        EnablePlayerCamera();
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

        foreach (var listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            listener.enabled = listener.transform.IsChildOf(playerRoot.transform);
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
