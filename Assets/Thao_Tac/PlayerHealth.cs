using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3800;
    private int currentHealth;
    [Header("Âm Thanh Đau Đớn")]
    private AudioSource audioSource;
    public AudioClip[] hitSounds;
    private Animator anim;
    private Rigidbody2D rb;
    private CharacterController2D playerScript;

    [Header("Thời gian bị choáng (giây)")]
    public float stunDuration = 0.5f;

    private Coroutine stunCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerScript = GetComponent<CharacterController2D>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (audioSource != null && hitSounds.Length > 0)
        {
            // Chọn ngẫu nhiên 1 âm thanh trong danh sách kéo vào để nghe đỡ chán
            AudioClip randomClip = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(randomClip);
        }

        if (anim != null) anim.SetTrigger("Hit");

        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine());

        if (currentHealth <= 0) Die();
    }

    private IEnumerator StunRoutine()
    {
        // Khóa phím, không cho Player bấm nút di chuyển hay ném chiêu
        if (playerScript != null) playerScript.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(stunDuration);

        // Hết choáng -> Mở khóa bàn phím
        if (currentHealth > 0 && playerScript != null) playerScript.enabled = true;
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã tử trận!");
        
        // Tự động quét xem Animator đang xài biến tên gì để gọi cho đúng
        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "Dead") { anim.SetTrigger("Dead"); break; }
                if (param.name == "Die") { anim.SetTrigger("Die"); break; }
            }
        }

        // Tắt bàn phím và vật lý của Người chơi
        if (playerScript != null) playerScript.enabled = false;
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Ngừng giả lập vật lý trọng lực hoàn toàn ngay tại chỗ
        }
        
        this.enabled = false; 
    }
}