using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(CharacterController))]
public class FootstepAudio : MonoBehaviour
{
    [SerializeField] private string resourcesFolder = "Audio/Footsteps";
    [SerializeField] private float stepInterval = 0.42f;
    [SerializeField] private float minMoveSpeed = 0.2f;
    [SerializeField] private float baseVolume = 0.45f;
    [SerializeField] private float pitchMin = 0.92f;
    [SerializeField] private float pitchMax = 1.08f;

    private CharacterController controller;
    private AudioSource audioSource;
    private AudioClip[] clips;
    private float stepTimer;
    private int lastClipIndex = -1;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        clips = Resources.LoadAll<AudioClip>(resourcesFolder);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1.5f;
        audioSource.maxDistance = 18f;
        audioSource.dopplerLevel = 0f;
    }

    private void Update()
    {
        if (clips == null || clips.Length == 0 || controller == null)
            return;

        if (!controller.isGrounded)
        {
            stepTimer = 0f;
            return;
        }

        Vector3 velocity = controller.velocity;
        velocity.y = 0f;
        float speed = velocity.magnitude;
        if (speed < minMoveSpeed)
        {
            stepTimer = 0f;
            return;
        }

        float speedFactor = Mathf.Clamp01(speed / 4f);
        float interval = Mathf.Lerp(stepInterval * 1.15f, stepInterval * 0.72f, speedFactor);

        stepTimer -= Time.deltaTime;
        if (stepTimer > 0f)
            return;

        PlayStep();
        stepTimer = interval;
    }

    private void PlayStep()
    {
        int index = Random.Range(0, clips.Length);
        if (clips.Length > 1 && index == lastClipIndex)
            index = (index + 1) % clips.Length;

        lastClipIndex = index;

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        audioSource.pitch = Random.Range(pitchMin, pitchMax);
        audioSource.volume = baseVolume * sfxVolume;
        audioSource.PlayOneShot(clips[index]);
    }
}
