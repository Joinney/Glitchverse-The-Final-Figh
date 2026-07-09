using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("Thông số đòn đánh")]
    public int damage = 35; // Mức sát thương đấm thường chuẩn Baryon Mode
    public string targetTag = "Enemy"; // Mục tiêu muốn đấm (Để "Enemy" nếu là Naruto đánh, để "Player" nếu AI đánh)

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đối tượng va chạm có đúng là phe địch không
        if (other.CompareTag(targetTag))
        {
            // Trừ máu AI (Nếu người chơi đánh)
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"👊 BỐP! Đấm trúng {other.name} - Trừ {damage} máu!");
                
                // QUAN TRỌNG: Tắt Hitbox ngay lập tức để không bị giật (bug) trừ máu 2-3 lần trong 1 cái vung tay
                GetComponent<Collider2D>().enabled = false; 
            }

            // Trừ máu Người chơi (Nếu gắn cho Hitbox của AI)
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"👊 BỐP! Đấm trúng {other.name} - Trừ {damage} máu!");
                
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
}