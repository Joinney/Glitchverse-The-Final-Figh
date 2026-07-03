using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    void Start()
    {
        // Kiểm tra xem ở màn hình trước người chơi đã chọn nhân vật nào và độ khó gì
        string p1Selection = PlayerPrefs.GetString("P1_Selection", "Chưa chọn");
        string gameDifficulty = PlayerPrefs.GetString("GameDifficulty", "Normal");
        
        Debug.Log("=== PHÓ BẢN KHỞI CHẠY ===");
        Debug.Log("Nhân vật Player 1: " + p1Selection);
        Debug.Log("Độ khó quái/AI: " + gameDifficulty);
    }

    // Hàm này sẽ được gọi khi bạn click vào các nút Ải trên Bản đồ
    public void SelectStage(string stageSceneName)
    {
        Debug.Log("Đang tiến vào ải: " + stageSceneName);
        
        // Lưu lại tên ải hiện tại để hệ thống biết bạn đang đấu ở map nào
        PlayerPrefs.SetString("CurrentStage", stageSceneName);
        
        // Tải màn chơi chiến đấu thực sự
        SceneManager.LoadScene(stageSceneName);
    }
}