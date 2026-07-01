using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps a rain emitter hovering above the active camera so the rain volume
/// covers the whole viewport. The shape is a flat horizontal box; particles
/// fall in world space while the emitter follows the viewer.
/// </summary>
public class RainFollow : MonoBehaviour
{
    [Tooltip("Target to follow. If empty, the active player camera is used.")]
    [SerializeField] private Transform target;

    [Tooltip("Height above the target where rain spawns.")]
    [SerializeField] private float heightAboveTarget = 14f;

    [Tooltip("Extra margin added around the camera frustum when sizing the rain box.")]
    [SerializeField] private float coveragePadding = 12f;

    [Tooltip("Minimum width/depth of the rain emission box.")]
    [SerializeField] private float minCoverageSize = 80f;

    [Tooltip("Smoothing speed for following. Higher snaps faster.")]
    [SerializeField] private float followSmooth = 8f;

    private ParticleSystem _particleSystem;
    private Camera _camera;
    private float _lastConfiguredFov = -1f;
    private float _lastConfiguredAspect = -1f;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        if (_particleSystem != null)
        {
            var main = _particleSystem.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        }
    }

    private void Start()
    {
        ResolveTarget();
        if (target != null)
            transform.position = SamplePosition();
        transform.rotation = Quaternion.identity;
        ConfigureEmitterShape();
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
        ConfigureEmitterShape();
    }

    private Vector3 SamplePosition()
    {
        var anchor = target.position;
        return new Vector3(anchor.x, anchor.y + heightAboveTarget, anchor.z);
    }

    private void ConfigureEmitterShape()
    {
        if (_particleSystem == null)
            return;

        var cam = ResolveCamera();
        if (cam == null)
            return;

        if (Mathf.Approximately(_lastConfiguredFov, cam.fieldOfView)
            && Mathf.Approximately(_lastConfiguredAspect, cam.aspect))
            return;

        _lastConfiguredFov = cam.fieldOfView;
        _lastConfiguredAspect = cam.aspect;

        var shape = _particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.rotation = Vector3.zero;

        float verticalFovRad = cam.fieldOfView * Mathf.Deg2Rad;
        float horizontalFovRad = 2f * Mathf.Atan(Mathf.Tan(verticalFovRad * 0.5f) * cam.aspect);
        float halfWidth = Mathf.Tan(horizontalFovRad * 0.5f) * heightAboveTarget + coveragePadding;
        float halfDepth = Mathf.Tan(verticalFovRad * 0.5f) * heightAboveTarget + coveragePadding;
        halfWidth = Mathf.Max(halfWidth, minCoverageSize * 0.5f);
        halfDepth = Mathf.Max(halfDepth, minCoverageSize * 0.5f);

        shape.scale = new Vector3(halfWidth * 2f, 1f, halfDepth * 2f);
    }

    private Camera ResolveCamera()
    {
        if (_camera != null && _camera.transform == target)
            return _camera;

        _camera = target != null ? target.GetComponent<Camera>() : null;
        if (_camera == null)
            _camera = PlayerSceneTransition.GetActiveCamera();
        return _camera;
    }

    private void ResolveTarget()
    {
        if (target != null)
            return;

        var playerCam = PlayerSceneTransition.GetActiveCamera();
        if (playerCam != null)
        {
            target = playerCam.transform;
            _camera = playerCam;
            return;
        }

        var player = SceneObjectLocator.FindInScene(SceneManager.GetActiveScene(), "Unarmed Idle 01", 3000);
        if (player != null)
            target = player.transform;
    }
}
