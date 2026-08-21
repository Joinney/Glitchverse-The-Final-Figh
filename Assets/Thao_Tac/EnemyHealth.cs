using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Thông tin UI")]
    public Sprite characterFace;
    public string characterName = "Name NV";

    public int baseHealth = 1000;
    [HideInInspector] public int maxHealth;
    public HealthBarUI healthBar;
    public int currentHealth;

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
        float tyLeMau = PlayerPrefs.GetFloat("HealthMultiplier", 1f);
        maxHealth = Mathf.RoundToInt(baseHealth * tyLeMau);
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        aiScript = GetComponent<CharacterController2D>();
        audioSource = GetComponent<AudioSource>();

        // An toàn tìm kiếm UI (Bỏ qua nếu là map đi cảnh hoặc quái nhỏ không có HealthBar_P2)
        GameObject p2BarObj = GameObject.Find("HealthBar_P2");
        if (p2BarObj != null)
        {
            healthBar = p2BarObj.GetComponent<HealthBarUI>();
            if (healthBar != null && characterFace != null)
            {
                healthBar.SetAvatar(characterFace);
                healthBar.SetCharacterName(characterName);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (healthBar != null) healthBar.SetHealth(currentHealth, maxHealth);

        if (ComboManager.instance != null) ComboManager.instance.AddCombo();

        // 🔊 Phát âm thanh trúng đòn (Hoạt động tốt cả khi không có AudioSource trên Prefab)
        if (hitSounds != null && hitSounds.Length > 0)
        {
            AudioClip randomClip = hitSounds[Random.Range(0, hitSounds.Length)];
            if (randomClip != null)
            {
                AudioSource.PlayClipAtPoint(randomClip, transform.position);
            }
        }

        if (anim != null) anim.SetTrigger("Hit");

        // 💥 Xử lý đẩy lùi & gọi chết cho quái nhỏ (Skeleton / Goblin)
        MinionMonsterAI minionAI = GetComponent<MinionMonsterAI>();
        if (minionAI != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Hướng đẩy lùi ngược chiều với hướng Player đang đứng
                float pushDir = transform.position.x > player.transform.position.x ? 1f : -1f;
                Vector2 knockForce = new Vector2(pushDir * 4.5f, 1.5f);
                minionAI.TakeKnockback(knockForce);
            }

            if (currentHealth <= 0)
            {
                minionAI.Die();
                return;
            }
        }

        // Xử lý choáng cho Boss / Nhân vật đối kháng
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

        // 1. Chạy hoạt ảnh gục ngã
        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "Dead") { anim.SetTrigger("Dead"); break; }
                if (param.name == "Die") { anim.SetTrigger("Die"); break; }
            }
        }

        // 2. Khóa AI di chuyển
        if (aiScript != null) aiScript.enabled = false;
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        // 3. Hiệu ứng hất văng
        if (rb != null)
        {
            float vangX = transform.localScale.x > 0 ? -1.0f : 1.0f;
            float vangY = 2.5f;
            rb.linearVelocity = new Vector2(vangX, vangY);
        }

        // 4. Kết thúc trận đấu hoặc kích hoạt Cinematic
        if (GameFeelManager.instance != null)
        {
            GameFeelManager.instance.TriggerCinematicFinish(this.transform, null, true);
        }
        else
        {
            MatchController match = FindAnyObjectByType<MatchController>();
            if (match != null) match.EndMatch(true);
        }

        this.enabled = false;
    }
}