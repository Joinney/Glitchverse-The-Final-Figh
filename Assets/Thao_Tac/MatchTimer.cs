using UnityEngine;
using TMPro; // Thư viện dùng cho TextMeshPro
using System.Collections;

public class MatchTimer : MonoBehaviour
{
    [Header("Chữ hiển thị số giây")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isInfinity = false;
    private bool matchEnded = false;

    void Start()
    {
        // ĐỌC "CUỐN SỔ TAY" TỪ BẢNG SETTINGS (Mặc định là 60s nếu chưa ai chỉnh gì)
        int timeSetting = PlayerPrefs.GetInt("RoundTime", 60);

        // Kiểm tra xem có phải chế độ Vô cực (-1) không
        if (timeSetting == -1)
        {
            isInfinity = true;
            if (timerText != null) timerText.text = "∞"; // Hiện dấu vô cực
        }
        else
        {
            isInfinity = false;
            currentTime = timeSetting; // Nạp số giây vào đồng hồ
            if (timerText != null) timerText.text = currentTime.ToString();
        }
    }

    void Update()
    {
        // --- CHẶN Ở ĐÂY: Nếu bảng 3-2-1 chưa chạy xong thì ĐÓNG BĂNG ĐỒNG HỒ ---
        if (CountdownManager.isCountdownFinished == false) return;

        // Nếu là Vô cực hoặc Trận đấu đã kết thúc thì không đếm ngược nữa
        if (isInfinity || matchEnded) return;

        if (currentTime > 0)
        {
            // Trừ lùi thời gian theo từng frame
            currentTime -= Time.deltaTime;

            // Làm tròn số lên (VD: 59.8 -> 60) để hiện lên UI cho đẹp
            if (timerText != null) timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
        else
        {
            // --- HẾT GIỜ ---
            currentTime = 0;
            if (timerText != null) timerText.text = "0";
            TimeOut();
        }
    }

    void TimeOut()
    {
        matchEnded = true;
        Debug.Log("ĐÃ HẾT GIỜ! Time Over!");

        // Gọi ông Trọng Tài (MatchController) ra để tuýt còi kết thúc trận
        MatchController match = FindAnyObjectByType<MatchController>();
        if (match != null)
        {
            // Tạm thời quy ước: Hết giờ mà chưa giết được Boss -> Người chơi Thua (false)
            match.EndMatch(false);
        }
    }
}