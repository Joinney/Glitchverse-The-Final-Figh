using UnityEngine;
using UnityEngine.SceneManagement;

public class PvPMapSelection : MonoBehaviour
{
    [Header("Giao Diện Panels")]
    public GameObject characterSelectPanel; // Bảng chọn tướng (để quay lại)
    public GameObject mapSelectPanel;       // Bảng chọn map (chính nó)

    // ==========================================
    // 1. HÀM QUAY LẠI CHỌN TƯỚNG (Gắn vào nút BACK)
    // ==========================================
    public void GoBackToCharSelect()
    {
        mapSelectPanel.SetActive(false);
        characterSelectPanel.SetActive(true);

        // Bắt hệ thống reset lại để chọn tướng từ đầu
        TwoPlayerCharSelection charManager = characterSelectPanel.GetComponent<TwoPlayerCharSelection>();
        if (charManager != null)
        {
            charManager.ResetSelection();
        }
    }

    // ==========================================
    // 2. HÀM CHỌN MAP VÀ VÀO GAME (Gắn vào các nút Map)
    // ==========================================
    // Lưu ý: Cần truyền tên chính xác của Scene Map (VD: "Map_1", "Map_2") vào Inspector
    public void SelectMapAndStart(string mapSceneName)
    {
        // Khẳng định lại một lần nữa đây là chế độ PvP để Scene Map biết đường xử lý
        PlayerPrefs.SetString("GameMode", "PvP");
        PlayerPrefs.Save();

        Debug.Log("TRẬN CHIẾN PVP BẮT ĐẦU TẠI: " + mapSceneName);

        // Tải cảnh (Load Scene)
        SceneManager.LoadScene(mapSceneName);
    }
}