using UnityEngine;
using TMPro;
using System.Collections;

public class CoinHUDManager : MonoBehaviour
{
    public static CoinHUDManager instance;

    [Header("UI Hiển Thị")]
    public TextMeshProUGUI totalCoinText;   // Text hiển thị tổng xu (vd: 150)
    public TextMeshProUGUI plusCoinPopupText; // Text hiệu ứng bay lên (vd: +15)

    private Coroutine popupCoroutine;
    private Vector3 originalPopupPos;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (plusCoinPopupText != null)
        {
            originalPopupPos = plusCoinPopupText.transform.localPosition;
            plusCoinPopupText.gameObject.SetActive(false);
        }

        UpdateCoinDisplay();
    }

    public void UpdateCoinDisplay()
    {
        int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        if (totalCoinText != null)
        {
            totalCoinText.text = currentCoins.ToString("N0");
        }
    }

    // Hiệu ứng nảy số và bay dòng chữ "+X" màu vàng kim
    public void TriggerAddCoinEffect(int amount)
    {
        UpdateCoinDisplay();

        if (plusCoinPopupText != null)
        {
            if (popupCoroutine != null) StopCoroutine(popupCoroutine);
            popupCoroutine = StartCoroutine(ShowPlusCoinRoutine(amount));
        }
    }

    // Ẩn toàn bộ cụm hiển thị xu
    public void SetHUDActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    private IEnumerator ShowPlusCoinRoutine(int amount)
    {
        plusCoinPopupText.gameObject.SetActive(true);
        plusCoinPopupText.text = "+" + amount.ToString();
        plusCoinPopupText.transform.localPosition = originalPopupPos;
        plusCoinPopupText.alpha = 1f;

        // Phóng to nhẹ chữ tổng xu tạo cảm giác nhận lực
        if (totalCoinText != null)
        {
            totalCoinText.transform.localScale = Vector3.one * 1.25f;
        }

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Bay lên nhẹ nhàng
            plusCoinPopupText.transform.localPosition = originalPopupPos + new Vector3(0, t * 35f, 0);

            // Mờ dần về sau
            plusCoinPopupText.alpha = Mathf.Lerp(1f, 0f, t);

            // Thu nhỏ lại kích thước chữ tổng tiền
            if (totalCoinText != null)
            {
                totalCoinText.transform.localScale = Vector3.Lerp(Vector3.one * 1.25f, Vector3.one, t);
            }

            yield return null;
        }

        plusCoinPopupText.gameObject.SetActive(false);
    }
}