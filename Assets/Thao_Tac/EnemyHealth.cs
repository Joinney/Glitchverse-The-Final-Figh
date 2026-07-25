using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Thông tin UI")]
    public Sprite characterFace;
    public string characterName = "Name NV";

    // TÁCH BIẾN GIỐNG PLAYER: Đổi maxHealth thành baseHealth
    public int baseHealth = 1000;

    // Khai báo ẩn maxHealth để code tự tính
    [HideInInspector] public int maxHealth;

    public HealthBarUI healthBar;
    private int currentHealth;

    private Animator anim;
    private Rigidbody2D rb;
    private CharacterController2D aiScript;

    [Header("Thời gian bị choáng (giây)")]
    public float stunDuration = 0.6f;

    private Coroutine stunCoroutine;

    [Header("Âm Thanh Đau Đớn Của AI")]
    private AudioSource audioSource;
    public AudioClip[] hitSounds;

    void Start()
    {
        // CẬP NHẬT TỈ LỆ MÁU TỪ SETTINGS
        float tyLeMau = PlayerPrefs.GetFloat("HealthMultiplier", 1f);

        // Dùng máu gốc nhân tỉ lệ
        maxHealth = Mathf.RoundToInt(baseHealth * tyLeMau);

        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        aiScript = GetComponent<CharacterController2D>();

        audioSource = GetComponent<AudioSource>();
        // (Khuyến nghị: Tương tự Player, nếu tối ưu được thì kéo thả thẳng vào Inspector thay vì dùng GameObject.Find)
        healthBar = GameObject.Find("HealthBar_P2").GetComponent<HealthBarUI>();

        if (healthBar != null && characterFace != null)
        {
            healthBar.SetAvatar(characterFace);
            healthBar.SetCharacterName(characterName);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (healthBar != null) healthBar.SetHealth(currentHealth, maxHealth);

        if (audioSource != null && hitSounds.Length > 0)
        {
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
        if (aiScript != null) aiScript.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(stunDuration);

        if (currentHealth > 0 && aiScript != null) aiScript.enabled = true;
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã bị hạ gục!");

        // 1. CHẠY HOẠT ẢNH GỤC NGÃ
        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "Dead") { anim.SetTrigger("Dead"); break; }
                if (param.name == "Die") { anim.SetTrigger("Die"); break; }
            }
        }

        // 2. KHÓA AI DI CHUYỂN
        if (aiScript != null) aiScript.enabled = false;
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        // 3. --- HIỆU ỨNG HẤT VĂNG LÊN KHÔNG (KNOCK-UP) ---
        if (rb != null)
        {
            // Xác định hướng văng (văng ngược lại với hướng đang nhìn)
            float vangX = transform.localScale.x > 0 ? -1.5f : 1.5f;
            float vangY = 6f;

            // Xóa hết quán tính cũ và bơm lực hất tung
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = new Vector2(vangX, vangY);

            // Gọi bộ đếm giờ để khóa cái xác lại sau khi nó rơi chạm đất
            StartCoroutine(FreezeBodyAfterDelay(2f));
        }

        // 4. GỌI GÓC QUAY CINEMATIC
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && GameFeelManager.instance != null)
        {
            GameFeelManager.instance.TriggerCinematicFinish(this.transform, player.transform, true);
        }
        else
        {
            MatchController match = FindAnyObjectByType<MatchController>();
            if (match != null) match.EndMatch(true);
        }

        this.enabled = false;
    }

    // Đợi xác rơi xuống đất rồi khóa cứng lại (không cho trượt trên sàn)
    private IEnumerator FreezeBodyAfterDelay(float delay)
    {
        // Dùng Realtime vì lúc này game đang bị Slow-mo 
        yield return new WaitForSecondsRealtime(delay);
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
    }
}