using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Số thứ tự của Ải này (1 đến 5)")] // <--- MỚI THÊM 
    public int currentStageIndex = 1;

    private bool matchEnded = false;

    // HÀM XỬ LÝ KẾT THÚC TRẬN ĐẤU
    public void EndMatch(bool isPlayerWin)
    {
        if (matchEnded) return;
        matchEnded = true;

        // 1. Hiện bảng kết quả lên
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // 2. Tráo ảnh và Quản lý nút bấm
        if (isPlayerWin)
        {
            if (resultImage != null) resultImage.sprite = victorySprite;
            if (nextStageButton != null) nextStageButton.SetActive(true); // Thắng -> Hiện nút Play

            // --- ĐOẠN CODE MỚI THÊM: LƯU TIẾN TRÌNH ---
            // Đọc xem hiện tại người chơi đang mở tới ải mấy (mặc định là 1)
            int currentUnlocked = PlayerPrefs.GetInt("UnlockedStage", 1);

            // Nếu ải vừa thắng là ải cao nhất người chơi từng tới -> Mở khóa ải tiếp theo!
            if (currentStageIndex >= currentUnlocked)
            {
                PlayerPrefs.SetInt("UnlockedStage", currentStageIndex + 1);
                Debug.Log("Đã mở khóa ải số: " + (currentStageIndex + 1));
            }
            // ------------------------------------------
        }
        else
        {
            if (resultImage != null) resultImage.sprite = gameoverSprite;
            if (nextStageButton != null) nextStageButton.SetActive(false); // Thua -> Giấu nút Play đi
        }

        // 3. Đóng băng mọi nhân vật trên sân
        CharacterController2D[] allCharacters = FindObjectsByType<CharacterController2D>(FindObjectsSortMode.None);
        foreach (CharacterController2D character in allCharacters)
        {
            character.enabled = false; // Tắt di chuyển
            Animator anim = character.GetComponent<Animator>();
            if (anim != null) anim.SetFloat("Speed", 0); // Ép về dáng đứng im
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
        // Về màn hình Bản đồ
        SceneManager.LoadScene("SampleScene");
    }

    public void OnNextStageClick()
    {
        // Nhảy sang map tiếp theo
        if (!string.IsNullOrEmpty(nextStageName))
        {
            SceneManager.LoadScene(nextStageName);
        }
    }
}