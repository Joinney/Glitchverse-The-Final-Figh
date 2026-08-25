using UnityEngine;
using UnityEngine.EventSystems; // Bắt buộc phải có dòng này để dùng hiệu ứng chuột

// Kế thừa IPointerEnterHandler và IPointerExitHandler để bắt sự kiện chuột
public class MapButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hiệu Ứng Viền Sáng")]
    public GameObject glowFrame; // Kéo thả cái GlowFrame vào đây

    [Header("Hiệu Ứng Phóng To")]
    public float hoverScale = 1.05f; // Chỉnh độ bự khi rà chuột (1.05 = to lên 5%)

    private Vector3 originalScale;

    void Start()
    {
        // Lưu lại kích thước gốc của tấm ảnh
        originalScale = transform.localScale;

        // Vừa vào game là tự động tắt cái viền đi
        if (glowFrame != null)
        {
            glowFrame.SetActive(false);
        }
    }

    // Khi CHUỘT ĐI VÀO tấm ảnh
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (glowFrame != null) glowFrame.SetActive(true);  // Bật viền sáng
        transform.localScale = originalScale * hoverScale; // Phóng to tấm ảnh lên
    }

    // Khi CHUỘT ĐI RA KHỎI tấm ảnh
    public void OnPointerExit(PointerEventData eventData)
    {
        if (glowFrame != null) glowFrame.SetActive(false); // Tắt viền sáng
        transform.localScale = originalScale;              // Thu về kích thước cũ
    }
}