using UnityEngine;
using TMPro;
using System.Collections;

public class MatchTimer : MonoBehaviour
{
    [Header("Chữ hiển thị số giây")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isInfinity = false;
    private bool matchEnded = false;

    // Bộ nhớ đệm để theo dõi máu của các nhân vật trên sân
    private PlayerHealth[] players;
    private EnemyHealth[] enemies;

    void Start()
    {
        // ĐỌC "CUỐN SỔ TAY" TỪ BẢNG SETTINGS (Mặc định là 60s)
        int timeSetting = PlayerPrefs.GetInt("RoundTime", 60);

        if (timeSetting == -1)
        {
            isInfinity = true;
            if (timerText != null) timerText.text = "∞";
        }
        else
        {
            isInfinity = false;
            currentTime = timeSetting;
            if (timerText != null) timerText.text = currentTime.ToString();
        }
    }

    void Update()
    {
        // Chờ bảng đếm ngược 3-2-1 xong mới chạy
        if (CountdownManager.isCountdownFinished == false) return;

        if (isInfinity || matchEnded) return;

        // ==========================================
        // 💡 CÁI PHANH TAY: K.O LÀ DỪNG ĐỒNG HỒ NGAY!
        // ==========================================
        if (CheckMatchOverByKO())
        {
            matchEnded = true;
            return; // Cắt luồng tại đây, không cho trừ lùi thời gian nữa
        }

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            if (timerText != null) timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
        else
        {
            // HẾT GIỜ
            currentTime = 0;
            if (timerText != null) timerText.text = "0";
            TimeOut();
        }
    }

    // ==========================================
    // HÀM QUÉT MÁU TRÊN SÂN
    // ==========================================
    bool CheckMatchOverByKO()
    {
        // Tìm và ghi nhớ các nhân vật trên sân (Chỉ tìm 1 lần cho nhẹ máy)
        if (players == null || players.Length == 0)
            players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        if (enemies == null || enemies.Length == 0)
            enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        // Quét Player (Dùng cho cả PvP và P1)
        foreach (PlayerHealth p in players)
        {
            if (p != null && p.currentHealth <= 0) return true; // Có người ngỏm -> Bóp phanh!
        }

        // Quét Boss/AI (Dùng cho Singleplayer)
        foreach (EnemyHealth e in enemies)
        {
            if (e != null && e.currentHealth <= 0) return true;
        }

        return false;
    }

    void TimeOut()
    {
        matchEnded = true;
        Debug.Log("ĐÃ HẾT GIỜ! Time Over!");

        MatchController match = FindAnyObjectByType<MatchController>();
        if (match != null)
        {
            string mode = PlayerPrefs.GetString("GameMode", "Single");
            if (mode == "PvP")
            {
                // Nếu PvP mà hết giờ (Time Out): Tạm thời xử lý mặc định là P1 thắng hiệp đó 
                // (Sau này bạn có thể nâng cấp thêm chức năng so sánh xem ai máu nhiều hơn thì người đó thắng)
                match.EndPvPMatch(1);
            }
            else
            {
                // Chơi đơn hết giờ mà Boss chưa chết -> Thua
                match.EndMatch(false);
            }
        }
    }
}