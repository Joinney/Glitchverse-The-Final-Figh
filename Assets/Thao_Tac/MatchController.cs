using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MatchController : MonoBehaviour
{
    [Header("Giao diện Kết quả")]
    public GameObject gameOverPanel;
    public Image resultImage;
    public Sprite victorySprite;
    public Sprite gameoverSprite;

    [Header("Nút bấm (Chỉ hiện khi thắng)")]
    public GameObject nextStageButton;

    [Header("Tên Màn Chọn Map (Chơi Tiếp)")]
    public string selectMapSceneName = "SampleScene";

    [Header("Số thứ tự của Ải này (1 đến 5)")]
    public int currentStageIndex = 1;

    [Header("UI Điểm Số & Thông Báo")]
    public TextMeshProUGUI scoreText;      
    public GameObject newRecordAlert;      

    [Header("Cấu hình Đối Kháng (PvP BO3)")]
    public string mainMenuSceneName = "MainMenu"; 
    public TextMeshProUGUI pvpResultText;         
    public TextMeshProUGUI pvpScoreText;          
    public GameObject pvpNextRoundBtnObject;      
    public GameObject pvpBackBtnObject;           
    
    [Header("Chờ tự động sang Hiệp mới (Giây)")]
    public float autoNextRoundDelay = 3f;

    private bool matchEnded = false;
    private string gameMode;

    void Awake()
    {
        // ⚡ ĐẢM BẢO THỜI GIAN VÀ TRẠNG THÁI LUÔN HOẠT ĐỘNG BÌNH THƯỜNG KHI VÀO GAME
        Time.timeScale = 1f;
        CountdownManager.isCountdownFinished = true;
    }

    void Start()
    {
        gameMode = PlayerPrefs.GetString("GameMode", "Single");
        
        if (pvpResultText != null) pvpResultText.gameObject.SetActive(false);
        if (pvpScoreText != null) pvpScoreText.gameObject.SetActive(false);
    }

    // ==========================================
    // HÀM 1: XỬ LÝ KẾT THÚC CHƠI ĐƠN (SINH TỒN)
    // ==========================================
    public void EndMatch(bool isPlayerWin)
    {
        if (matchEnded) return;

        if (gameMode == "PvP")
        {
            int winnerIndex = 1;
            PlayerHealth[] allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
            foreach (PlayerHealth p in allPlayers)
            {
                if (p.currentHealth <= 0)
                {
                    CharacterController2D cc = p.GetComponent<CharacterController2D>();
                    if (cc != null && cc.playerIndex == 1) winnerIndex = 2; 
                    else winnerIndex = 1; 
                }
            }
            EndPvPMatch(winnerIndex);
            return; 
        }

        matchEnded = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (isPlayerWin)
        {
            if (resultImage != null) resultImage.sprite = victorySprite;
            if (nextStageButton != null) nextStageButton.SetActive(true);

            int baseScore = 5000;
            int healthBonus = 0;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerHealth pHealth = player.GetComponent<PlayerHealth>();
                if (pHealth != null && pHealth.maxHealth > 0)
                {
                    float healthPct = (float)pHealth.currentHealth / pHealth.maxHealth;
                    healthBonus = Mathf.RoundToInt(healthPct * 5000);
                }
            }

            int totalScore = baseScore + healthBonus;

            if (scoreText != null) scoreText.text = "ĐIỂM TỔNG: " + totalScore.ToString();

            string stageScoreKey = "BestScore_Stage_" + currentStageIndex;
            int bestScore = PlayerPrefs.GetInt(stageScoreKey, 0);

            bool isNewRecord = false;
            if (totalScore > bestScore) 
            { 
                PlayerPrefs.SetInt(stageScoreKey, totalScore); 
                isNewRecord = true; 
            }

            if (newRecordAlert != null) newRecordAlert.SetActive(isNewRecord);

            PlayerPrefs.SetInt("Current_Stage_Index", currentStageIndex + 1);
            PlayerPrefs.SetInt("Has_Saved_Game", 1);
            int currentUnlocked = PlayerPrefs.GetInt("UnlockedStage", 1);
            if (currentStageIndex >= currentUnlocked) PlayerPrefs.SetInt("UnlockedStage", currentStageIndex + 1);
            PlayerPrefs.Save();
        }
        else
        {
            if (resultImage != null) resultImage.sprite = gameoverSprite;
            if (nextStageButton != null) nextStageButton.SetActive(false);
            if (scoreText != null) scoreText.text = "THẤT BẠI";
            if (newRecordAlert != null) newRecordAlert.SetActive(false);

            PlayerPrefs.SetInt("Current_Stage_Index", currentStageIndex);
            PlayerPrefs.SetInt("Has_Saved_Game", 1);
            PlayerPrefs.Save();
        }

        FreezeAllCharacters();
    }

    // ==========================================
    // HÀM 2: XỬ LÝ ĐỐI KHÁNG PVP
    // ==========================================
    public void EndPvPMatch(int winningPlayerIndex)
    {
        if (matchEnded) return;
        matchEnded = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (scoreText != null) scoreText.gameObject.SetActive(false);
        if (newRecordAlert != null) newRecordAlert.SetActive(false);
        if (nextStageButton != null) nextStageButton.SetActive(false);
        if (resultImage != null) resultImage.gameObject.SetActive(false);
        
        string scoreKey = "PvP_P" + winningPlayerIndex + "_Score";
        int currentScore = PlayerPrefs.GetInt(scoreKey, 0) + 1; 
        PlayerPrefs.SetInt(scoreKey, currentScore);

        int p1Score = PlayerPrefs.GetInt("PvP_P1_Score", 0);
        int p2Score = PlayerPrefs.GetInt("PvP_P2_Score", 0);

        if (pvpScoreText != null)
        {
            pvpScoreText.gameObject.SetActive(true);
            pvpScoreText.text = $"TỈ SỐ\nP1 [{p1Score}] - [{p2Score}] P2";
        }

        if (pvpResultText != null) pvpResultText.gameObject.SetActive(true);

        if (currentScore >= 2)
        {
            if (pvpResultText != null) pvpResultText.text = "PLAYER " + winningPlayerIndex + "\nWINS THE MATCH!";
            if (pvpNextRoundBtnObject != null) pvpNextRoundBtnObject.SetActive(true);
            if (pvpBackBtnObject != null) pvpBackBtnObject.SetActive(true);
        }
        else
        {
            if (pvpResultText != null) pvpResultText.text = "PLAYER " + winningPlayerIndex + "\nWINS ROUND!";
            if (pvpNextRoundBtnObject != null) pvpNextRoundBtnObject.SetActive(false);
            if (pvpBackBtnObject != null) pvpBackBtnObject.SetActive(false);

            StartCoroutine(AutoNextRoundRoutine());
        }

        FreezeAllCharacters();
    }

    private IEnumerator AutoNextRoundRoutine()
    {
        yield return new WaitForSeconds(autoNextRoundDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void FreezeAllCharacters()
    {
        CharacterController2D[] allCharacters = FindObjectsByType<CharacterController2D>(FindObjectsSortMode.None);
        foreach (CharacterController2D character in allCharacters)
        {
            Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            character.StopAllCoroutines();
            character.enabled = false;
            Animator anim = character.GetComponent<Animator>();
            if (anim != null) { anim.SetFloat("Speed", 0); anim.SetBool("IsBlocking", false); }
        }
    }

    // ==========================================
    // CÁC HÀM XỬ LÝ SỰ KIỆN NÚT BẤM (BUTTONS)
    // ==========================================

    // 🔄 Nút Chơi Lại (Play Again)
    public void OnReplayClick() 
    { 
        Time.timeScale = 1f; 
        CountdownManager.isCountdownFinished = true;

        // 1. Bỏ chọn UI để bàn phím truyền trực tiếp vào nhân vật ngay lập tức
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        if (gameMode == "PvP")
        {
            PlayerPrefs.SetInt("PvP_P1_Score", 0);
            PlayerPrefs.SetInt("PvP_P2_Score", 0);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
    
    public void OnBackClick() 
    { 
        Time.timeScale = 1f;
        CountdownManager.isCountdownFinished = true;

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        if (gameMode == "PvP")
        {
            PlayerPrefs.SetInt("BackToCharSelect", 1);
            SceneManager.LoadScene(mainMenuSceneName); 
        }
        else
        {
            SceneManager.LoadScene("SampleScene"); 
        }
    }

    public void OnNextStageClick() 
    { 
        Time.timeScale = 1f;
        CountdownManager.isCountdownFinished = true;

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        SceneManager.LoadScene(selectMapSceneName); 
    }
}