using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject startMenuPanel;     // Màn hình Start ban đầu (ảnh nền chung + nút START)
    public GameObject modeMenuPanel;      // Màn hình Goku (chứa ảnh nền Goku)
    public GameObject difficultyPanel;    // Màn hình chọn độ khó (chữ SELECT + các nút độ khó)

    [Header("Sub Containers")]
    public GameObject gokuButtonsContainer; // Nhóm chứa 4 nút cũ (New Game, Continue, Play More, BACK)

    // 1. Gắn vào nút START ban đầu
    public void OpenModeMenu()
    {
        startMenuPanel.SetActive(false);
        modeMenuPanel.SetActive(true);
        gokuButtonsContainer.SetActive(true); // Đảm bảo nhóm nút cũ luôn hiện khi mở màn hình Goku
        difficultyPanel.SetActive(false);
    }

    // 2. Gắn vào nút BACK trên màn hình Goku (Quay về màn hình Start ban đầu)
    public void GoBackToStartMenu()
    {
        modeMenuPanel.SetActive(false);
        startMenuPanel.SetActive(true);
    }

    // 3. Gắn vào nút NEW GAME trên màn hình Goku
    public void OpenDifficultyMenu()
    {
        gokuButtonsContainer.SetActive(false); // Ẩn 4 nút cũ đi để không bị đè chữ
        difficultyPanel.SetActive(true);       // Hiện bảng chọn độ khó lên
    }

    // 4. Gắn vào nút BACK trên màn hình chọn Độ khó (Quay về màn hình Goku cũ)
    public void GoBackToModeMenu()
    {
        difficultyPanel.SetActive(false);      // Ẩn 3 nút độ khó đi
        gokuButtonsContainer.SetActive(true);  // Hiện lại 4 nút cũ cho người chơi chọn
    }

    // 5. Gắn vào nút NORMAL hoặc HARD để vào chơi game
    public void StartGameWithDifficulty(string difficulty)
    {
        // Lưu lại độ khó người chơi chọn vào máy (Normal hoặc Hard)
        PlayerPrefs.SetString("GameDifficulty", difficulty);
        
        // Chuyển sang màn chơi chính của bạn
        SceneManager.LoadScene("SampleScene"); 
    }
}