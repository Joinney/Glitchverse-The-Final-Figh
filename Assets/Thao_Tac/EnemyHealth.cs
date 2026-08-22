using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Thông Tin Cơ Bản")]
    public string characterName = "Monster";
    public int baseHealth = 100;
    [HideInInspector] public int maxHealth;
    public int currentHealth;

    [Header("Cài Đặt Thanh Máu Trên Đầu")]
    public bool showFloatingHealthBar = true;
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    private FloatingHealthBar floatingBar;

    [Header("Âm Thanh Bị Đánh")]
    private AudioSource audioSource;
    public AudioClip[] hitSounds;

    private Animator anim;
    private Rigidbody2D rb;
    private MinionMonsterAI minionAI;
    private CharacterController2D aiBossCtrl;
    private HealthBarUI bossMainHealthBar;

    void Start()
    {
        float tyLeMau = PlayerPrefs.GetFloat("HealthMultiplier", 1f);
        maxHealth = Mathf.RoundToInt(baseHealth * tyLeMau);
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        minionAI = GetComponent<MinionMonsterAI>();
        aiBossCtrl = GetComponent<CharacterController2D>();

        // 1. Chỉ hiện thanh máu trên đầu cho quái thường và Mini Boss (không hiện cho Boss chính trong màn đấu võ)
        bool isRealBossFight = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Fight_Stage");
        
        if (showFloatingHealthBar && !isRealBossFight)
        {
            GameObject barObj = new GameObject(gameObject.name + "_FloatingHealth");
            floatingBar = barObj.AddComponent<FloatingHealthBar>();
            floatingBar.Setup(transform, currentHealth, maxHealth, healthBarOffset);
        }

        // 2. Khớp thanh máu góc trên bên phải UI cho Boss chính
        GameObject barP2 = GameObject.Find("HealthBar_P2");
        if (barP2 != null && !gameObject.name.Contains("Goblin") && !gameObject.name.Contains("skeleton") && !gameObject.name.Contains("DarkWolf"))
        {
            bossMainHealthBar = barP2.GetComponent<HealthBarUI>();
            if (bossMainHealthBar != null)
            {
                bossMainHealthBar.SetCharacterName(characterName);
                bossMainHealthBar.SetHealth(currentHealth, maxHealth);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // Bắn chữ số dame
        DamagePopup.Create(transform.position + Vector3.up * (healthBarOffset.y * 0.7f), damage);

        // Cập nhật thanh máu trên đầu
        if (floatingBar != null)
        {
            floatingBar.UpdateHealth(currentHealth, maxHealth);
        }

        // Cập nhật thanh máu UI trên cùng màn hình
        if (bossMainHealthBar != null)
        {
            bossMainHealthBar.SetHealth(currentHealth, maxHealth);
        }

        // Âm thanh
        if (audioSource != null && hitSounds != null && hitSounds.Length > 0)
        {
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
            if (clip != null) audioSource.PlayOneShot(clip);
        }

        // Đẩy lùi quái nhỏ/Mini Boss
        if (minionAI != null)
        {
            Vector2 knockbackDir = transform.localScale.x > 0 ? new Vector2(-1.2f, 0.5f) : new Vector2(1.2f, 0.5f);
            minionAI.TakeKnockback(knockbackDir);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (floatingBar != null)
        {
            Destroy(floatingBar.gameObject);
        }

        // Nếu là quái thường hoặc Mini Boss trong map sinh tồn
        if (minionAI != null)
        {
            minionAI.Die();
            return;
        }

        // 👑 XỬ LÝ KHI BOSS CHÍNH BỊ HẠ GỤC:
        if (anim != null) anim.SetTrigger("Dead");
        if (aiBossCtrl != null) aiBossCtrl.enabled = false;

        // Văng nhẹ khi ngã gục
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = new Vector2(transform.localScale.x > 0 ? -2f : 2f, 3f);
        }

        // 🏆 BÁO CHO MATCH CONTROLLER KÍCH HOẠT CHIẾN THẮNG (VICTORY)
        if (GameFeelManager.instance != null)
        {
            GameFeelManager.instance.TriggerCinematicFinish(this.transform, null, true);
        }
        else
        {
            MatchController match = FindAnyObjectByType<MatchController>();
            if (match != null)
            {
                match.EndMatch(true); // true = Player 1 Chiến Thắng!
            }
        }

        Destroy(gameObject, 2.0f);
    }

    private void OnDestroy()
    {
        if (floatingBar != null)
        {
            Destroy(floatingBar.gameObject);
        }
    }
}