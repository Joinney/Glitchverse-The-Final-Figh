using UnityEngine;
using TMPro;
using System.Collections;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;

    [Header("Giao diện Combo")]
    public TextMeshProUGUI comboText;

    [Header("Cài đặt thời gian")]
    public float comboTimeout = 1.5f; // Nếu 1.5 giây mà không đánh bồi thêm thì rớt combo

    private int currentCombo = 0;
    private float timer = 0f;
    private Coroutine popCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ẩn chữ đi khi trận đấu mới bắt đầu
        if (comboText != null) comboText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Đếm ngược thời gian rớt combo
        if (currentCombo > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                ResetCombo();
            }
        }
    }

    // Hàm này sẽ được gọi mỗi khi quái bị ăn đấm
    public void AddCombo()
    {
        currentCombo++;
        timer = comboTimeout; // Reset lại đồng hồ đếm ngược

        if (comboText != null)
        {
            comboText.gameObject.SetActive(true);

            // --- Hiệu ứng đổi màu ngầu hơn theo số Hit ---
            if (currentCombo < 5) comboText.color = Color.white;
            else if (currentCombo < 10) comboText.color = Color.yellow;
            else comboText.color = Color.red;

            // Cập nhật chữ
            comboText.text = currentCombo + " HITS!";

            // --- Hiệu ứng giật chữ nảy lên ---
            if (popCoroutine != null) StopCoroutine(popCoroutine);
            popCoroutine = StartCoroutine(PopTextEffect());
        }
    }

    private void ResetCombo()
    {
        currentCombo = 0;
        if (comboText != null) comboText.gameObject.SetActive(false);
    }

    // Hoạt ảnh nảy to chữ rồi thu nhỏ lại
    private IEnumerator PopTextEffect()
    {
        comboText.transform.localScale = new Vector3(1.5f, 1.5f, 1f); // Giật bự lên 150%
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Thu nhỏ từ từ về kích thước 100%
            comboText.transform.localScale = Vector3.Lerp(new Vector3(1.5f, 1.5f, 1f), Vector3.one, t);
            yield return null;
        }
        comboText.transform.localScale = Vector3.one;
    }
}