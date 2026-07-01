using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class RealMovement : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 150f;
    public float animSmoothTime = 0.15f;
    public float gravity = -20f;
    [Header("Collider Alignment")]
    public bool autoFitCharacterController = false;
    public float minControllerHeight = 1.2f;

    private Animator anim;
    private float animSpeed;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private CharacterController characterController;
    private float verticalVelocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }

        // Ensure collision is always active for wall-hit behavior.
        characterController.detectCollisions = true;
        if (characterController.skinWidth < 0.02f)
            characterController.skinWidth = 0.08f;
    }

    void Start()
    {
        if (PlayerSceneTransition.GetPlayerRoot() == null)
            PlayerSceneTransition.CachePlayer(this);

        anim = GetComponent<Animator>();
        if (anim != null)
            anim.applyRootMotion = false;

        if (autoFitCharacterController)
            FitControllerToVisual();

        if (GetComponent<FootstepAudio>() == null)
            gameObject.AddComponent<FootstepAudio>();
    }

    void Update()
    {
        float vertical = 0f;
        float horizontal = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
        }

        // Always move through CharacterController so the player cannot pass through walls/models.
        Vector3 move = transform.forward * vertical * moveSpeed;
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;
        characterController.Move(move * Time.deltaTime);

        if (horizontal != 0f)
            transform.Rotate(0f, horizontal * rotationSpeed * Time.deltaTime, 0f, Space.World);

        if (anim != null)
        {
            // Chỉ chạy animation Run khi tiến/lùi (không khi chỉ xoay A/D)
            float targetSpeed = Mathf.Abs(vertical);
            animSpeed = Mathf.MoveTowards(animSpeed, targetSpeed, Time.deltaTime / animSmoothTime);
            anim.SetFloat(SpeedHash, animSpeed);
        }
    }

    void FitControllerToVisual()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
            return;

        bool hasBounds = false;
        Bounds visualBounds = new Bounds();
        foreach (var r in renderers)
        {
            if (r == null) continue;
            if (!hasBounds)
            {
                visualBounds = r.bounds;
                hasBounds = true;
            }
            else
            {
                visualBounds.Encapsulate(r.bounds);
            }
        }

        if (!hasBounds)
            return;

        Vector3 localCenter = transform.InverseTransformPoint(visualBounds.center);
        Vector3 localExtents = AbsVec(transform.InverseTransformVector(visualBounds.extents));

        float height = Mathf.Max(minControllerHeight, localExtents.y * 2f);
        float radius = Mathf.Max(0.15f, Mathf.Max(localExtents.x, localExtents.z) * 0.9f);

        characterController.height = height;
        characterController.radius = radius;
        characterController.center = new Vector3(localCenter.x, localCenter.y, localCenter.z);
        characterController.stepOffset = Mathf.Min(characterController.stepOffset, height * 0.45f);
    }

    static Vector3 AbsVec(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
}
