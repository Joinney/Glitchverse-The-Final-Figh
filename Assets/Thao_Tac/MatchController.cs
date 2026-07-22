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

    [Header("Số thứ tự của Ải này (1 đến 5)")]
    public int currentStageIndex = 1;

    private bool matchEnded = false;

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
            Debug.Log("Thắng! Đã lưu tiến trình. Lần tới Continue sẽ vào Ải: " + (currentStageIndex + 1));
        }
        else
        {
            if (resultImage != null) resultImage.sprite = gameoverSprite;
            if (nextStageButton != null) nextStageButton.SetActive(false);

            // --- LƯU TIẾN TRÌNH KHI THUA ---
            PlayerPrefs.SetInt("Current_Stage_Index", currentStageIndex);
            PlayerPrefs.SetInt("Has_Saved_Game", 1);
            PlayerPrefs.Save();
        }

        // 3. Đóng băng hoàn toàn mọi nhân vật trên sân khi chiến thắng
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