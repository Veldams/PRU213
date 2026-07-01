using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Hàm này được gọi khi người chơi bấm nút "Bắt đầu"
    public void StartGame()
    {
        // Tải Scene tiếp theo trong danh sách Build Settings (thường là Scene có index 1)
        // Bạn nhớ Add Scene vào phần File -> Build Settings nhé
        SceneLoader.Load("SampleScene");
    }

    // Hàm này được gọi khi người chơi bấm nút "Cài đặt"
    public void OpenSettings()
    {
        // Thường dùng để bật/tắt một Canvas Panel chứa các thanh trượt âm thanh, đồ họa
        Debug.Log("Đang mở bảng Cài đặt...");
        // Ví dụ: settingsPanel.SetActive(true);
    }

    // Hàm này được gọi khi người chơi bấm nút "Thoát"
    public void QuitGame()
    {
        // Lưu ý: Application.Quit() chỉ hoạt động khi game đã được Build ra file .exe hoặc .apk
        // Nó sẽ không tắt cửa sổ Play trong Unity Editor
        Debug.Log("Đã thoát khỏi trò chơi!");
        Application.Quit();
    }
}