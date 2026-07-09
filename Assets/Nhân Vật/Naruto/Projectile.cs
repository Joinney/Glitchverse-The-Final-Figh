using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] 
    public float speed = 12f; 
    public float lifeTime = 3f;
    public int damage = 10;
    
    [Header("Thời gian chờ xóa đạn (khớp với hoạt ảnh Nổ)")]
    public float hitAnimationDuration = 0.5f;

    [Header("Hệ Thống Âm Thanh")]
    private AudioSource audioSource;
    public AudioClip castSound; // Tiếng khi đạn phóng ra (ví dụ: tiếng gồng, sấm sét)
    public AudioClip hitSound;  // Tiếng khi đạn nổ trúng đích (ví dụ: tiếng bùm, kiếm chém)

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Đẩy lực thẳng theo trục X
            rb.linearVelocity = new Vector2(speed, 0f);
        }

        // LẤY LOA VÀ PHÁT ÂM THANH XẢ CHIÊU:
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && castSound != null)
        {
            audioSource.PlayOneShot(castSound);
        }
        
        // Tự hủy sau một khoảng thời gian (nếu bay trượt ra ngoài màn hình)
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chạm phe đối địch thì tính sát thương và tự hủy
        if ((gameObject.CompareTag("Player") && other.CompareTag("Enemy")) ||
            (gameObject.CompareTag("Enemy") && other.CompareTag("Player")))
        {
            Debug.Log($"[TRÚNG CHIÊU] Chiêu thức bắn trúng {other.name}!");
            other.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            // PHÁT ÂM THANH NỔ/TRÚNG ĐÒN:
            if (audioSource != null && hitSound != null)
            {
                // Dùng PlayOneShot để âm thanh nổ không bị ngắt giữa chừng khi đạn bị Destroy
                audioSource.PlayOneShot(hitSound);
            }

            // 1. Phanh gấp! Ép vận tốc viên đạn về 0 để đứng im tại vị trí trúng
           
            Rigidbody2D realRb = GetComponent<Rigidbody2D>();
            if (realRb != null) 
            {
                realRb.linearVelocity = Vector2.zero;
            }

            // 2. Tắt vòng Collider để không trừ máu địch thêm lần nào nữa (chống bug double-hit)
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) 
            {
                col.enabled = false;
            }

            // 3. Ra lệnh cho Animator chuyển sang hoạt ảnh Nổ/Trúng đòn
            Animator anim = GetComponent<Animator>();
            if (anim != null) 
            {
                anim.SetTrigger("Hit");
            }

            // 4. Ghi đè lệnh hủy cũ, hẹn giờ xóa viên đạn ngay sau khi diễn xong hoạt ảnh nổ
            Destroy(gameObject, hitAnimationDuration); 
        }
    }
}