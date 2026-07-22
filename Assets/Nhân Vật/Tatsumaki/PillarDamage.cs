using UnityEngine;

public class PillarDamage : MonoBehaviour
{
    [Header("Thông số Sát Thương")]
    public int damage = 40;            // Sát thương khi cột đâm trúng
    public float lifeTime = 1.2f;      // Cột tồn tại 1.2 giây rồi tự biến mất

    void Start()
    {
        // Vừa xuất hiện là đếm ngược thời gian để tự hủy, tránh lỗi lặp vô tận
        Destroy(gameObject, lifeTime);
    }

    // Khi cột đâm trúng ai đó
    void OnTriggerEnter2D(Collider2D other)
    {
        // Tránh tự đâm trúng phe mình (người triệu hồi)
        if (other.CompareTag(gameObject.tag)) return;

        // --- BƯỚC 1: KIỂM TRA TRẠNG THÁI ĐỠ ĐÒN ---
        CharacterController2D targetController = other.GetComponent<CharacterController2D>();

        int finalDamage = damage; // Gán sát thương mặc định ban đầu bằng sát thương gốc

        // Nếu mục tiêu có tồn tại và đang giữ nút đỡ đòn
        if (targetController != null && targetController.isBlocking)
        {
            // SKILL 3: Giảm 50% sát thương
            finalDamage = finalDamage / 2;
        }

        // --- BƯỚC 2: TRỪ MÁU (Sử dụng finalDamage thay vì damage gốc) ---

        // Nếu đâm trúng AI kẻ địch
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(finalDamage);
        }

        // Nếu đâm trúng Player đối thủ
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(finalDamage);
        }
    }
}