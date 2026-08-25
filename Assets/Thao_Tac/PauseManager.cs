using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseManager : MonoBehaviour
{
    [Header("Các Bảng Giao Diện")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public Button pauseButton; // ⚙️ Nút icon cài đặt / tạm dừng trên màn hình

    [Header("Cài đặt Âm thanh")]
    public Slider volumeSlider;
    public TMP_Text volumeText;

    private bool isPaused = false;

    void Start()
    {
        // Gán sự kiện bấm nút Pause trên màn hình cho điện thoại
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseGame);
        }

        // 1. Cập nhật Âm thanh
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
            UpdateVolumeText(savedVolume);
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
        if (pauseButton != null) pauseButton.gameObject.SetActive(false); // Ẩn nút pause khi đang mở bảng
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pauseButton != null) pauseButton.gameObject.SetActive(true); // Hiện lại nút pause khi tiếp tục chơi
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

    public void SetVolume(float volumeValue)
    {
        AudioListener.volume = volumeValue;
        PlayerPrefs.SetFloat("GameVolume", volumeValue);
        UpdateVolumeText(volumeValue);
    }

    private void UpdateVolumeText(float vol)
    {
        if (volumeText != null)
        {
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