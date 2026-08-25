using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject startMenuPanel;     
    public GameObject modeMenuPanel;      
    public GameObject difficultyPanel;    
    public GameObject characterSelectPanel;
    public GameObject singlePlayerPanel;  

    [Header("Sub Containers")]
    public GameObject gokuButtonsContainer; 

    [Header("Save System UI")]
    public Button continueButton; 

    [Header("Scoreboard (Bảng Thành Tích)")]
    public GameObject scoreboardPanel;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI totalTimeText;
    public GameObject scoreCupObject;

    void Start()
    {
        bool hasSavedGame = PlayerPrefs.GetInt("Has_Saved_Game", 0) == 1;
        if (continueButton != null) continueButton.interactable = hasSavedGame;

        // ==========================================
        // 💡 XỬ LÝ LỜI NHẮN QUAY LẠI TỪ TRONG GAME (ĐÃ ĐỒNG BỘ CHỮ)
        // ==========================================
        if (PlayerPrefs.GetInt("BackToCharSelect", 0) == 1)
        {
            PlayerPrefs.SetInt("BackToCharSelect", 0); // Xé thư

            if (startMenuPanel != null) startMenuPanel.SetActive(false);
            if (modeMenuPanel != null) modeMenuPanel.SetActive(false);
            if (difficultyPanel != null) difficultyPanel.SetActive(false);
            if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(false);

            // ĐỌC BẰNG CHỮ THAY VÌ SỐ ĐỂ TRÁNH XUNG ĐỘT
            string gameMode = PlayerPrefs.GetString("GameMode", "Single");

            if (gameMode == "Single" && singlePlayerPanel != null)
            {
                singlePlayerPanel.SetActive(true);
            }
            else if (gameMode == "PvP" && characterSelectPanel != null)
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

    public void OpenDifficultyMenu()
    {
        // 💡 LƯU BẰNG CHỮ "Single" CHO KHỚP VỚI HỆ THỐNG CHIẾN ĐẤU
        PlayerPrefs.SetString("GameMode", "Single");

        PlayerPrefs.SetInt("Current_Stage_Index", 1);
        PlayerPrefs.SetInt("UnlockedStage", 1);
        PlayerPrefs.SetInt("Has_Saved_Game", 0);

        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey("BestScore_Stage_" + i);
            PlayerPrefs.DeleteKey("BestTime_Stage_" + i);
        }
        PlayerPrefs.Save();

        gokuButtonsContainer.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    public void OnClickContinue()
    {
        if (PlayerPrefs.GetInt("Has_Saved_Game", 0) == 1)
        {
            // Bấm Continue thì mặc định là đang chơi Single
            PlayerPrefs.SetString("GameMode", "Single");
            
            int currentStage = PlayerPrefs.GetInt("Current_Stage_Index", 1);
            string savedSceneName = "Fight_Stage" + currentStage;
            SceneManager.LoadScene(savedSceneName);
        }
    }

    public void OpenScoreboard()
    {
        int totalScore = 0;
        float totalTime = 0f;

        for (int i = 1; i <= 5; i++)
        {
            totalScore += PlayerPrefs.GetInt("BestScore_Stage_" + i, 0);
            float stageTime = PlayerPrefs.GetFloat("BestTime_Stage_" + i, 9999f);
            if (stageTime < 9999f) totalTime += stageTime;
        }

        if (totalScoreText != null) totalScoreText.text = "TỔNG ĐIỂM: " + totalScore.ToString("N0");

        if (totalTimeText != null)
        {
            if (totalTime == 0f) totalTimeText.text = "THỜI GIAN: Chưa có";
            else
            {
                int minutes = Mathf.FloorToInt(totalTime / 60F);
                int seconds = Mathf.FloorToInt(totalTime - minutes * 60);
                totalTimeText.text = string.Format("TỔNG THỜI GIAN: {0:00}:{1:00}", minutes, seconds);
            }
        }

        if (scoreboardPanel != null) scoreboardPanel.SetActive(true);
        if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(false);

        GameObject cup = GameObject.Find("Score");
        if (cup != null)
        {
            cup.GetComponent<Image>().enabled = false;
            cup.GetComponent<Button>().enabled = false;
        }
    }

    public void CloseScoreboard()
    {
        if (scoreboardPanel != null) scoreboardPanel.SetActive(false);
        if (gokuButtonsContainer != null) gokuButtonsContainer.SetActive(true);

        GameObject cup = GameObject.Find("Score");
        if (cup != null)
        {
            cup.GetComponent<Image>().enabled = true;
            cup.GetComponent<Button>().enabled = true;
        }
    }

    public void OpenCharacterSelectDirectly()
    {
        PlayerPrefs.SetString("GameMode", "PvP");
        PlayerPrefs.SetInt("PvP_P1_Score", 0);
        PlayerPrefs.SetInt("PvP_P2_Score", 0);
        PlayerPrefs.Save();

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
        difficultyPanel.SetActive(false);
        modeMenuPanel.SetActive(false);
        characterSelectPanel.SetActive(false);

        if (singlePlayerPanel != null) singlePlayerPanel.SetActive(true);
    }

    public void ResetSaveData()
    {
        PlayerPrefs.SetInt("Current_Stage_Index", 1);
        PlayerPrefs.SetInt("Has_Saved_Game", 0);
        PlayerPrefs.Save();
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

    // ==========================================
    // 💡 HÀM THOÁT GAME (Dùng cho cả Editor & Bản Build)
    // ==========================================
    public void QuitGame()
    {
        Debug.Log("Đang thoát game...");

        // Nếu đang chạy trong Unity Editor thì dừng Play mode
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Khi build ra file .exe (Windows) hoặc chạy thực tế
        Application.Quit();
        #endif
    }
}