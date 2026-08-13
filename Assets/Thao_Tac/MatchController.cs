using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // 💡 BẮT BUỘC PHẢI THÊM DÒNG NÀY ĐỂ DÙNG TÍNH NĂNG TỰ ĐỘNG ĐẾM GIÂY

public class MatchController : MonoBehaviour
{
    [Header("Giao diện Kết quả")]
    public GameObject gameOverPanel;
    public Image resultImage;
    public Sprite victorySprite;
    public Sprite gameoverSprite;

    [Header("Nút bấm (Chỉ hiện khi thắng)")]
    public GameObject nextStageButton;

    [Header("Tên Ải Tiếp Theo")]
    public string nextStageName = "Fight_Stage2";

    [Header("Số thứ tự của Ải này (1 đến 5)")]
    public int currentStageIndex = 1;

    [Header("UI Kỷ Lục & Điểm Số")]
    public TextMeshProUGUI timeText;       
    public TextMeshProUGUI scoreText;      
    public GameObject newRecordAlert;      

    [Header("Cấu hình Đối Kháng (PvP BO3)")]
    public string mainMenuSceneName = "MainMenu"; 
    public TextMeshProUGUI pvpResultText;         
    public TextMeshProUGUI pvpScoreText;          
    public GameObject pvpNextRoundBtnObject;      
    public GameObject pvpBackBtnObject;           
    
    // ==========================================
    // 💡 TỐI ƯU TRẢI NGHIỆM: ĐỘ TRỄ TỰ ĐỘNG RESET TRẬN
    // ==========================================
    [Header("Chờ tự động sang Hiệp mới (Giây)")]
    public float autoNextRoundDelay = 3f; // Để người chơi kịp ngắm tỉ số 3 giây rồi auto lướt qua ván mới

    private float matchTime = 0f;
    private bool matchEnded = false;
    private string gameMode;

    void Start()
    {
        gameMode = PlayerPrefs.GetString("GameMode", "Single");
        
        if (pvpResultText != null) pvpResultText.gameObject.SetActive(false);
        if (pvpScoreText != null) pvpScoreText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (CountdownManager.isCountdownFinished && !matchEnded)
        {
            matchTime += Time.deltaTime;
        }
    }

    // ==========================================
    // HÀM 1: BỘ ĐÁNH CHẶN VÀ XỬ LÝ CHƠI ĐƠN
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

        // ... [GIỮ NGUYÊN CODE TÍNH ĐIỂM CHƠI ĐƠN] ...
        if (isPlayerWin)
        {
            if (resultImage != null) resultImage.sprite = victorySprite;
            if (nextStageButton != null) nextStageButton.SetActive(true);

            int minutes = Mathf.FloorToInt(matchTime / 60F);
            int seconds = Mathf.FloorToInt(matchTime - minutes * 60);
            string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

            int baseScore = 1000;
            int timeBonus = Mathf.Max(0, 5000 - Mathf.RoundToInt(matchTime * 50));
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

            int totalScore = baseScore + timeBonus + healthBonus;

            if (timeText != null) timeText.text = "THỜI GIAN: " + timeString;
            if (scoreText != null) scoreText.text = "ĐIỂM TỔNG: " + totalScore.ToString();

            string stageScoreKey = "BestScore_Stage_" + currentStageIndex;
            string stageTimeKey = "BestTime_Stage_" + currentStageIndex;

            int bestScore = PlayerPrefs.GetInt(stageScoreKey, 0);
            float bestTime = PlayerPrefs.GetFloat(stageTimeKey, 9999f);

            bool isNewRecord = false;
            if (totalScore > bestScore) { PlayerPrefs.SetInt(stageScoreKey, totalScore); isNewRecord = true; }
            if (matchTime < bestTime) { PlayerPrefs.SetFloat(stageTimeKey, matchTime); isNewRecord = true; }

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
            if (timeText != null) timeText.text = "--:--";
            if (scoreText != null) scoreText.text = "THẤT BẠI";
            if (newRecordAlert != null) newRecordAlert.SetActive(false);

            PlayerPrefs.SetInt("Current_Stage_Index", currentStageIndex);
            PlayerPrefs.SetInt("Has_Saved_Game", 1);
            PlayerPrefs.Save();
        }

        FreezeAllCharacters();
    }

    // ==========================================
    // HÀM 2: XỬ LÝ ĐỐI KHÁNG ĐẾM HIỆP CỰC MƯỢT
    // ==========================================
    public void EndPvPMatch(int winningPlayerIndex)
    {
        if (matchEnded) return;
        matchEnded = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (timeText != null) timeText.gameObject.SetActive(false);
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

        // ==========================================
        // 💡 CẬP NHẬT: LUÔN HIỆN PLAY AGAIN KHI KẾT THÚC TRẬN
        // ==========================================
        if (currentScore >= 2)
        {
            // THẮNG CHUNG CUỘC: Hiện chữ to và HIỆN CẢ 2 NÚT BẤM
            if (pvpResultText != null) pvpResultText.text = "PLAYER " + winningPlayerIndex + "\nWINS THE MATCH!";
            
            if (pvpNextRoundBtnObject != null) pvpNextRoundBtnObject.SetActive(true); // Bật nút PLAY AGAIN
            if (pvpBackBtnObject != null) pvpBackBtnObject.SetActive(true);           // Bật nút BACK
        }
        else
        {
            // MỚI THẮNG 1 HIỆP: Ẩn nút đi, để 3 giây auto-reset chạy mượt
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
    // 💡 CẬP NHẬT: NÚT PLAY AGAIN SẼ RESET TỈ SỐ PVP
    // ==========================================
    public void OnReplayClick() 
    { 
        if (gameMode == "PvP")
        {
            // Nếu bấm chơi lại trận mới, phải trả tỉ số về 0-0!
            PlayerPrefs.SetInt("PvP_P1_Score", 0);
            PlayerPrefs.SetInt("PvP_P2_Score", 0);
            PlayerPrefs.Save();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
    
    public void OnBackClick() 
    { 
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
        if (!string.IsNullOrEmpty(nextStageName)) SceneManager.LoadScene(nextStageName); 
    }
}
