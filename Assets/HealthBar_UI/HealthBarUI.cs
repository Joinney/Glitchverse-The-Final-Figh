using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Các Lớp Thanh Máu")]
    public Image mainFill; // Kéo Main_Fill (Xanh/Đỏ) vào đây
    public Image easeFill; // Kéo Ease_Fill (Vàng/Trắng) vào đây

    [Header("Avatar Nhân Vật")]
    public Image avatarPic;

    [Header("Tên Nhân Vật")]
    public TextMeshProUGUI nameText;

    [Header("Tốc Độ Tụt Máu Đệm")]
    public float easeSpeed = 5f; // Đã tăng lên 5f để hiệu ứng mượt và bám sát hơn

    void Update()
    {
        // Hiệu ứng "Rớt máu": Nếu thanh vàng đang dài hơn thanh đỏ -> từ từ thu nhỏ nó lại cho bằng
        if (easeFill != null && mainFill != null)
        {
            if (easeFill.fillAmount > mainFill.fillAmount)
            {
                easeFill.fillAmount = Mathf.Lerp(easeFill.fillAmount, mainFill.fillAmount, easeSpeed * Time.deltaTime);

                // --- CODE TỐI ƯU HÓA ---
                // Nếu khoảng cách giữa 2 thanh đã cực kỳ nhỏ (mắt người không thấy được nữa)
                // thì ép nó bằng nhau luôn để giải phóng bộ nhớ, ngừng tính toán.
                if (Mathf.Abs(easeFill.fillAmount - mainFill.fillAmount) < 0.005f)
                {
                    easeFill.fillAmount = mainFill.fillAmount;
                }
            }
        }
    }

    // Hàm này sẽ được gọi mỗi khi bị ăn đấm
    public void SetHealth(int currentHealth, int maxHealth)
    {
        // Tính toán số % máu còn lại (từ 0.0 đến 1.0)
        float healthPercentage = (float)currentHealth / maxHealth;

        // Giật thanh máu chính tụt ngay lập tức
        if (mainFill != null)
        {
            mainFill.fillAmount = healthPercentage;
        }
    }

    public void SetAvatar(Sprite characterFace)
    {
        if (avatarPic != null && characterFace != null)
        {
            avatarPic.sprite = characterFace;
        }
    }

    public void SetCharacterName(string name)
    {
        if (nameText != null)
        {
            nameText.text = name;
        }
    }
}