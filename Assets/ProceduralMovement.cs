using UnityEngine;
using UnityEngine.InputSystem;

public class ProceduralMovement : MonoBehaviour
{
    public float moveSpeed = 3f;      // Tốc độ đi tới/lui
    public float rotationSpeed = 100f; // Tốc độ xoay trái/phải
    public float stepSpeed = 15f;      // Tốc độ nhún nhảy
    public float bounceHeight = 0.2f;  // Độ cao nhún
    public float wobbleAngle = 10f;    // Góc lắc lư trái phải
    
    private float walkTime = 0f;
    private Transform visualMesh;      // Khối 3D hiển thị bên trong
    private CharacterController characterController;

    void Start()
    {
        // Tắt Animator nếu có vì model không có xương
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        // Lấy model 3D bên trong (con của GameObject hiện tại)
        if (transform.childCount > 0)
        {
            visualMesh = transform.GetChild(0);
        }
        else
        {
            visualMesh = transform; // Fallback
        }

        characterController = GetComponent<CharacterController>();

        if (GetComponent<FootstepAudio>() == null)
            gameObject.AddComponent<FootstepAudio>();
    }

    void LateUpdate()
    {
        // 1. NHẬN NÚT BẤM (New Input System)
        float vertical = 0f;
        float horizontal = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
        }

        // 2. DI CHUYỂN TOÀN BỘ CƠ THỂ
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
        if (horizontal != 0) transform.Rotate(Vector3.up * horizontal * rotationSpeed * Time.deltaTime);

        // 3. TẠO CỬ ĐỘNG BƯỚC ĐI CHO MODEL KHÔNG CÓ XƯƠNG (Wobble & Bounce)
        if (visualMesh != null && visualMesh != transform)
        {
            float speed = Mathf.Max(Mathf.Abs(vertical), Mathf.Abs(horizontal));
            
            if (speed > 0.1f)
            {
                walkTime += Time.deltaTime * stepSpeed;
                
                // Tính toán nhún lên xuống (Bounce) bằng giá trị tuyệt đối của Sin để luôn nảy lên
                float bounce = Mathf.Abs(Mathf.Sin(walkTime)) * bounceHeight;
                
                // Tính toán lắc lư trái phải (Wobble)
                float wobble = Mathf.Sin(walkTime) * wobbleAngle;

                // Áp dụng nhún và lắc lư vào phần hiển thị
                visualMesh.localPosition = new Vector3(0, bounce, 0);
                visualMesh.localRotation = Quaternion.Euler(0, 0, wobble);
            }
            else
            {
                walkTime = 0f;
                // Từ từ đưa nhân vật trở về tư thế đứng yên
                visualMesh.localPosition = Vector3.Lerp(visualMesh.localPosition, Vector3.zero, Time.deltaTime * 10f);
                visualMesh.localRotation = Quaternion.Lerp(visualMesh.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            }
        }
    }
}
