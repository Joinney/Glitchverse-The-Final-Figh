using UnityEngine;
using UnityEngine.UI; // Bắt buộc phải có dòng này để code nhận diện được UI Image
using TMPro;
public class HealthBarUI : MonoBehaviour
{
    [Header("Các Lớp Thanh Máu")]
    public Image mainFill; // Kéo Main_Fill (Đỏ/Xanh) vào đây
    public Image easeFill; // Kéo Ease_Fill (Vàng) vào đây
    [Header("Avatar Nhân Vật")]
    public Image avatarPic;
    [Header("Tên Nhân Vật")]
    public TextMeshProUGUI nameText;

    [Header("Tốc Độ Tụt Máu Đệm")]
    public float easeSpeed = 2f; // Chỉnh số này để vệt màu vàng rớt nhanh hay chậm

    void Update()
    {
        // Hiệu ứng "Rớt máu": Nếu thanh vàng đang dài hơn thanh đỏ -> từ từ thu nhỏ nó lại cho bằng thanh đỏ
        if (easeFill != null && mainFill != null)
        {
            if (easeFill.fillAmount > mainFill.fillAmount)
            {
                easeFill.fillAmount = Mathf.Lerp(easeFill.fillAmount, mainFill.fillAmount, easeSpeed * Time.deltaTime);
            }
        }
    }

    // Hàm này sẽ được Naruto/Zenitsu gọi mỗi khi bị ăn đấm
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