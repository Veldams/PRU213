using UnityEngine;
using UnityEngine.InputSystem;

public class RealMovement : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 150f;
    public float animSmoothTime = 0.15f;

    private Animator anim;
    private float animSpeed;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private CharacterController characterController;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim != null)
            anim.applyRootMotion = false;
        characterController = GetComponent<CharacterController>();
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

        // Di chuyển: ưu tiên CharacterController để va chạm (lan can/collider) hoạt động chính xác.
        if (vertical != 0)
        {
            Vector3 move = transform.forward * vertical * moveSpeed * Time.deltaTime;
            if (characterController != null)
            {
                characterController.Move(move);
            }
            else
            {
                transform.Translate(Vector3.forward * vertical * moveSpeed * Time.deltaTime);
            }
        }
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
}
