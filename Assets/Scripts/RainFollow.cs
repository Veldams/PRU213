using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps a rain emitter hovering above a target (player or main camera) so the
/// rain volume always surrounds the area the viewer can see. Only the horizontal
/// position is tracked; the emitter stays world-up and at a fixed height so the
/// rain always falls straight down regardless of how the camera is rotated.
/// </summary>
public class RainFollow : MonoBehaviour
{
    [Tooltip("Target to follow. If empty, the main camera (or a GameObject named 'Unarmed Idle 01') is used.")]
    [SerializeField] private Transform target;

    [Tooltip("Height above the target where the rain spawns.")]
    [SerializeField] private float heightAboveTarget = 14f;

    [Tooltip("How far ahead of the target (along its forward) to bias the rain volume.")]
    [SerializeField] private float forwardBias = 4f;

    [Tooltip("Smoothing speed for following. Higher snaps faster.")]
    [SerializeField] private float followSmooth = 8f;

    private void Start()
    {
        ResolveTarget();
        if (target != null)
            transform.position = SamplePosition();
        transform.rotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            ResolveTarget();
            if (target == null)
                return;
        }

        var desired = SamplePosition();
        float t = 1f - Mathf.Exp(-followSmooth * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desired, t);
        transform.rotation = Quaternion.identity;
    }

    private Vector3 SamplePosition()
    {
        var forward = target.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.001f)
            forward.Normalize();

        return target.position + Vector3.up * heightAboveTarget + forward * forwardBias;
    }

    private void ResolveTarget()
    {
        if (target != null)
            return;

        var playerCam = PlayerSceneTransition.GetActiveCamera();
        if (playerCam != null)
        {
            target = playerCam.transform;
            return;
        }

        var player = SceneObjectLocator.FindInScene(SceneManager.GetActiveScene(), "Unarmed Idle 01", 3000);
        if (player != null)
            target = player.transform;
    }
}
