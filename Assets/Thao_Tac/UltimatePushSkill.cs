using UnityEngine;

public class UltimatePushSkill : MonoBehaviour
{
    [Header("Thông số Chiêu Cuối")]
    public int damage = 80;
    public float speed = 15f; 
    public float lifeTime = 3f;

    [Header("Hiệu ứng Hất Tung & Choáng")]
    public Vector2 knockbackForce = new Vector2(0f, 15f); 
    public float stunDuration = 1.2f; 

    [Header("Cài đặt Tường chặn")]
    public string wallTag = "Wall"; // Tên Tag của bức tường hoặc mặt đất

    [Header("Cài đặt Âm Thanh")]
    public AudioClip castSound; 
    public AudioClip hitSound;  

    private Rigidbody2D rb;
    private bool hasPlayedHitSound = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (castSound != null)
        {
            AudioSource.PlayClipAtPoint(castSound, transform.position);
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Liên tục cập nhật vận tốc. Khi đụng tường, speed = 0 thì đạn sẽ tự đứng im
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    // 💥 BƯỚC 1: Xử lý va chạm
    void OnTriggerEnter2D(Collider2D other)
    {
        // 🛑 NẾU CHẠM VÀO TƯỜNG (Ranh giới map): Dừng lại ngay lập tức!
        if (other.CompareTag(wallTag))
        {
            speed = 0f; // Triệt tiêu tốc độ bay của đạn
            return;     // Dừng đọc code tại đây (Không trừ máu bức tường)
        }

        // Bỏ qua phe mình
        if (other.CompareTag(gameObject.tag)) return;

        bool hitSomeone = false;

        CharacterController2D target = other.GetComponent<CharacterController2D>();
        if (target != null)
        {
            target.TakeKnockback(new Vector2(0, knockbackForce.y), stunDuration);
            hitSomeone = true;
        }

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null) { enemy.TakeDamage(damage); hitSomeone = true; }

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null) { player.TakeDamage(damage); hitSomeone = true; }

        if (hitSomeone && hitSound != null && !hasPlayedHitSound)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
            hasPlayedHitSound = true; 
        }
    }

    // 🌪️ BƯỚC 2: Cuốn địch đi theo đạn (Hoặc ghim vào tường)
    void OnTriggerStay2D(Collider2D other)
    {
        // Nếu đụng tường thì bỏ qua, không kéo tường đi
        if (other.CompareTag(wallTag)) return; 

        if (other.CompareTag(gameObject.tag)) return;

        Rigidbody2D enemyRb = other.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            // Ép vận tốc kẻ địch. Nếu đạn đã đụng tường (speed = 0), địch cũng sẽ bị ép vận tốc về 0 và dính chặt vào tường!
            enemyRb.linearVelocity = new Vector2(speed, enemyRb.linearVelocity.y);
        }
    }
}