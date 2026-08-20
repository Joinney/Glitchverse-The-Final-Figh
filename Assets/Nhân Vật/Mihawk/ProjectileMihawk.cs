using UnityEngine;

public class ProjectileMihawk : MonoBehaviour
{
    [HideInInspector]
    public float speed = 15f;
    public float lifeTime = 3f;
    public int damage = 100;

    [Header("Phân Loại Chiêu Thức")]
    [Tooltip("Gõ 2, 3 hoặc 4. 2: Bị đỡ 100% | 3: Bị đỡ 50% dame | 4: Không thể đỡ (Ulti)")]
    public int skillType = 2;

    [Header("Xử lý Hình Ảnh")]
    public float hitAnimationDuration = 0.5f;
    public GameObject impactEffect;

    [Header("Xử lý Âm Thanh (Tự do chỉnh to nhỏ)")]
    public AudioClip castSound;
    [Range(0f, 1f)] public float castVolume = 1f; // Chỉnh âm lượng tiếng chém gió

    public AudioClip hitSound;
    [Range(0f, 1f)] public float hitVolume = 1f;  // Chỉnh âm lượng tiếng trúng thịt

    public AudioClip blockSound;
    [Range(0f, 1f)] public float blockVolume = 1f; // Chỉnh âm lượng tiếng đỡ đòn

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(speed, 0f);
        }

        // ==========================================
        // 💡 CẢI TIẾN: PHÁT ÂM THANH NGAY TẠI CAMERA ĐỂ NGHE TO RÕ NHẤT
        // ==========================================
        if (castSound != null)
        {
            // Ép nó phát ngay sát lỗ tai người chơi (Camera) để tiếng Cast luôn to và uy lực
            Vector3 camPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(castSound, camPos, castVolume);
        }

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((gameObject.CompareTag("Player") && other.CompareTag("Enemy")) ||
            (gameObject.CompareTag("Enemy") && other.CompareTag("Player")))
        {
            CharacterController2D targetController = other.GetComponent<CharacterController2D>();
            int finalDamage = damage;
            bool isBlocked = false;

            if (targetController != null && targetController.isBlocking)
            {
                isBlocked = true;

                if (skillType == 2) finalDamage = 0;
                else if (skillType == 3) finalDamage = damage / 2;
                else if (skillType == 4)
                {
                    finalDamage = damage;
                    isBlocked = false;
                }
            }

            if (finalDamage > 0)
            {
                other.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage);
                other.GetComponent<PlayerHealth>()?.TakeDamage(finalDamage);
            }

            // ==========================================
            // 💡 CẢI TIẾN: DÙNG LOA TÀNG HÌNH ĐỂ ÂM THANH KHÔNG BỊ CẮT ĐỨT
            // ==========================================
            if (isBlocked && blockSound != null)
            {
                AudioSource.PlayClipAtPoint(blockSound, transform.position, blockVolume);
            }
            else if (!isBlocked && hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
            }

            if (impactEffect != null)
            {
                Instantiate(impactEffect, transform.position, transform.rotation);
            }

            if (GameFeelManager.instance != null)
            {
                if (skillType == 4)
                {
                    GameFeelManager.instance.TriggerHitStop(0.1f);
                    GameFeelManager.instance.TriggerCameraShake(0.3f, 0.4f);
                }
                else if (skillType == 3)
                {
                    GameFeelManager.instance.TriggerHitStop(0.05f);
                    GameFeelManager.instance.TriggerCameraShake(0.2f, 0.25f);
                }
                else
                {
                    GameFeelManager.instance.TriggerHitStop(0.05f);
                    GameFeelManager.instance.TriggerCameraShake(0.1f, 0.15f);
                }
            }

            Rigidbody2D realRb = GetComponent<Rigidbody2D>();
            if (realRb != null) realRb.linearVelocity = Vector2.zero;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Animator anim = GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Hit");

            Destroy(gameObject, hitAnimationDuration);
        }
    }
}