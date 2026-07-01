using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerViewSwitcher : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Key toggleViewKey = Key.V;
    [SerializeField] private bool startInFirstPerson = false;

    [Header("First Person")]
    [SerializeField] private Vector3 firstPersonLocalOffset = new Vector3(0f, 1.2f, 0.05f);

    [Header("Third Person")]
    [SerializeField] private Vector3 thirdPersonFocusOffset = new Vector3(0f, 1.7f, 0f);
    [SerializeField] private Vector3 thirdPersonLocalOffset = new Vector3(0.35f, 1.8f, -4.2f);
    [SerializeField] private float thirdPersonFov = 85f;
    [SerializeField] private float thirdPersonSmooth = 12f;

    [Header("Third Person Collision")]
    [SerializeField] private float cameraCollisionRadius = 0.2f;
    [SerializeField] private float minDistanceWhenBlocked = 1.6f;
    [SerializeField] private LayerMask collisionMask = Physics.DefaultRaycastLayers;

    private bool _isFirstPerson;
    private int _sphereCastFrameSkip;
    private float _cachedFinalDistance = -1f;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>(true);

        _isFirstPerson = startInFirstPerson;
    }

    private void LateUpdate()
    {
        if (playerCamera == null)
            return;

        if (Keyboard.current != null && Keyboard.current[toggleViewKey].wasPressedThisFrame)
            _isFirstPerson = !_isFirstPerson;

        if (_isFirstPerson)
            ApplyFirstPerson();
        else
            ApplyThirdPersonWithCollision();
    }

    private void ApplyFirstPerson()
    {
        var targetPos = transform.TransformPoint(firstPersonLocalOffset);
        var targetRot = Quaternion.LookRotation(transform.forward, Vector3.up);
        playerCamera.transform.SetPositionAndRotation(targetPos, targetRot);
    }

    private void ApplyThirdPersonWithCollision()
    {
        var focus = transform.TransformPoint(thirdPersonFocusOffset);
        var desiredPos = transform.TransformPoint(thirdPersonLocalOffset);

        var toDesired = desiredPos - focus;
        var direction = toDesired.normalized;
        var maxDist = toDesired.magnitude;
        var finalDistance = maxDist;
        var isBlocked = false;

        if ((_sphereCastFrameSkip++ & 1) == 0)
        {
            finalDistance = maxDist;
            if (Physics.SphereCast(focus, cameraCollisionRadius, direction, out var hit, maxDist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                isBlocked = true;
                finalDistance = Mathf.Max(minDistanceWhenBlocked, hit.distance - cameraCollisionRadius);
            }

            _cachedFinalDistance = finalDistance;
        }
        else if (_cachedFinalDistance >= 0f)
        {
            finalDistance = _cachedFinalDistance;
            isBlocked = finalDistance < maxDist - 0.05f;
        }

        var finalPos = focus + direction * finalDistance;
        var smooth = isBlocked ? thirdPersonSmooth * 2.2f : thirdPersonSmooth;
        var lerpT = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, finalPos, lerpT);
        playerCamera.transform.rotation = Quaternion.LookRotation((focus - playerCamera.transform.position).normalized, Vector3.up);
        playerCamera.fieldOfView = thirdPersonFov;
    }
}
