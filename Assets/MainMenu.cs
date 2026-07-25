using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // BẮT BUỘC để dùng Text hiển thị điểm

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject startMenuPanel;     // Màn hình Start ban đầu
    public GameObject modeMenuPanel;      // Màn hình Goku
    public GameObject difficultyPanel;    // Màn hình chọn độ khó
    public GameObject characterSelectPanel;
    public GameObject singlePlayerPanel;  // Ô CHỨA MÀN HÌNH CHƠI ĐƠN 

    [Header("Sub Containers")]
    public GameObject gokuButtonsContainer; // Nhóm chứa các nút (New Game, Continue...)

    [Header("Save System UI")]
    public Button continueButton; // Nút Continue

    // ==========================================
    // PHẦN MỚI: BẢNG KỶ LỤC CÚP VÀNG
    // ==========================================
    [Header("Scoreboard (Bảng Thành Tích)")]
    public GameObject scoreboardPanel;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI totalTimeText;

    public GameObject scoreCupObject;
    // ==========================================
    // HÀM CHẠY NGAY KHI VỪA MỞ MÀN HÌNH MENU
    // ==========================================
    void Start()
    {
        // Kiểm tra file save để bật/tắt nút Continue
        bool hasSavedGame = PlayerPrefs.GetInt("Has_Saved_Game", 0) == 1;
        if (continueButton != null)
        {
            continueButton.interactable = hasSavedGame;
        }

        // Xử lý lời nhắn quay lại từ trong game
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
    // NÚT "NEW GAME" (Chơi 1 mình vs AI)
    // ==========================================
    public void OpenDifficultyMenu()
    {
        PlayerPrefs.SetInt("GameMode", 1);

        // --- RESET TIẾN TRÌNH CHƠI ---
        PlayerPrefs.SetInt("Current_Stage_Index", 1);
        PlayerPrefs.SetInt("UnlockedStage", 1);
        PlayerPrefs.SetInt("Has_Saved_Game", 0);

        // --- MỚI THÊM: XÓA SẠCH ĐIỂM SỐ KỶ LỤC CŨ ---
        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey("BestScore_Stage_" + i);
            PlayerPrefs.DeleteKey("BestTime_Stage_" + i);
        }

        PlayerPrefs.Save();
        Debug.Log("Chế độ: NEW GAME -> Đã reset toàn bộ bản đồ, kỷ lục và mở chọn độ khó.");

        gokuButtonsContainer.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    // ==========================================
    // NÚT "CONTINUE" (Chơi tiếp màn dở dang)
    // ==========================================
    public void OnClickContinue()
    {
        if (PlayerPrefs.GetInt("Has_Saved_Game", 0) == 1)
        {
            int currentStage = PlayerPrefs.GetInt("Current_Stage_Index", 1);
            string savedSceneName = "Fight_Stage" + currentStage;
            string p1Name = PlayerPrefs.GetString("P1_Selection", "Gojo");
            string diff = PlayerPrefs.GetString("GameDifficulty", "Normal");

            Debug.Log($"[CONTINUE] Đang tải trận: P1 {p1Name} | Độ khó: {diff} | Ải: {savedSceneName}");
            SceneManager.LoadScene(savedSceneName);
        }
    }

    // ==========================================
    // HỆ THỐNG GIAO DIỆN KỶ LỤC (CÚP VÀNG)
    // ==========================================
    public void OpenScoreboard()
    {
        int totalScore = 0;
        float totalTime = 0f;

        for (int i = 1; i <= 5; i++)
        {
            totalScore += PlayerPrefs.GetInt("BestScore_Stage_" + i, 0);

            float stageTime = PlayerPrefs.GetFloat("BestTime_Stage_" + i, 9999f);
            if (stageTime < 9999f)
            {
                totalTime += stageTime;
            }
        }

        if (totalScoreText != null)
        {
            totalScoreText.text = "TỔNG ĐIỂM: " + totalScore.ToString("N0");
        }

        if (totalTimeText != null)
        {
            if (totalTime == 0f)
            {
                totalTimeText.text = "THỜI GIAN: Chưa có";
            }
            else
            {
                int minutes = Mathf.FloorToInt(totalTime / 60F);
                int seconds = Mathf.FloorToInt(totalTime - minutes * 60);
                totalTimeText.text = string.Format("TỔNG THỜI GIAN: {0:00}:{1:00}", minutes, seconds);
            }
        }

        // 1. Hiển thị bảng thành tích
        if (scoreboardPanel != null) scoreboardPanel.SetActive(true);

        // 2. Ẩn nhóm nút bấm menu
        if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(false);

        // 3. AN TOÀN: Chỉ tắt hình ảnh và nút bấm của cái cúp (Không tắt GameObject nên không sợ mất tích)
        GameObject cup = GameObject.Find("Score");
        if (cup != null)
        {
            cup.GetComponent<Image>().enabled = false;
            cup.GetComponent<Button>().enabled = false;
        }
    }

    public void CloseScoreboard()
    {
        // 1. Tắt bảng thành tích
        if (scoreboardPanel != null) scoreboardPanel.SetActive(false);

        // 2. Hiện lại nhóm nút bấm menu
        if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(true);

        // 3. Bật lại hình ảnh và nút bấm cho cái cúp
        GameObject cup = GameObject.Find("Score");
        if (cup != null)
        {
            cup.GetComponent<Image>().enabled = true;
            cup.GetComponent<Button>().enabled = true;
        }
    }

    // ==========================================
    // CÁC NÚT ĐIỀU HƯỚNG KHÁC
    // ==========================================
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
        PlayerPrefs.SetInt("Has_Saved_Game", 0);
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