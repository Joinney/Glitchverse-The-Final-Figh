using System.Collections; // Bắt buộc phải có dòng này để dùng bộ đếm thời gian
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 1000; // Có thể đổi thành 1000 hoặc giữ nguyên tuỳ bạn cân bằng với Naruto
    private int currentHealth;
    
    private Animator anim;
    private Rigidbody2D rb;
    private CharacterController2D aiScript; // Dây thần kinh nối với não AI

    [Header("Thời gian bị choáng (giây)")]
    public float stunDuration = 0.6f; // Bạn có thể chỉnh số này thoải mái ngoài Inspector

    private Coroutine stunCoroutine; // Biến ghi nhớ trạng thái đang bị choáng

    [Header("Âm Thanh Đau Đớn Của AI")]
    private AudioSource audioSource;
    public AudioClip[] hitSounds; // Nơi chứa các file .wav tiếng kêu của Zenitsu ngoài Inspector

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        aiScript = GetComponent<CharacterController2D>(); // Lấy bộ não AI của nhân vật
        
        audioSource = GetComponent<AudioSource>(); // Tự động kết nối với Cái loa trên người AI
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // PHÁT ÂM THANH KHI AI BỊ ĐÁNH:
        if (audioSource != null && hitSounds.Length > 0)
        {
            // Chọn ngẫu nhiên 1 âm thanh gầm gừ/đau đớn của Zenitsu trong danh sách kéo vào
            AudioClip randomClip = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(randomClip);
        }

        // 1. Chạy hoạt ảnh giật nảy mình
        if (anim != null) anim.SetTrigger("Hit");

        // 2. Nếu đang bị choáng dở mà ăn thêm đòn -> Hủy đếm giờ cũ, tính thời gian choáng lại từ đầu (Combo liên hoàn)
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(StunRoutine());

        // 3. Kiểm tra tử vong
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Bộ đếm thời gian thực hiện hiệu ứng Choáng
    private IEnumerator StunRoutine()
    {
        // BƯỚC 1: Tắt não AI (để nó ngừng đọc lệnh dí theo người chơi)
        if (aiScript != null) aiScript.enabled = false;

        // BƯỚC 2: Phanh gấp cơ thể! Đánh trúng là phải đứng sựng lại tại chỗ
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // BƯỚC 3: Đứng im chịu trận trong đúng khoảng thời gian stunDuration
        yield return new WaitForSeconds(stunDuration);

        // BƯỚC 4: Hết thời gian choáng -> Bật não lên cho nó tỉnh lại chiến tiếp (nếu chưa chết)
        if (currentHealth > 0 && aiScript != null)
        {
            aiScript.enabled = true;
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã bị hạ gục!");
        
        // Tự động quét xem Animator đang xài biến tên gì để gọi cho đúng
        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "Dead") { anim.SetTrigger("Dead"); break; }
                if (param.name == "Die") { anim.SetTrigger("Die"); break; }
            }
        }

        // Tắt não và vật lý của Kẻ địch
        if (aiScript != null) aiScript.enabled = false;
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        
        Rigidbody2D targetRb = GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetRb.linearVelocity = Vector2.zero;
            targetRb.simulated = false; // Ngừng giả lập vật lý trọng lực hoàn toàn ngay tại chỗ
        }
        
        this.enabled = false; 
    }
}