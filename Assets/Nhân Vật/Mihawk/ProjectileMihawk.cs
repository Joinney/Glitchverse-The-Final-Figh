using UnityEngine;

public class ProjectileMihawk : MonoBehaviour
{
    [HideInInspector]
    public float speed = 15f;
    public float lifeTime = 3f;
    public int damage = 100;

    [Header("Phân Loại Chiêu Thức")]
    [Tooltip("Gõ 2, 3 hoặc 4. 2: Bị đỡ 100% | 3: Bị đỡ 50% dame | 4: Không thể đỡ (Ulti)")]
    public int skillType = 2; // Mặc định là Skill 2

    [Header("Xử lý Hình Ảnh & Âm Thanh")]
    public float hitAnimationDuration = 0.5f; // Đạn sẽ đứng im bao lâu để chờ diễn xong hoạt ảnh nổ?
    public GameObject impactEffect; // Kéo prefab vụ nổ rời (nếu có) vào đây

    private AudioSource audioSource;
    public AudioClip castSound; // Tiếng vung kiếm xé gió
    public AudioClip hitSound;  // Tiếng kiếm chém trúng thịt
    public AudioClip blockSound; // (Tùy chọn) Tiếng kim loại va chạm khi bị đỡ đòn

    void Start()
    {
        // 1. Dùng lực vật lý đẩy đạn đi thẳng
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(speed, 0f);
        }

        // 2. Phát tiếng vung kiếm ngay khi đạn sinh ra
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && castSound != null)
        {
            audioSource.PlayOneShot(castSound);
        }

        // 3. Tự hủy nếu bay quá xa khỏi màn hình
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Nhận diện thông minh: Player bắn trúng Enemy HOẶC Enemy bắn trúng Player
        if ((gameObject.CompareTag("Player") && other.CompareTag("Enemy")) ||
            (gameObject.CompareTag("Enemy") && other.CompareTag("Player")))
        {
            // --- LOGIC KIỂM TRA ĐỠ ĐÒN (BLOCK) ---
            CharacterController2D targetController = other.GetComponent<CharacterController2D>();
            int finalDamage = damage;
            bool isBlocked = false;

            if (targetController != null && targetController.isBlocking)
            {
                isBlocked = true;

                if (skillType == 2)
                {
                    finalDamage = 0; // Skill 2: Kháng 100% sát thương
                }
                else if (skillType == 3)
                {
                    finalDamage = damage / 2; // Skill 3: Kháng 50% sát thương
                }
                else if (skillType == 4)
                {
                    finalDamage = damage; // Skill 4 (Ulti): Ăn trọn sát thương
                    isBlocked = false; // Xuyên thủng phòng ngự
                }
            }

            // --- TRỪ MÁU MỤC TIÊU ---
            if (finalDamage > 0)
            {
                other.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage);
                other.GetComponent<PlayerHealth>()?.TakeDamage(finalDamage);
            }

            // --- XỬ LÝ ÂM THANH KHI TRÚNG/ĐỠ ---
            if (audioSource != null)
            {
                if (isBlocked && blockSound != null)
                    audioSource.PlayOneShot(blockSound); // Kêu tiếng "Keng" nếu bị đỡ
                else if (hitSound != null)
                    audioSource.PlayOneShot(hitSound); // Kêu tiếng xuyệt thịt nếu trúng
            }

            // Sinh ra hiệu ứng nổ văng tóe lửa
            if (impactEffect != null)
            {
                Instantiate(impactEffect, transform.position, transform.rotation);
            }

            // --- CHỐNG BUG DOUBLE-HIT ---
            // Bước 1: Phanh gấp, không cho kiếm khí bay xuyên qua người địch
            Rigidbody2D realRb = GetComponent<Rigidbody2D>();
            if (realRb != null) realRb.linearVelocity = Vector2.zero;

            // Bước 2: Tắt lưỡi dao (Collider) để không trừ máu đối thủ thêm lần 2
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Bước 3: Ép kiếm khí chuyển sang frame vỡ vụn
            Animator anim = GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("Hit"); // Nhớ bỏ comment dòng này nếu bạn đã cài Trigger "Hit" trong Animator đạn nhé!

            // Xóa viên đạn sau khi diễn xong hiệu ứng
            Destroy(gameObject, hitAnimationDuration);
        }
    }
}