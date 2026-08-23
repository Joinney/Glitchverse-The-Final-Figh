using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Thông Tin Cơ Bản")]
    public string characterName = "Tatsumaki";
    public Sprite characterFace;
    public int baseHealth = 2500;
    public int maxHealth;
    public int currentHealth;

    [Header("Cài Đặt Thanh Máu Trên Đầu")]
    public bool showFloatingHealthBar = true;
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    private FloatingHealthBar floatingBar;

    [Header("Cấu Hình Rớt Xu (Chỉ Áp Dụng Cho Quái Thường)")]
    public GameObject coinPrefab;     // Kéo Prefab CoinDrop vào đây
    public int dropCoinAmount = 15;   // Goblin: 15 xu, Skeleton: 25 xu

    [Header("Âm Thanh Bị Đánh")]
    private AudioSource audioSource;
    public AudioClip[] hitSounds;

    private Animator anim;
    private Rigidbody2D rb;
    private MinionMonsterAI minionAI;
    private CharacterController2D aiBossCtrl;
    private HealthBarUI bossMainHealthBar;

    void Awake()
    {
        // ⚡ TRẢ VỀ LƯỢNG MÁU GỐC THEO CÀI ĐẶT
        float tyLeMau = PlayerPrefs.GetFloat("HealthMultiplier", 1f);
        maxHealth = Mathf.RoundToInt(baseHealth * tyLeMau);
        currentHealth = maxHealth;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        minionAI = GetComponent<MinionMonsterAI>();
        aiBossCtrl = GetComponent<CharacterController2D>();

        bool isMainBoss = (aiBossCtrl != null && minionAI == null) || gameObject.name.Contains("Tatsumaki");

        // 🛑 NẾU LÀ BOSS -> TỰ ĐỘNG TẮT THANH COIN HUD ĐỂ TẬP TRUNG ĐẤU BOSS
        if (isMainBoss)
        {
            if (CoinHUDManager.instance != null)
            {
                CoinHUDManager.instance.SetHUDActive(false);
            }

            InitBossUI();
        }
        else if (showFloatingHealthBar)
        {
            GameObject barObj = new GameObject(gameObject.name + "_FloatingHealth");
            floatingBar = barObj.AddComponent<FloatingHealthBar>();
            floatingBar.Setup(transform, currentHealth, maxHealth, healthBarOffset);
        }
    }

    public void InitBossUI()
    {
        if (maxHealth <= 0)
        {
            float tyLeMau = PlayerPrefs.GetFloat("HealthMultiplier", 1f);
            maxHealth = Mathf.RoundToInt(baseHealth * tyLeMau);
            currentHealth = maxHealth;
        }

        HealthBarUI[] allBars = FindObjectsByType<HealthBarUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var bar in allBars)
        {
            if (bar.gameObject.name == "HealthBar_P2")
            {
                bossMainHealthBar = bar;
                bar.gameObject.SetActive(true);
                
                if (characterFace != null) bar.SetAvatar(characterFace);
                bar.SetCharacterName(characterName);
                bar.SetHealth(currentHealth, maxHealth);
                break;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        DamagePopup.Create(transform.position + Vector3.up * (healthBarOffset.y * 0.7f), damage);

        if (floatingBar != null)
        {
            floatingBar.UpdateHealth(currentHealth, maxHealth);
        }

        if (bossMainHealthBar != null)
        {
            bossMainHealthBar.SetHealth(currentHealth, maxHealth);
        }

        if (audioSource != null && hitSounds != null && hitSounds.Length > 0)
        {
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
            if (clip != null) audioSource.PlayOneShot(clip);
        }

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

    private bool isDead = false;

    void Die()
    {
        if (isDead) return; // ✨ Chặn gọi Die nhiều lần
        isDead = true;

        if (floatingBar != null)
        {
            Destroy(floatingBar.gameObject);
        }

        if (minionAI != null)
        {
            if (coinPrefab != null)
            {
                GameObject coin = Instantiate(coinPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                CoinPickup pickup = coin.GetComponent<CoinPickup>();
                if (pickup != null)
                {
                    pickup.coinValue = dropCoinAmount;
                }
            }

            minionAI.Die();
            return;
        }

        // 👑 KHU VỰC CHẾT CỦA BOSS CHÍNH / MINI BOSS (KHÔNG RỚT XU)
        if (anim != null) anim.SetTrigger("Dead");
        if (aiBossCtrl != null) aiBossCtrl.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = new Vector2(transform.localScale.x > 0 ? -2f : 2f, 3f);
        }

        if (GameFeelManager.instance != null)
        {
            GameFeelManager.instance.TriggerCinematicFinish(this.transform, null, true);
        }
        else
        {
            MatchController match = FindAnyObjectByType<MatchController>();
            if (match != null)
            {
                match.EndMatch(true);
            }
        }

        Destroy(gameObject, 2.5f);
    }

    private void OnDestroy()
    {
        if (floatingBar != null)
        {
            Destroy(floatingBar.gameObject);
        }
    }
}