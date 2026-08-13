using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Thông tin UI")]
    public Sprite characterFace;
    public string characterName = "Name NV";

    public int baseHealth = 3800;
    [HideInInspector] public int maxHealth;
    public int currentHealth;

    [Header("Âm Thanh Đau Đớn")]
    private AudioSource audioSource;
    public AudioClip[] hitSounds;
    private Animator anim;
    private Rigidbody2D rb;
    private CharacterController2D playerScript;

    [Header("Thời gian bị choáng (giây)")]
    public float stunDuration = 0.5f;

    private Coroutine stunCoroutine;
    private HealthBarUI healthBar;

    void Start()
    {
        // CẬP NHẬT TỈ LỆ MÁU TỪ SETTINGS
        float tyLeMau = PlayerPrefs.GetFloat("HealthMultiplier", 1f);
        maxHealth = Mathf.RoundToInt(baseHealth * tyLeMau);
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerScript = GetComponent<CharacterController2D>();

        // ==========================================
        // TỰ ĐỘNG PHÂN BIỆT THANH MÁU P1 HAY P2 / ENEMY
        // ==========================================
        string gameMode = PlayerPrefs.GetString("GameMode", "Single");

        if (gameMode == "PvP")
        {
            // Nếu là chế độ 2 người: Dựa vào playerIndex để gán thanh máu
            if (playerScript != null && playerScript.playerIndex == 2)
            {
                GameObject barP2 = GameObject.Find("HealthBar_P2");
                if (barP2 != null) healthBar = barP2.GetComponent<HealthBarUI>();
            }
            else
            {
                // Mặc định cho Player 1
                GameObject barP1 = GameObject.Find("HealthBar_P1");
                if (barP1 != null) healthBar = barP1.GetComponent<HealthBarUI>();
            }
        }
        else
        {
            // Nếu là chế độ Chơi đơn (Singleplayer): Player dùng P1, AI dùng P2
            if (gameObject.CompareTag("Enemy"))
            {
                GameObject barP2 = GameObject.Find("HealthBar_P2");
                if (barP2 != null) healthBar = barP2.GetComponent<HealthBarUI>();
            }
            else
            {
                GameObject barP1 = GameObject.Find("HealthBar_P1");
                if (barP1 != null) healthBar = barP1.GetComponent<HealthBarUI>();
            }
        }

        // KÍCH HOẠT HIỂN THỊ LÊN THANH MÁU TƯƠNG TỨC
        if (healthBar != null)
        {
            if (characterFace != null) healthBar.SetAvatar(characterFace);
            healthBar.SetCharacterName(characterName);
            healthBar.SetHealth(currentHealth, maxHealth);
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

        if (rb != null)
        {
            float vangX = transform.localScale.x > 0 ? -1.5f : 1.5f;
            float vangY = 2.5f;

            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = new Vector2(vangX, vangY);
        }

        if (GameFeelManager.instance != null)
        {
            GameFeelManager.instance.TriggerCinematicFinish(this.transform, null, false);
        }
        else
        {
            // ==========================================
            // NÂNG CẤP: GỌI MATCH CONTROLLER THEO ĐÚNG CHẾ ĐỘ
            // ==========================================
            MatchController match = FindAnyObjectByType<MatchController>();
            if (match != null)
            {
                string mode = PlayerPrefs.GetString("GameMode", "Single");
                
                if (mode == "PvP")
                {
                    // Logic Đối Kháng: Ai chết thì người kia thắng!
                    // Lấy playerIndex để biết nạn nhân là ai
                    int winner = (playerScript != null && playerScript.playerIndex == 1) ? 2 : 1;
                    match.EndPvPMatch(winner);
                }
                else
                {
                    // Logic Đi Ải (Giữ nguyên như cũ)
                    match.EndMatch(false);
                }
            }
        }

        this.enabled = false;
    }
}