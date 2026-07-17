using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Thông tin UI")]
    public Sprite characterFace;
    public string characterName = "Name NV";
    public int maxHealth = 1000;
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
        maxHealth = Mathf.RoundToInt(maxHealth * tyLeMau);

        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        aiScript = GetComponent<CharacterController2D>();

        audioSource = GetComponent<AudioSource>();
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
        // --- GỌI TRỌNG TÀI BÁO THẮNG ---
        MatchController match = FindAnyObjectByType<MatchController>();
        if (match != null) match.EndMatch(true);
        // -------------------------------

        Debug.Log(gameObject.name + " đã bị hạ gục!");

        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "Dead") { anim.SetTrigger("Dead"); break; }
                if (param.name == "Die") { anim.SetTrigger("Die"); break; }
            }
        }

        if (aiScript != null) aiScript.enabled = false;
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        Rigidbody2D targetRb = GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetRb.linearVelocity = Vector2.zero;
            targetRb.simulated = false;
        }

        this.enabled = false;
    }
}