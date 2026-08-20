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

    // ==========================================
    // 💡 GIỮ LẠI CAST SOUND VÀ CHỈNH TO HẾT CỠ
    // ==========================================
    [Header("Cài đặt Âm Thanh")]
    public AudioClip castSound;
    [Range(0f, 1f)] public float castVolume = 1f; // Tiếng thét/gầm tung chiêu

    public AudioClip hitSound;
    [Range(0f, 1f)] public float hitVolume = 1f;  // Tiếng nổ ầm ĩ khi chạm mục tiêu / chạm tường

    private Rigidbody2D rb;
    private bool hasPlayedHitSound = false;
    private bool hasTriggeredGameFeel = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 1. NGAY KHI CHIÊU XUẤT HIỆN LÀ PHÁT TIẾNG CAST SOUND LẬP TỨC (Dù trúng hay trượt)
        if (castSound != null)
        {
            Vector3 camPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(castSound, camPos, castVolume);
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Liên tục ép vận tốc
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    // 💥 BƯỚC 1: Xử lý va chạm
    void OnTriggerEnter2D(Collider2D other)
    {
        // Bỏ qua phe mình
        if (other.CompareTag(gameObject.tag)) return;

        // 🛑 KIỂM TRA XEM CÓ ĐỤNG TƯỜNG KHÔNG?
        bool isWall = other.CompareTag(wallTag);
        if (isWall)
        {
            speed = 0f; // Triệt tiêu tốc độ bay để đạn dừng lại cắm vào tường
        }

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

        // ==========================================
        // 💡 TRÚNG ĐỊCH HOẶC TRÚNG TƯỜNG ĐỀU SẼ KÍCH NỔ HIT SOUND
        // ==========================================
        if (hitSomeone || isWall)
        {
            // Phát âm thanh nổ ngay tại Camera
            if (hitSound != null && !hasPlayedHitSound)
            {
                Vector3 camPos = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(hitSound, camPos, hitVolume);
                hasPlayedHitSound = true;
            }

            // Kích hoạt hiệu ứng khựng và rung màn hình bạo lực
            if (!hasTriggeredGameFeel && GameFeelManager.instance != null)
            {
                GameFeelManager.instance.TriggerHitStop(0.1f);
                GameFeelManager.instance.TriggerCameraShake(0.3f, 0.4f);
                hasTriggeredGameFeel = true;
            }
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
            enemyRb.linearVelocity = new Vector2(speed, enemyRb.linearVelocity.y);
        }
    }
}