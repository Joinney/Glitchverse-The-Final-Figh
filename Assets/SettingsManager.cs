using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;
    public TMP_Dropdown healthDropdown;
    public TMP_Dropdown graphicsDropdown;
    public TMP_Dropdown timeDropdown;

    private void Start()
    {
        // Khi vừa mở bảng cài đặt, load lại các thông số đã lưu
        LoadSettings();
    }

    public void OnVolumeChanged(float value)
    {
        volumeText.text = Mathf.RoundToInt(value * 100) + "%";
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void OnHealthChanged()
    {
        float multiplier = 1f;
        switch (healthDropdown.value)
        {
            case 0: multiplier = 1f; break; // 100% (x1)
            case 1: multiplier = 2f; break; // 200% (x2)
            case 2: multiplier = 4f; break; // 400% (x4)
            case 3: multiplier = 5f; break; // 500% (x5)
        }
        PlayerPrefs.SetFloat("HealthMultiplier", multiplier);
        // LƯU THÊM INDEX ĐỂ GIAO DIỆN KHÔNG BỊ LỖI
        PlayerPrefs.SetInt("HealthUI_Index", healthDropdown.value);
    }

    public void OnGraphicsChanged()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
        PlayerPrefs.SetInt("GraphicsQuality", graphicsDropdown.value);
    }

    public void OnTimeChanged()
    {
        int timeLimit = 60;
        switch (timeDropdown.value)
        {
            case 0: timeLimit = 60; break;
            case 1: timeLimit = 120; break;
            case 2: timeLimit = 240; break;
            case 3: timeLimit = -1; break; // Vô cực
        }
        PlayerPrefs.SetInt("RoundTime", timeLimit);
        // LƯU THÊM INDEX ĐỂ GIAO DIỆN KHÔNG BỊ LỖI LỘN XỘN 60s -> VÔ CỰC
        PlayerPrefs.SetInt("TimeUI_Index", timeDropdown.value);
    }

    private void LoadSettings()
    {
        // Load Volume
        float savedVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider.value = savedVol;
        OnVolumeChanged(savedVol);

        // Load Graphics
        graphicsDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", 1);

        // TẢI LẠI ĐÚNG VỊ TRÍ DROPDOWN CỦA MÁU VÀ THỜI GIAN
        // Bắt buộc phải bỏ code gán sự kiện (RemoveListener) tạm thời để tránh Unity gọi hàm OnChange 2 lần lúc khởi động
        healthDropdown.onValueChanged.RemoveAllListeners();
        healthDropdown.value = PlayerPrefs.GetInt("HealthUI_Index", 0); // Mặc định là 0 (100%)
        healthDropdown.onValueChanged.AddListener(delegate { OnHealthChanged(); });

        timeDropdown.onValueChanged.RemoveAllListeners();
        timeDropdown.value = PlayerPrefs.GetInt("TimeUI_Index", 0); // Mặc định là 0 (60s)
        timeDropdown.onValueChanged.AddListener(delegate { OnTimeChanged(); });
    }
}