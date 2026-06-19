using UnityEngine;
using UnityEditor;
using UnityEditor.Animations; // Thư viện cần thiết để tạo và quản lý Animator Controller
using System.IO;

// Kế thừa từ EditorWindow để tạo ra một giao diện cửa sổ (Window) tùy chỉnh trên Unity Editor
public class SetupHumanoidAnimators : EditorWindow
{
    // Mảng lưu trữ tên của 3 model cần xử lý trên Scene theo đúng yêu cầu
    private readonly string[] targetModelNames = { "Villain1", "Ong 5", "CAND" };

    // Tạo menu item trên thanh công cụ của Unity (Nằm ở mục Tools -> Setup Humanoid Animators)
    [MenuItem("Tools/Setup Humanoid Animators")]
    public static void ShowWindow()
    {
        // Hiển thị cửa sổ công cụ
        GetWindow<SetupHumanoidAnimators>("Setup Animators");
    }

    // Hàm OnGUI chịu trách nhiệm vẽ giao diện người dùng (UI) cho cửa sổ Editor
    private void OnGUI()
    {
        GUILayout.Label("Cài đặt Animator cho Nhân vật", EditorStyles.boldLabel);

        // Nút bấm để thực thi toàn bộ logic của script
        if (GUILayout.Button("Chạy script thiết lập"))
        {
            SetupModels();
        }
    }

    // Hàm chứa logic xử lý chính
    private void SetupModels()
    {
        // 1. TỰ ĐỘNG TẠO THƯ MỤC 'Animators'
        string folderPath = "Assets/Animators";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Animators");
            Debug.Log("✔️ Đã tạo thư mục mới: " + folderPath);
        }

        int successCount = 0; // Biến đếm số lượng nhân vật được xử lý thành công

        // Duyệt qua từng tên model có trong danh sách
        foreach (string modelName in targetModelNames)
        {
            // Tìm GameObject có tên tương ứng đang có trên Scene
            GameObject modelGO = GameObject.Find(modelName);

            if (modelGO == null)
            {
                Debug.LogWarning($"❌ Không tìm thấy model '{modelName}' trong Scene. Vui lòng kiểm tra lại xem có gõ sai tên không.");
                continue; // Bỏ qua và xét nhân vật tiếp theo
            }

            // 2. KIỂM TRA THIẾT LẬP RIG -> HUMANOID
            // Để kiểm tra thuộc tính Rig, ta cần truy xuất ngược từ GameObject trên Scene về file gốc (Asset/Prefab)
            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(modelGO);
            if (!string.IsNullOrEmpty(assetPath))
            {
                // Lấy thông tin import của file gốc
                ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer != null)
                {
                    // So sánh kiểu Animation Type
                    if (importer.animationType != ModelImporterAnimationType.Human)
                    {
                        Debug.LogWarning($"⚠️ Model '{modelName}' CHƯA được thiết lập Rig là Humanoid. Vui lòng vào file Asset gốc để đổi!");
                    }
                    else
                    {
                        Debug.Log($"✔️ Model '{modelName}' đã chuẩn Rig Humanoid.");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ '{modelName}' không phải là một instance của Model/Prefab gốc. Hệ thống bỏ qua bước kiểm tra Rig.");
            }

            // 3. TỰ ĐỘNG GÁN COMPONENT ANIMATOR (NẾU CHƯA CÓ)
            Animator animator = modelGO.GetComponent<Animator>();
            if (animator == null)
            {
                // Sử dụng AddComponent để thêm Animator vào GameObject
                animator = modelGO.AddComponent<Animator>();
                Debug.Log($"✔️ Đã tự động thêm component Animator cho '{modelName}'.");
            }
            else
            {
                Debug.Log($"✔️ '{modelName}' đã có sẵn component Animator từ trước.");
            }

            // 4. TẠO VÀ GÁN ANIMATOR CONTROLLER CƠ BẢN
            string controllerPath = $"{folderPath}/{modelName}Controller.controller";

            // Thử load Controller xem đã tồn tại hay chưa
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                // Nếu chưa có, ta tiến hành tạo ra một Animator Controller mới
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                Debug.Log($"✔️ Đã tạo Animator Controller mới tại đường dẫn: {controllerPath}");
            }

            // Gán Animator Controller vừa tìm/tạo được vào Runtime Animator Controller của nhân vật
            if (animator.runtimeAnimatorController != controller)
            {
                animator.runtimeAnimatorController = controller;
                Debug.Log($"✔️ Đã gán thành công {modelName}Controller cho nhân vật '{modelName}'.");
            }

            successCount++;
        }

        // 5. IN RA LOG BÁO CÁO TIẾN TRÌNH TRÊN CONSOLE
        if (successCount == targetModelNames.Length)
        {
            // Sử dụng thẻ <color> của Unity Rich Text để log ra màu xanh lá khi thành công 100%
            Debug.Log($"<color=green><b>[THÀNH CÔNG] Đã hoàn tất quá trình thiết lập! Đã xử lý {successCount}/{targetModelNames.Length} nhân vật trên Scene.</b></color>");
        }
        else if (successCount > 0)
        {
            Debug.Log($"<color=orange><b>[CẢNH BÁO NHẸ] Hoàn thành thiết lập một phần. Chỉ xử lý được {successCount}/{targetModelNames.Length} nhân vật. Hãy kiểm tra lại log chi tiết.</b></color>");
        }
        else
        {
            Debug.Log("<color=red><b>[THẤT BẠI] Không có nhân vật nào được tìm thấy hoặc thiết lập. Hãy kiểm tra lại tên của các GameObject trên Scene.</b></color>");
        }
    }
}