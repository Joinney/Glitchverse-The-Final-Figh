using UnityEngine;
using UnityEngine.UI;
using TMPro; // Thư viện dùng cho TextMeshPro

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
        // Khi vừa mở bảng cài đặt, load lại các thông số đã lưu trước đó (nếu có)
        LoadSettings();
    }

    // 1. Hàm xử lý Âm lượng (Được gọi mỗi khi kéo thanh trượt)
    public void OnVolumeChanged(float value)
    {
        // Đổi giá trị 0.0 -> 1.0 thành 0% -> 100%
        volumeText.text = Mathf.RoundToInt(value * 100) + "%";

        // Chỉnh âm lượng tổng của cả game
        AudioListener.volume = value;

        // Lưu lại để lần sau mở game vẫn giữ mức âm lượng này
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    // 2. Hàm xử lý Máu (Được gọi khi bấm Save hoặc khi đổi Dropdown)
    public void OnHealthChanged()
    {
        float multiplier = 1f; // Mặc định 100%
        switch (healthDropdown.value)
        {
            case 0: multiplier = 1f; break; // 100%
            case 1: multiplier = 2f; break; // 200%
            case 2: multiplier = 4f; break; // 400%
            case 3: multiplier = 5f; break; // 500%
        }
        PlayerPrefs.SetFloat("HealthMultiplier", multiplier); // Lưu lại tỉ lệ máu
    }

    // 3. Hàm xử lý Đồ họa
    public void OnGraphicsChanged()
    {
        // Unity sẽ tự động đổi đồ họa dựa trên số index (0: Low, 1: Med, 2: High)
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
        PlayerPrefs.SetInt("GraphicsQuality", graphicsDropdown.value);
    }

    // 4. Hàm xử lý Thời gian
    public void OnTimeChanged()
    {
        int timeLimit = 60;
        switch (timeDropdown.value)
        {
            case 0: timeLimit = 60; break;
            case 1: timeLimit = 120; break;
            case 2: timeLimit = 200; break;
            case 3: timeLimit = -1; break; // Dùng số âm (-1) để tượng trưng cho vô cực
        }
        PlayerPrefs.SetInt("RoundTime", timeLimit);
    }

    // Hàm phụ trợ: Tải lại dữ liệu cũ khi bật game
    private void LoadSettings()
    {
        // Load Volume
        float savedVol = PlayerPrefs.GetFloat("MasterVolume", 1f); // Mặc định là 1 (100%)
        volumeSlider.value = savedVol;
        OnVolumeChanged(savedVol); // Cập nhật luôn chữ hiển thị

        // Load Graphics
        graphicsDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", 1); // Mặc định Medium

        // Chú ý: Với Health và Time, ta cần lưu/load index (0, 1, 2, 3) thay vì lưu giá trị thực
        // Bạn có thể tùy chỉnh thêm phần load này cho Health và Time nếu cần thiết
    }
}