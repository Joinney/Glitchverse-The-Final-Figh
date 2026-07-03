using UnityEngine;
using UnityEngine.SceneManagement;

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

    // 1. Gắn vào nút START ban đầu
    public void OpenModeMenu()
    {
        startMenuPanel.SetActive(false);
        modeMenuPanel.SetActive(true);
        gokuButtonsContainer.SetActive(true); 
        difficultyPanel.SetActive(false);
    }

    // 2. Gắn vào nút BACK trên màn hình Goku (Quay về màn hình Start ban đầu)
    public void GoBackToStartMenu()
    {
        modeMenuPanel.SetActive(false);
        startMenuPanel.SetActive(true);
    }

    // 3. Gắn vào nút NEW GAME trên màn hình Goku (Chơi 1 mình vs AI)
    public void OpenDifficultyMenu()
    {
        PlayerPrefs.SetInt("GameMode", 1);
        Debug.Log("Chế độ: CHƠI 1 NGƯỜI -> Mở chọn độ khó.");

        gokuButtonsContainer.SetActive(false); 
        difficultyPanel.SetActive(true);       
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

    // 4. Gắn vào nút BACK trên màn hình chọn Độ khó (Quay về màn hình Goku cũ)
    public void GoBackToModeMenu()
    {
        difficultyPanel.SetActive(false);      
        gokuButtonsContainer.SetActive(true);  
    }

    // 5. Gắn vào nút NORMAL hoặc HARD để chuyển sang giao diện chơi đơn mới
    public void StartGameWithDifficulty(string difficulty)
    {
        PlayerPrefs.SetString("GameDifficulty", difficulty);
        Debug.Log("Đã chọn độ khó: " + difficulty + " -> Chuyển sang màn hình chơi đơn.");
        
        // ẨN CÁC PANEL CŨ ĐI
        difficultyPanel.SetActive(false);        
        modeMenuPanel.SetActive(false);          
        characterSelectPanel.SetActive(false); // Đảm bảo ẩn màn chọn nhân vật PVP đi
        
        // HIỆN MÀN HÌNH CHƠI ĐƠN MỚI LÊN!
        if (singlePlayerPanel != null)
        {
            singlePlayerPanel.SetActive(true); 
        }
    }
}