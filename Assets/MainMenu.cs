using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // BẮT BUỘC phải có dòng này để dùng Button

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

    [Header("Save System UI")]
    public Button continueButton; // Kéo nút Continue trên Canvas vào đây

    // ==========================================
    // HÀM CHẠY NGAY KHI VỪA MỞ MÀN HÌNH MENU LÊN
    // ==========================================
    void Start()
    {
        // --- LOGIC MỚI: KIỂM TRA FILE SAVE ĐỂ BẬT/TẮT NÚT CONTINUE ---
        bool hasSavedGame = PlayerPrefs.GetInt("Has_Saved_Game", 0) == 1;
        if (continueButton != null)
        {
            continueButton.interactable = hasSavedGame; // Có file save thì nút sáng lên, không thì mờ đi không bấm được
        }

        // --- LOGIC CŨ: XỬ LÝ LỜI NHẮN BACK TỪ TRONG BẢN ĐỒ ---
        if (PlayerPrefs.GetInt("BackToCharSelect", 0) == 1)
        {
            PlayerPrefs.SetInt("BackToCharSelect", 0);

            if (startMenuPanel != null) startMenuPanel.SetActive(false);
            if (modeMenuPanel != null) modeMenuPanel.SetActive(false);
            if (difficultyPanel != null) difficultyPanel.SetActive(false);
            if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(false);

            int gameMode = PlayerPrefs.GetInt("GameMode", 1);

            if (gameMode == 1 && singlePlayerPanel != null)
            {
                singlePlayerPanel.SetActive(true);
            }
            else if (gameMode == 2 && characterSelectPanel != null)
            {
                characterSelectPanel.SetActive(true);
            }
        }
    }

    public void OpenModeMenu()
    {
        startMenuPanel.SetActive(false);
        modeMenuPanel.SetActive(true);
        gokuButtonsContainer.SetActive(true);
        difficultyPanel.SetActive(false);
    }

    public void GoBackToStartMenu()
    {
        modeMenuPanel.SetActive(false);
        startMenuPanel.SetActive(true);
    }

    // ==========================================
    // 3. NÚT "NEW GAME" (Chơi 1 mình vs AI)
    // ==========================================
    public void OpenDifficultyMenu()
    {
        PlayerPrefs.SetInt("GameMode", 1);

        PlayerPrefs.SetInt("Current_Stage_Index", 1); // Reset ải cho nút Continue
        PlayerPrefs.SetInt("UnlockedStage", 1);       // Khóa lại toàn bộ Bản Đồ, chỉ mở Ải 1
        PlayerPrefs.SetInt("Has_Saved_Game", 0);      // Tắt nút Continue cho đến khi thắng ải 1
        PlayerPrefs.Save();
        // ---------------------------------------------------

        Debug.Log("Chế độ: NEW GAME -> Đã reset toàn bộ bản đồ và mở chọn độ khó.");

        gokuButtonsContainer.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    // ==========================================
    // 3.1 NÚT "CONTINUE" (Chơi tiếp màn dở dang)
    // ==========================================
    public void OnClickContinue()
    {
        // Kiểm tra chắc chắn là có file Save mới cho chạy
        if (PlayerPrefs.GetInt("Has_Saved_Game", 0) == 1)
        {
            int currentStage = PlayerPrefs.GetInt("Current_Stage_Index", 1);

            // Format tên Scene (Ví dụ: Fight_Stage1, Fight_Stage2...)
            string savedSceneName = "Fight_Stage" + currentStage;

            string p1Name = PlayerPrefs.GetString("P1_Selection", "Gojo");
            string diff = PlayerPrefs.GetString("GameDifficulty", "Normal");

            Debug.Log($"[CONTINUE] Đang tải trận: P1 {p1Name} | Độ khó: {diff} | Ải: {savedSceneName}");

            // LOAD THẲNG VÀO TRẬN, BỎ QUA MỌI THỨ!
            SceneManager.LoadScene(savedSceneName);
        }
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

    public void GoBackToModeMenu()
    {
        difficultyPanel.SetActive(false);
        gokuButtonsContainer.SetActive(true);
    }

    public void StartGameWithDifficulty(string difficulty)
    {
        PlayerPrefs.SetString("GameDifficulty", difficulty);
        Debug.Log("Đã chọn độ khó: " + difficulty + " -> Chuyển sang màn hình chơi đơn.");

        difficultyPanel.SetActive(false);
        modeMenuPanel.SetActive(false);
        characterSelectPanel.SetActive(false);

        if (singlePlayerPanel != null)
        {
            singlePlayerPanel.SetActive(true);
        }
    }

    public void ResetSaveData()
    {
        PlayerPrefs.SetInt("Current_Stage_Index", 1);
        PlayerPrefs.SetInt("Has_Saved_Game", 0); // Hủy file Save
        PlayerPrefs.Save();
        Debug.Log("ĐÃ RESET GAME: Mất file Continue, quay về Ải 1");
    }

    public void GoBackFromCharacterSelect()
    {
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (modeMenuPanel != null) modeMenuPanel.SetActive(true);
        if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(true);
    }

    public void GoBackFromSinglePlayer()
    {
        if (singlePlayerPanel != null) singlePlayerPanel.SetActive(false);
        if (modeMenuPanel != null) modeMenuPanel.SetActive(true);
        if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(true);
    }
}