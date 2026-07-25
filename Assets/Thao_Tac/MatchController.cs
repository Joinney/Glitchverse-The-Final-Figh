using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Bắt buộc phải có để dùng TextMeshPro hiển thị điểm

public class MatchController : MonoBehaviour
{
    [Header("Giao diện Kết quả")]
    public GameObject gameOverPanel;
    public Image resultImage;

    [Header("Kho Ảnh Mẫu")]
    public Sprite victorySprite;
    public Sprite gameoverSprite;

    [Header("Nút bấm (Chỉ hiện khi thắng)")]
    public GameObject nextStageButton;

    [Header("Tên Ải Tiếp Theo")]
    public string nextStageName = "Fight_Stage2";

    [Header("Số thứ tự của Ải này (1 đến 5)")]
    public int currentStageIndex = 1;

    // --- PHẦN MỚI THÊM: GIAO DIỆN KỶ LỤC VÀ ĐIỂM SỐ ---
    [Header("UI Kỷ Lục & Điểm Số")]
    public TextMeshProUGUI timeText;       // Kéo Text hiển thị thời gian vào đây
    public TextMeshProUGUI scoreText;      // Kéo Text hiển thị điểm số vào đây
    public GameObject newRecordAlert;      // Kéo Chữ "NEW RECORD!" vào đây

    private float matchTime = 0f;
    private bool matchEnded = false;

    void Update()
    {
        // Bấm giờ: Trận đấu đang diễn ra và đếm ngược đã xong
        if (CountdownManager.isCountdownFinished && !matchEnded)
        {
            matchTime += Time.deltaTime;
        }
    }

    // HÀM XỬ LÝ KẾT THÚC TRẬN ĐẤU
    public void EndMatch(bool isPlayerWin)
    {
        if (matchEnded) return;
        matchEnded = true;

        // 1. Hiện bảng kết quả lên
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // 2. Tráo ảnh, Quản lý nút bấm và LƯU TIẾN TRÌNH SAVE GAME
        if (isPlayerWin)
        {
            if (resultImage != null) resultImage.sprite = victorySprite;
            if (nextStageButton != null) nextStageButton.SetActive(true);

            // ==========================================
            // TÍNH TOÁN ĐIỂM SỐ VÀ THỜI GIAN
            // ==========================================
            int minutes = Mathf.FloorToInt(matchTime / 60F);
            int seconds = Mathf.FloorToInt(matchTime - minutes * 60);
            string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

            int baseScore = 1000;
            // Thưởng thời gian (Đánh càng nhanh điểm càng cao, tối đa 5000 điểm)
            int timeBonus = Mathf.Max(0, 5000 - Mathf.RoundToInt(matchTime * 50));
            int healthBonus = 0;

            // Thưởng sinh tồn (Máu còn càng nhiều điểm càng cao, tối đa 5000 điểm)
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

            // Cập nhật lên UI
            if (timeText != null) timeText.text = "THỜI GIAN: " + timeString;
            if (scoreText != null) scoreText.text = "ĐIỂM TỔNG: " + totalScore.ToString();

            // Kiểm tra và lưu kỷ lục mới vào máy
            string stageScoreKey = "BestScore_Stage_" + currentStageIndex;
            string stageTimeKey = "BestTime_Stage_" + currentStageIndex;

            int bestScore = PlayerPrefs.GetInt(stageScoreKey, 0);
            float bestTime = PlayerPrefs.GetFloat(stageTimeKey, 9999f);

            bool isNewRecord = false;
            if (totalScore > bestScore)
            {
                PlayerPrefs.SetInt(stageScoreKey, totalScore);
                isNewRecord = true;
            }
            if (matchTime < bestTime)
            {
                PlayerPrefs.SetFloat(stageTimeKey, matchTime);
                isNewRecord = true;
            }

            // Hiển thị chữ chúc mừng Kỷ lục mới nếu có
            if (newRecordAlert != null) newRecordAlert.SetActive(isNewRecord);
            // ==========================================

            // --- 1. LƯU CHO NÚT CONTINUE ---
            PlayerPrefs.SetInt("Current_Stage_Index", currentStageIndex + 1);
            PlayerPrefs.SetInt("Has_Saved_Game", 1);

            // --- 2. LƯU ĐỂ MỞ Ổ KHÓA BẢN ĐỒ (MAP) ---
            int currentUnlocked = PlayerPrefs.GetInt("UnlockedStage", 1);
            if (currentStageIndex >= currentUnlocked)
            {
                PlayerPrefs.SetInt("UnlockedStage", currentStageIndex + 1);
                Debug.Log("Đã phá ổ khóa ải số: " + (currentStageIndex + 1));
            }

            PlayerPrefs.Save();
            Debug.Log($"Thắng! Thời gian: {timeString} | Điểm: {totalScore} | Kỷ lục mới: {isNewRecord}");
        }
        else
        {
            if (resultImage != null) resultImage.sprite = gameoverSprite;
            if (nextStageButton != null) nextStageButton.SetActive(false);

            // Nếu thua thì hiển thị trắng số liệu trên UI
            if (timeText != null) timeText.text = "--:--";
            if (scoreText != null) scoreText.text = "THẤT BẠI";
            if (newRecordAlert != null) newRecordAlert.SetActive(false);

            // --- LƯU TIẾN TRÌNH KHI THUA ---
            PlayerPrefs.SetInt("Current_Stage_Index", currentStageIndex);
            PlayerPrefs.SetInt("Has_Saved_Game", 1);
            PlayerPrefs.Save();
        }

        // 3. Đóng băng hoàn toàn mọi nhân vật trên sân khi kết thúc
        CharacterController2D[] allCharacters = FindObjectsByType<CharacterController2D>(FindObjectsSortMode.None);
        foreach (CharacterController2D character in allCharacters)
        {
            Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            character.StopAllCoroutines();
            character.enabled = false;

            Animator anim = character.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetFloat("Speed", 0); // Ép về dáng đứng im
                anim.SetBool("IsBlocking", false);
            }
        }
    }

    // --- CÁC HÀM GẮN CHO NÚT BẤM (ON CLICK) ---

    public void OnReplayClick()
    {
        // Chơi lại map hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnBackClick()
    {
        // Về màn hình Menu chính (Chỗ có nút Continue)
        SceneManager.LoadScene("SampleScene"); // Đảm bảo tên "SampleScene" đúng với Scene Menu của bạn
    }

    public void OnNextStageClick()
    {
        // Nhảy sang map tiếp theo trực tiếp từ bảng Victory
        if (!string.IsNullOrEmpty(nextStageName))
        {
            SceneManager.LoadScene(nextStageName);
        }
    }
}