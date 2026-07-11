using UnityEngine;

public class ProjectileMihawk : MonoBehaviour 
{
    [HideInInspector] 
    public float speed = 15f; 
    public float lifeTime = 3f;
    public int damage = 100; 

    [Header("Xử lý Hình Ảnh & Âm Thanh")]
    public float hitAnimationDuration = 0.5f; // Đạn sẽ đứng im bao lâu để chờ diễn xong hoạt ảnh nổ?
    public GameObject impactEffect; // Kéo prefab vụ nổ rời (nếu có) vào đây
    
    private AudioSource audioSource;
    public AudioClip castSound; // Tiếng vung kiếm xé gió
    public AudioClip hitSound;  // Tiếng kiếm chém trúng thịt

    void Start()
    {
        // 1. Dùng lực vật lý đẩy đạn đi thẳng (Khắc phục lỗi đạn xuyên người)
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
            // Trừ máu chính xác mục tiêu
            other.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            // Phát tiếng nổ/chém trúng
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            // Sinh ra hiệu ứng nổ văng tóe lửa (nếu bạn có Prefab nổ bên ngoài)
            if (impactEffect != null)
            {
                Instantiate(impactEffect, transform.position, transform.rotation);
            }

            // CHỐNG BUG DOUBLE-HIT BẰNG 3 BƯỚC:
            // Bước 1: Phanh gấp, không cho kiếm khí bay xuyên qua người địch nữa
            Rigidbody2D realRb = GetComponent<Rigidbody2D>();
            if (realRb != null) realRb.linearVelocity = Vector2.zero;

            // Bước 2: Tắt lưỡi dao (Collider) để không trừ máu đối thủ thêm lần 2
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Bước 3: Ép kiếm khí chuyển sang frame vỡ vụn (Nếu có gắn Animator)
            Animator anim = GetComponent<Animator>();
            if (anim != null) 
            //anim.SetTrigger("Hit");

            // Xóa viên đạn sau khi diễn xong hiệu ứng
            Destroy(gameObject, hitAnimationDuration);
        }
    }
}