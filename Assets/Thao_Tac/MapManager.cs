using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // <--- BẮT BUỘC PHẢI CÓ DÒNG NÀY

public class MapManager : MonoBehaviour
{
    [Header("Danh sách Nút Ải (Từ 1 đến Boss)")]
    public Button[] stageButtons;

    [Header("Danh sách Ổ Khóa tương ứng (Để trống ô số 1 nếu ải 1 không có khóa)")]
    public GameObject[] lockIcons;

    void Start()
    {
        // Lấy dữ liệu ải đã mở từ sổ tay (Mặc định là 1)
        int unlockedStage = PlayerPrefs.GetInt("UnlockedStage", 1);

        for (int i = 0; i < stageButtons.Length; i++)
        {
            // --- XỬ LÝ ẢI ĐÃ MỞ ---
            if (i + 1 <= unlockedStage)
            {
                stageButtons[i].interactable = true; // Cho phép bấm

                // Nếu có gắn ổ khóa thì giấu nó đi
                if (lockIcons.Length > i && lockIcons[i] != null)
                {
                    lockIcons[i].SetActive(false);
                }
            }
            // --- XỬ LÝ ẢI CHƯA MỞ ---
            else
            {
                stageButtons[i].interactable = false; // Khóa bấm

                // Bật hiện cái ổ khóa / mảng đen che lại
                if (lockIcons.Length > i && lockIcons[i] != null)
                {
                    lockIcons[i].SetActive(true);
                }
            }
        }
    }

    // Hàm xử lý khi bấm nút BACK quay về Menu chính
    public void BackToMainMenu()
    {
        // 1. Để lại lời nhắn vào sổ tay trước khi chuyển cảnh
        PlayerPrefs.SetInt("BackToCharSelect", 1);

        // 2. Chuyển về Scene MainMenu
        SceneManager.LoadScene("MainMenu");
    }
}