using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // BẮT BUỘC THÊM ĐỂ NHẬN CHỮ TMP

public class PauseManager : MonoBehaviour
{
    [Header("Các Bảng Giao Diện")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Cài đặt Âm thanh")]
    public Slider volumeSlider;
    public TMP_Text volumeText; // Ô chứa chữ số % (Thêm mới)

    private bool isPaused = false;

    void Start()
    {
        // 1. Cập nhật Âm thanh
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
            volumeSlider.value = savedVolume; // Kéo thanh trượt về đúng chỗ
            AudioListener.volume = savedVolume;
            UpdateVolumeText(savedVolume); // Đổi số %
        }

        // 2. Cập nhật Đồ họa
        int savedQuality = PlayerPrefs.GetInt("GameQuality", QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(savedQuality);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    // ===================================
    // KHI KÉO THANH VOLUME NÓ SẼ CHẠY HÀM NÀY
    // ===================================
    public void SetVolume(float volumeValue)
    {
        AudioListener.volume = volumeValue;
        PlayerPrefs.SetFloat("GameVolume", volumeValue);
        UpdateVolumeText(volumeValue); // Kéo tới đâu số đổi tới đó
    }

    // HÀM HIỂN THỊ SỐ %
    private void UpdateVolumeText(float vol)
    {
        if (volumeText != null)
        {
            // Biến số từ 0.0 -> 1.0 thành 0 -> 100
            int percentage = Mathf.RoundToInt(vol * 100);
            volumeText.text = percentage.ToString() + "%";
        }
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("GameQuality", qualityIndex);
    }
}