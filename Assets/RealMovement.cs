using UnityEngine;
using UnityEngine.InputSystem;

public class RealMovement : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 150f;
    
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
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

        // Di chuyển
        if (vertical != 0) transform.Translate(Vector3.forward * vertical * moveSpeed * Time.deltaTime);
        if (horizontal != 0) transform.Rotate(Vector3.up * horizontal * rotationSpeed * Time.deltaTime);

        // Gửi lệnh cho Animator để đổi animation từ Đứng Yên sang Chạy
        if (anim != null)
        {
            float speed = Mathf.Abs(vertical) + Mathf.Abs(horizontal);
            anim.SetFloat("Speed", speed);
        }
    }
}
