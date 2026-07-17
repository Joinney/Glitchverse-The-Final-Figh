using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject startMenuPanel;     // Màn hình Start ban đầu (ảnh nền chung + nút START)
    public GameObject modeMenuPanel;      // Màn hình Goku (chứa ảnh nền Goku)
    public GameObject difficultyPanel;    // Màn hình chọn độ khó (chữ SELECT + các nút độ khó)
    public GameObject characterSelectPanel;
    public GameObject singlePlayerPanel;  // Ô CHỨA MÀN HÌNH CHƠI ĐƠN MỚI TÍNH

    [Header("Sub Containers")]
    public GameObject gokuButtonsContainer; // Nhóm chứa các nút cũ (New Game, Continue, Play More, BACK)

    // ==========================================
    // HÀM CHẠY NGAY KHI VỪA MỞ MÀN HÌNH MENU LÊN
    // ==========================================
    void Start()
    {
        // Kiểm tra xem có lời nhắn yêu cầu vào thẳng Chọn Nhân Vật từ Bản đồ không?
        if (PlayerPrefs.GetInt("BackToCharSelect", 0) == 1)
        {
            // 1. Đã đọc xong thì phải xóa lời nhắn đi (để lần sau mở game lên không bị nhảy bậy)
            PlayerPrefs.SetInt("BackToCharSelect", 0);

            // 2. Giấu hết các màn hình Menu bên ngoài đi
            if (startMenuPanel != null) startMenuPanel.SetActive(false);
            if (modeMenuPanel != null) modeMenuPanel.SetActive(false);
            if (difficultyPanel != null) difficultyPanel.SetActive(false);
            if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(false);

            // 3. Mở đúng bảng Chọn nhân vật tùy theo chế độ đang chơi
            int gameMode = PlayerPrefs.GetInt("GameMode", 1); // Đọc xem đang chơi Single (1) hay 2 Players (2)

            if (gameMode == 1 && singlePlayerPanel != null)
            {
                singlePlayerPanel.SetActive(true); // Mở bảng chơi đơn
            }
            else if (gameMode == 2 && characterSelectPanel != null)
            {
                characterSelectPanel.SetActive(true); // Mở bảng chơi 2 người
            }
        }
    }

    // 1. Gắn vào nút START ban đầu
    public void OpenModeMenu()
    {
        startMenuPanel.SetActive(false);
        modeMenuPanel.SetActive(true);
        gokuButtonsContainer.SetActive(true);
        difficultyPanel.SetActive(false);
    }

    // 2. Gắn vào nút BACK trên màn hình Goku (Quay về màn hình Start ban đầu)
    public void GoBackToStartMenu()
    {
        modeMenuPanel.SetActive(false);
        startMenuPanel.SetActive(true);
    }

    // 3. Gắn vào nút NEW GAME trên màn hình Goku (Chơi 1 mình vs AI)
    public void OpenDifficultyMenu()
    {
        PlayerPrefs.SetInt("GameMode", 1);
        Debug.Log("Chế độ: CHƠI 1 NGƯỜI -> Mở chọn độ khó.");

        gokuButtonsContainer.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    // 3.5 Gắn vào nút CHƠI 2 NGƯỜI trên màn hình Goku
    public void OpenCharacterSelectDirectly()
    {
        PlayerPrefs.SetInt("GameMode", 2);
        Debug.Log("Chế độ: CHƠI 2 NGƯỜI -> Vào thẳng màn chọn nhân vật.");

        gokuButtonsContainer.SetActive(false);
        modeMenuPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
    }

    // 4. Gắn vào nút BACK trên màn hình chọn Độ khó (Quay về màn hình Goku cũ)
    public void GoBackToModeMenu()
    {
        difficultyPanel.SetActive(false);
        gokuButtonsContainer.SetActive(true);
    }

    // 5. Gắn vào nút NORMAL hoặc HARD để chuyển sang giao diện chơi đơn mới
    public void StartGameWithDifficulty(string difficulty)
    {
        PlayerPrefs.SetString("GameDifficulty", difficulty);
        Debug.Log("Đã chọn độ khó: " + difficulty + " -> Chuyển sang màn hình chơi đơn.");

        // ẨN CÁC PANEL CŨ ĐI
        difficultyPanel.SetActive(false);
        modeMenuPanel.SetActive(false);
        characterSelectPanel.SetActive(false); // Đảm bảo ẩn màn chọn nhân vật PVP đi

        // HIỆN MÀN HÌNH CHƠI ĐƠN MỚI LÊN!
        if (singlePlayerPanel != null)
        {
            singlePlayerPanel.SetActive(true);
        }
    }

    // ==========================================
    // --- ĐOẠN MỚI THÊM ĐỂ RESET TIẾN TRÌNH ---
    // ==========================================
    public void ResetSaveData()
    {
        PlayerPrefs.SetInt("UnlockedStage", 1);
        PlayerPrefs.Save();
        Debug.Log("ĐÃ RESET GAME: Tiến trình quay về Ải 1");
    }

    // ==========================================
    // --- NÚT BACK TỪ MÀN CHỌN NHÂN VẬT ---
    // ==========================================

    // Gắn vào nút Back của màn hình chơi 2 Người
    public void GoBackFromCharacterSelect()
    {
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (modeMenuPanel != null) modeMenuPanel.SetActive(true);
        if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(true);

        Debug.Log("Quay lại màn hình Goku từ Chọn 2 Người");
    }

    // Gắn vào nút Back của màn hình chơi Đơn
    public void GoBackFromSinglePlayer()
    {
        if (singlePlayerPanel != null) singlePlayerPanel.SetActive(false);
        if (modeMenuPanel != null) modeMenuPanel.SetActive(true);
        if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(true);

        Debug.Log("Quay lại màn hình Goku từ Chọn 1 Người");
    }
}