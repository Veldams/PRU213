using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-50)]
public class AudioTuningMiniGameController : MonoBehaviour
{
    [Header("Balance Targets")]
    [SerializeField] private float requiredVoiceMin = 0.78f;
    [SerializeField] private float requiredNoiseMax = 0.22f;
    [SerializeField] private float requiredHoldSeconds = 1.5f;

    [Header("Input Tuning")]
    [SerializeField] private float voiceAdjustSpeed = 0.45f;
    [SerializeField] private float noiseAdjustSpeed = 0.45f;

    [Header("Initial Mix")]
    [SerializeField] private float initialVoiceVolume = 0.35f;
    [SerializeField] private float initialNoiseVolume = 0.85f;

    private AudioSource voiceSource;
    private AudioSource noiseSource;
    private AudioClip voiceClip;
    private AudioClip noiseClip;

    private float voiceVolume;
    private float noiseVolume;
    private float tunedHoldTime;
    private bool completed;

    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private GUIStyle keyStyle;

    private void EnsureGuiStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold };
        textStyle = new GUIStyle(GUI.skin.label) { fontSize = 20 };
        keyStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
    }

    private void Start()
    {
        StartCoroutine(BootstrapRoutine());
    }

    private IEnumerator BootstrapRoutine()
    {
        yield return null;

        SetupEnvironment();
        SetupAudio();
        PlayMix();
    }

    private Camera FindSceneCamera()
    {
        var scene = gameObject.scene;
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var cam = roots[i].GetComponentInChildren<Camera>(true);
            if (cam != null)
                return cam;
        }

        return Camera.main;
    }

    private void SetupEnvironment()
    {
        Transform focus = BuildSimpleSet();

        var cam = FindSceneCamera();
        if (cam != null)
        {
            cam.gameObject.SetActive(true);
            cam.enabled = true;
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0f, 1.55f, -1.6f);
            if (focus != null)
                cam.transform.LookAt(focus.position + new Vector3(0f, 0.1f, 0f));
            else
                cam.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            cam.backgroundColor = new Color(0.05f, 0.07f, 0.1f, 1f);
        }

        EnsureSingleAudioListener(cam);
    }

    private void EnsureSingleAudioListener(Camera sceneCamera)
    {
        AudioListener selected = null;
        if (sceneCamera != null)
            selected = sceneCamera.GetComponent<AudioListener>() ?? sceneCamera.gameObject.AddComponent<AudioListener>();

        var playerRoot = PlayerSceneTransition.GetPlayerRoot();
        if (playerRoot != null)
        {
            var playerListeners = playerRoot.GetComponentsInChildren<AudioListener>(true);
            for (int i = 0; i < playerListeners.Length; i++)
                playerListeners[i].enabled = false;
        }

        if (selected != null)
            selected.enabled = true;
    }

    private static Transform BuildSimpleSet()
    {
        var existingPanel = GameObject.Find("RecorderPanel");
        if (existingPanel != null)
            return existingPanel.transform;

        Material MakeMat(Color c)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            return new Material(shader) { color = c };
        }

        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "MiniGameFloor";
        floor.transform.position = new Vector3(0f, -0.1f, 0.8f);
        floor.transform.localScale = new Vector3(8f, 0.2f, 8f);
        floor.GetComponent<Renderer>().material = MakeMat(new Color(0.16f, 0.18f, 0.22f));

        var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "ControlDesk";
        desk.transform.position = new Vector3(0f, 0.45f, 1.25f);
        desk.transform.localScale = new Vector3(2.6f, 0.9f, 1.5f);
        desk.GetComponent<Renderer>().material = MakeMat(new Color(0.05f, 0.06f, 0.08f));

        var recorderBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        recorderBase.name = "RecorderBase";
        recorderBase.transform.position = new Vector3(0f, 1.03f, 1.18f);
        recorderBase.transform.localScale = new Vector3(1.5f, 0.26f, 0.9f);
        recorderBase.GetComponent<Renderer>().material = MakeMat(new Color(0.02f, 0.03f, 0.05f));

        var recorderPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        recorderPanel.name = "RecorderPanel";
        recorderPanel.transform.position = new Vector3(0f, 1.21f, 1.27f);
        recorderPanel.transform.localScale = new Vector3(1.25f, 0.05f, 0.68f);
        recorderPanel.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
        recorderPanel.GetComponent<Renderer>().material = MakeMat(new Color(0.1f, 0.45f, 0.65f));

        return recorderPanel.transform;
    }

    private void SetupAudio()
    {
        voiceClip = Resources.Load<AudioClip>(AudioTuningRequest.VoiceResourcePath);
        noiseClip = Resources.Load<AudioClip>(AudioTuningRequest.NoiseResourcePath);

        if (voiceClip == null || noiseClip == null)
        {
            Debug.LogError("[AudioTuning] Missing clip. Voice=" + (voiceClip != null) + " Noise=" + (noiseClip != null));
            return;
        }

        var voiceGo = new GameObject("VoiceAudio");
        voiceSource = voiceGo.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
        voiceSource.spatialBlend = 0f;
        voiceSource.loop = true;
        voiceSource.clip = voiceClip;

        var noiseGo = new GameObject("NoiseAudio");
        noiseSource = noiseGo.AddComponent<AudioSource>();
        noiseSource.playOnAwake = false;
        noiseSource.spatialBlend = 0f;
        noiseSource.loop = true;
        noiseSource.clip = noiseClip;

        voiceVolume = Mathf.Clamp01(initialVoiceVolume);
        noiseVolume = Mathf.Clamp01(initialNoiseVolume);
    }

    private void PlayMix()
    {
        if (voiceSource == null || noiseSource == null)
            return;

        ApplyVolumes();
        voiceSource.Play();
        noiseSource.Play();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            ReplayMix();

        if (completed)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                ReturnToPreviousScene();
            return;
        }

        float dt = Time.deltaTime;
        if (Keyboard.current.aKey.isPressed) voiceVolume -= voiceAdjustSpeed * dt;
        if (Keyboard.current.dKey.isPressed) voiceVolume += voiceAdjustSpeed * dt;
        if (Keyboard.current.jKey.isPressed) noiseVolume -= noiseAdjustSpeed * dt;
        if (Keyboard.current.lKey.isPressed) noiseVolume += noiseAdjustSpeed * dt;

        voiceVolume = Mathf.Clamp01(voiceVolume);
        noiseVolume = Mathf.Clamp01(noiseVolume);
        ApplyVolumes();

        bool isTuned = voiceVolume >= requiredVoiceMin && noiseVolume <= requiredNoiseMax;
        tunedHoldTime = isTuned ? tunedHoldTime + dt : Mathf.Max(0f, tunedHoldTime - dt * 0.5f);

        if (tunedHoldTime >= requiredHoldSeconds)
            CompleteMiniGame();
    }

    private void ReplayMix()
    {
        if (voiceSource == null || noiseSource == null)
            return;

        voiceSource.Stop();
        noiseSource.Stop();
        voiceSource.time = 0f;
        noiseSource.time = 0f;
        voiceSource.Play();
        noiseSource.Play();
    }

    private void ApplyVolumes()
    {
        if (voiceSource != null)
            voiceSource.volume = voiceVolume;
        if (noiseSource != null)
            noiseSource.volume = noiseVolume;
    }

    private void CompleteMiniGame()
    {
        completed = true;
        AudioTuningRequest.HasCompletedMiniGame = true;

        voiceVolume = 1f;
        noiseVolume = 0f;
        ApplyVolumes();
        ReplayMix();
    }

    private void ReturnToPreviousScene()
    {
        if (completed && AudioTuningRequest.HasCompletedMiniGame)
            PoliceReceptionGuideState.PendingAbandonedBuildingObjective = true;

        var sceneName = AudioTuningRequest.ReturnSceneName;
        PlayerSceneTransition.ReturnFromMiniGame(sceneName);
    }

    private void OnGUI()
    {
        EnsureGuiStyles();

        const float panelW = 980f;
        const float panelH = 235f;
        var panel = new Rect((Screen.width - panelW) * 0.5f, Screen.height - panelH - 24f, panelW, panelH);

        GUI.color = new Color(0f, 0f, 0f, 0.76f);
        GUI.Box(panel, GUIContent.none);
        GUI.color = Color.white;

        GUI.Label(new Rect(panel.x + 20f, panel.y + 14f, 900f, 34f), "Khôi phục bản ghi âm", titleStyle);
        GUI.Label(new Rect(panel.x + 20f, panel.y + 52f, 940f, 28f), "A/D: chỉnh Voice Volume    J/L: chỉnh Noise Volume    SPACE: phát lại", textStyle);

        GUI.Label(new Rect(panel.x + 20f, panel.y + 90f, 460f, 30f), "Voice Volume: " + Mathf.RoundToInt(voiceVolume * 100f) + "%", keyStyle);
        GUI.HorizontalScrollbar(new Rect(panel.x + 22f, panel.y + 122f, 430f, 18f), 0f, voiceVolume, 0f, 1f);

        GUI.Label(new Rect(panel.x + 500f, panel.y + 90f, 460f, 30f), "Noise Volume: " + Mathf.RoundToInt(noiseVolume * 100f) + "%", keyStyle);
        GUI.HorizontalScrollbar(new Rect(panel.x + 502f, panel.y + 122f, 430f, 18f), 0f, noiseVolume, 0f, 1f);

        if (!completed)
        {
            float p = Mathf.Clamp01(tunedHoldTime / requiredHoldSeconds);
            GUI.Label(new Rect(panel.x + 20f, panel.y + 150f, 940f, 28f), "Độ rõ câu nói: " + Mathf.RoundToInt(p * 100f) + "%", textStyle);
            GUI.HorizontalScrollbar(new Rect(panel.x + 22f, panel.y + 184f, 910f, 18f), 0f, p, 0f, 1f);
        }
        else
        {
            GUI.Label(
                new Rect(panel.x + 20f, panel.y + 150f, 940f, 34f),
                "\"" + AudioTuningRequest.CriminalSentence + "\"",
                keyStyle
            );
            GUI.Label(new Rect(panel.x + 20f, panel.y + 184f, 940f, 28f), "E hoặc ESC: Quay lại phòng tiếp dân", textStyle);
        }
    }
}
