using UnityEngine;

public class PillarDamage : MonoBehaviour
{
    [Header("Thông số Sát Thương")]
    public int damage = 40;
    public float lifeTime = 1.2f;

    // ==========================================
    // 💡 LOGIC ÂM THANH CHUẨN Ý BẠN
    // ==========================================
    [Header("1. Âm thanh Xuất Chiêu (Phát ngay lập tức)")]
    [Tooltip("Giọng nói/Tiếng hô chiêu của nhân vật")]
    public AudioClip castSound;
    [Range(0f, 1f)] public float castVolume = 1f;

    [Tooltip("Tiếng đất nứt, đá mọc lên ầm ầm")]
    public AudioClip hitSound;
    [Range(0f, 1f)] public float hitVolume = 1f;

    [Header("2. Âm thanh Va Chạm (Chỉ phát khi trúng)")]
    [Tooltip("Tiếng phập khi đâm ngập vào da thịt mục tiêu (Tùy chọn)")]
    public AudioClip impactSound;
    [Range(0f, 1f)] public float impactVolume = 1f;

    [Tooltip("Tiếng Keng khi bị đối thủ đỡ đòn")]
    public AudioClip blockSound;
    [Range(0f, 1f)] public float blockVolume = 1f;

    private bool hasPlayedImpactSound = false;
    private bool hasTriggeredHitStop = false;

    void Start()
    {
        Vector3 camPos = Camera.main != null ? Camera.main.transform.position : transform.position;

        // 1. PHÁT GIỌNG NÓI NHÂN VẬT LẬP TỨC
        if (castSound != null) AudioSource.PlayClipAtPoint(castSound, camPos, castVolume);

        // 2. PHÁT TIẾNG MỌC CỘT LẬP TỨC (Không cần trúng ai)
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, camPos, hitVolume);

        // RUNG MÀN HÌNH NGAY KHI CỘT MỌC
        if (GameFeelManager.instance != null) GameFeelManager.instance.TriggerCameraShake(0.2f, 0.25f);

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(gameObject.tag)) return;

        CharacterController2D targetController = other.GetComponent<CharacterController2D>();
        int finalDamage = damage;
        bool isBlocked = false;

        if (targetController != null && targetController.isBlocking)
        {
            isBlocked = true;
            finalDamage = finalDamage / 2; // Kháng 50%
        }

        bool hitSomeone = false;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null) { enemy.TakeDamage(finalDamage); hitSomeone = true; }

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null) { player.TakeDamage(finalDamage); hitSomeone = true; }

        // --- XỬ LÝ KHI THỰC SỰ TRÚNG ĐỊCH ---
        if (hitSomeone)
        {
            if (!hasPlayedImpactSound)
            {
                Vector3 camPos = Camera.main != null ? Camera.main.transform.position : transform.position;

                // Nếu địch đỡ -> Kêu tiếng Keng
                if (isBlocked && blockSound != null)
                {
                    AudioSource.PlayClipAtPoint(blockSound, camPos, blockVolume);
                }
                // Nếu trúng da thịt -> Kêu tiếng Phập/Nổ (nếu có bỏ vào ô Impact Sound)
                else if (!isBlocked && impactSound != null)
                {
                    AudioSource.PlayClipAtPoint(impactSound, camPos, impactVolume);
                }
                hasPlayedImpactSound = true;
            }

            // Khựng hình (Hit Stop) khi lưỡi đất ghim vào người
            if (!hasTriggeredHitStop && GameFeelManager.instance != null)
            {
                GameFeelManager.instance.TriggerHitStop(0.05f);
                hasTriggeredHitStop = true;
            }
        }
    }
}