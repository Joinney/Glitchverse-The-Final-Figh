using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Thông tin UI")]
    public Sprite characterFace;
    public string characterName = "Name NV";
    public int maxHealth = 3800;
    public HealthBarUI healthBar;
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
        // CẬP NHẬT TỈ LỆ MÁU TỪ SETTINGS
        float tyLeMau = PlayerPrefs.GetFloat("HealthMultiplier", 1f);
        maxHealth = Mathf.RoundToInt(maxHealth * tyLeMau);

        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerScript = GetComponent<CharacterController2D>();
        healthBar = GameObject.Find("HealthBar_P1").GetComponent<HealthBarUI>();

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
        if (playerScript != null) playerScript.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(stunDuration);

        if (currentHealth > 0 && playerScript != null) playerScript.enabled = true;
    }

    void Die()
    {
        // --- GỌI TRỌNG TÀI BÁO THUA ---
        MatchController match = FindAnyObjectByType<MatchController>();
        if (match != null) match.EndMatch(false);
        // ------------------------------

        Debug.Log(gameObject.name + " đã tử trận!");

        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "Dead") { anim.SetTrigger("Dead"); break; }
                if (param.name == "Die") { anim.SetTrigger("Die"); break; }
            }
        }

        if (playerScript != null) playerScript.enabled = false;
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        this.enabled = false;
    }
}