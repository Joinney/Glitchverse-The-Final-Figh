using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Transform enemyTarget;

    [Header("Cấu Hình Phân Loại")]
    public bool isAI = false;
    public int playerIndex = 1; // 1 = P1, 2 = P2

    [Header("Thông Số Thuộc Tính")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float dashForce = 25f;
    public float attackRange = 2.5f;
    public float timeBetweenActions = 1.2f;

    [Header("Cấu Hình Nhảy (Double Jump)")]
    public int maxJumps = 2;
    private int jumpsRemaining;
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;
    public bool isGrounded;

    [Header("Cấu Hình Lướt (Né Đòn)")]
    public float dashDuration = 0.25f;
    public bool isDashing = false;

    [Header("Hệ Thống Combo Đánh Thường")]
    public int comboStep = 0;
    public float comboWindow = 0.8f;
    private float timeSinceLastAttack = 0f;

    [Header("Năng Lượng Tiêu Hao & Hồi Phục")]
    public int skill1Cost = 0;
    public int skill2Cost = 25;
    public int skill3Cost = 50;
    public int skill4Cost = 100;
    public int energyGainOnHit = 15;
    private EnergySystem energySys;

    [Header("Cấu Hình Cận Chiến (Skill 1/Combo)")]
    public Transform attackPoint;
    public float meleeHitRange = 1.2f;
    public LayerMask enemyLayers;
    public int meleeDamage = 35;

    [Header("Cấu Hình Hiệu Ứng Phóng Chiêu (Skill 2, 3, 4)")]
    public Transform castPoint;
    public Transform castPointSkill2;
    public Transform castPointSkill3;
    public Transform castPointSkill4;

    public GameObject skill2ProjectilePrefab;
    public GameObject skill3ProjectilePrefab;
    public GameObject skill4ProjectilePrefab;

    [Header("Trạng Thái Chiến Đấu")]
    public bool isBlocking = false;
    public bool isStunned = false;
    public bool isCastingUltimate = false;
    public bool canMoveAndFight = true;

    private float actionTimer = 0f;
    private float horizontalInput;
    private Vector3 originalScale;
    private int aiCurrentStrategy = 0;

    void Start()
    {
        CountdownManager.isCountdownFinished = true;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        energySys = GetComponent<EnergySystem>();
        originalScale = transform.localScale;
        jumpsRemaining = maxJumps;

        if (isAI)
        {
            FindEnemyTarget();
        }
    }

    void Update()
    {
        // 🦘 KIỂM TRA MẶT ĐẤT & HỒI LƯỢT NHẢY
        CheckGroundStatus();

        if (comboStep > 0)
        {
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack > comboWindow)
            {
                comboStep = 0;
            }
        }

        if (!isAI) HandlePlayerInput();
        else HandleAILogic();
    }

    private void CheckGroundStatus()
    {
        bool groundDetected = false;

        if (groundCheck != null)
        {
            groundDetected = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Tự động kiểm tra thêm qua tia Raycast phụ xuống chân nếu chưa gán GroundCheck
        if (!groundDetected)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, groundLayer);
            if (hit.collider != null) groundDetected = true;
        }

        // Khi tiếp đất và không bay lên
        if (groundDetected && Mathf.Abs(rb.linearVelocity.y) < 0.2f)
        {
            isGrounded = true;
            jumpsRemaining = maxJumps; // ✨ Hồi phục đủ 2 lần nhảy
        }
        else
        {
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Tiếp đất khi chạm bất kỳ sàn nào
        if (collision.gameObject.name.Contains("Nền") || collision.gameObject.name.Contains("Ground") || collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpsRemaining = maxJumps;
        }
    }

    void HandlePlayerInput()
    {
        if (isDashing || isStunned || isCastingUltimate) return;

        bool keyLeft = false, keyRight = false, keyJump = false, keyDash = false;
        bool keyS1 = false, keyS2 = false, keyS3 = false, keyS4 = false;
        bool keyBlock = false;

        if (playerIndex == 1)
        {
            keyLeft = Input.GetKey(KeyCode.A);
            keyRight = Input.GetKey(KeyCode.D);
            keyJump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W);
            keyBlock = Input.GetKey(KeyCode.K);
            keyDash = Input.GetKeyDown(KeyCode.L);
            
            keyS1 = Input.GetKeyDown(KeyCode.U);
            keyS2 = Input.GetKeyDown(KeyCode.I);
            keyS3 = Input.GetKeyDown(KeyCode.O);
            keyS4 = Input.GetKeyDown(KeyCode.P);
        }

        isBlocking = keyBlock;
        if (anim != null) anim.SetBool("IsBlocking", isBlocking);

        if (isBlocking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        if (keyLeft) horizontalInput = -1f;
        else if (keyRight) horizontalInput = 1f;
        else horizontalInput = 0f;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        if (horizontalInput > 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        // 🚀 NHẢY KHÔNG TỐN NĂNG LƯỢNG
        if (keyJump && jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (anim != null) anim.SetTrigger("Jump");
            jumpsRemaining--;
        }

        if (keyDash)
        {
            float dashDirection = transform.localScale.x > 0 ? 1 : -1;
            StartCoroutine(DashRoutine(dashDirection));
        }

        if (keyS1) PerformComboAttack();
        if (keyS2) TryUseSkill("Skill2", skill2Cost);
        if (keyS3) TryUseSkill("Skill3", skill3Cost);
        if (keyS4) UseUltimateSkill();
    }

    [Header("AI Chống Kẹt & Tầm Xa")]
    private float aiStuckTimer = 0f;
    private float aiLastPosX;
    public float rangedSkillMaxDistance = 8.5f; // Tầm bắn tối đa của Skill 2, 3, 4

    void HandleAILogic()
    {
        if (isDashing || isStunned || isCastingUltimate) return;

        if (enemyTarget == null)
        {
            FindEnemyTarget();
            if (enemyTarget == null) return;
        }

        float distanceToEnemy = Vector2.Distance(transform.position, enemyTarget.position);
        float deltaX = enemyTarget.position.x - transform.position.x;
        float moveDirection = deltaX > 0 ? 1f : -1f;

        // Luôn xoay mặt nhìn về phía Player
        transform.localScale = new Vector3(moveDirection * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        actionTimer += Time.deltaTime;

        // 🧠 1. HỆ THỐNG PHÁT HIỆN BỊ CẢN / KẸT ĐƯỜNG -> LƯỚT / NHẢY QUA NGAY
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            // Nếu đang di chuyển mà tọa độ thực tế gần như không đổi
            if (Mathf.Abs(transform.position.x - aiLastPosX) < 0.05f)
            {
                aiStuckTimer += Time.deltaTime;
                if (aiStuckTimer >= 0.45f) // Bị cản quá 0.45s
                {
                    aiStuckTimer = 0f;
                    // 50% cơ hội Lướt qua vật cản, 50% Nhảy vượt qua
                    if (Random.value > 0.5f)
                    {
                        StartCoroutine(DashRoutine(moveDirection));
                    }
                    else if (isGrounded)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                        if (anim != null) anim.SetTrigger("Jump");
                    }
                    return;
                }
            }
            else
            {
                aiStuckTimer = 0f;
            }
        }
        else
        {
            aiStuckTimer = 0f;
        }
        aiLastPosX = transform.position.x;

        // 🧠 2. CHỌN CHIẾN THUẬT RA CHIÊU NẾU CHƯA CÓ
        if (aiCurrentStrategy == 0)
        {
            // Tỉ lệ: 75% tung chiêu từ xa (Skill 2, 3, 4), 25% áp sát đấm thường (Skill 1)
            float roll = Random.value;
            if (roll < 0.25f) aiCurrentStrategy = 1;      // Đánh thường cận chiến
            else if (roll < 0.55f) aiCurrentStrategy = 2; // Skill 2 tầm xa
            else if (roll < 0.85f) aiCurrentStrategy = 3; // Skill 3 tầm xa
            else aiCurrentStrategy = 4;                   // Chiêu cuối (Ulti)
        }

        // 🧠 3. THỰC THI CHIẾN THUẬT
        if (aiCurrentStrategy == 1)
        {
            // === CHIÊU CẬN CHIẾN (SKILL 1 COMBO) -> PHẢI ÁP SÁT ===
            if (distanceToEnemy > meleeHitRange)
            {
                rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
                if (anim != null) anim.SetFloat("Speed", 1f);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (anim != null) anim.SetFloat("Speed", 0f);

                if (actionTimer >= timeBetweenActions)
                {
                    actionTimer = 0f;
                    PerformComboAttack();
                    aiCurrentStrategy = 0;
                }
            }
        }
        else
        {
            // === CHIÊU TẦM XA (SKILL 2, 3, 4) -> KHÔNG CẦN ÁP SÁT, TẦM BẮN RỘNG ===
            if (distanceToEnemy > rangedSkillMaxDistance)
            {
                // Nếu quá xa ngoài tầm bắn (> 8.5m) thì mới chạy lại gần
                rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
                if (anim != null) anim.SetFloat("Speed", 1f);
            }
            else
            {
                // ĐÃ VÀO TẦM BẮN (2m - 8.5m) -> DỪNG LẠI XẢ CHIÊU NGAY
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (anim != null) anim.SetFloat("Speed", 0f);

                if (actionTimer >= timeBetweenActions)
                {
                    actionTimer = 0f;

                    if (aiCurrentStrategy == 4)
                    {
                        UseUltimateSkill();
                    }
                    else
                    {
                        int cost = (aiCurrentStrategy == 2) ? skill2Cost : skill3Cost;
                        TryUseSkill("Skill" + aiCurrentStrategy, cost);
                    }

                    // Sau khi xả chiêu từ xa, 40% cơ hội lướt lùi lại tạo khoảng cách (thả diều)
                    if (Random.value < 0.4f && distanceToEnemy < 4f)
                    {
                        StartCoroutine(DashRoutine(-moveDirection));
                    }

                    aiCurrentStrategy = 0;
                }
            }
        }
    }

    [Header("Cấu Hình Độ Trễ Combo")]
    public float attackCooldown = 0.35f;
    private bool isAttacking = false;

    private void PerformComboAttack()
    {
        if (isAttacking) return;

        timeSinceLastAttack = 0f;
        comboStep++;

        if (comboStep == 1)
        {
            TryUseSkill("Attack1", skill1Cost);
            StartCoroutine(AttackCooldownRoutine());
        }
        else if (comboStep == 2)
        {
            TryUseSkill("Attack2", 0);
            StartCoroutine(AttackCooldownRoutine());
        }
        else if (comboStep >= 3)
        {
            TryUseSkill("Attack3", 0);
            StartCoroutine(AttackCooldownRoutine());
            comboStep = 0;
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void UseUltimateSkill()
    {
        if (TryUseSkill("Skill4", skill4Cost))
        {
            StartCoroutine(UltimateSuperArmorRoutine());
        }
    }

    private IEnumerator UltimateSuperArmorRoutine()
    {
        isCastingUltimate = true;
        yield return new WaitForSeconds(1.5f);
        isCastingUltimate = false;
    }

    public void TakeKnockback(Vector2 force, float stunTime)
    {
        if (isBlocking || isCastingUltimate) return;
        StartCoroutine(KnockbackRoutine(force, stunTime));
    }

    private IEnumerator KnockbackRoutine(Vector2 force, float stunTime)
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = force;
        if (anim != null) anim.SetTrigger("Hit");
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }

    private IEnumerator DashRoutine(float direction)
    {
        isDashing = true;
        if (anim != null) anim.SetTrigger("Dash");
        rb.linearVelocity = new Vector2(direction * dashForce, 0f);
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    void FindEnemyTarget()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) enemyTarget = p.transform;
    }

    bool TryUseSkill(string skillParameterName, int cost)
    {
        if (energySys != null)
        {
            if (energySys.UseEnergy(cost)) { anim.SetTrigger(skillParameterName); return true; }
            return false;
        }
        else { anim.SetTrigger(skillParameterName); return true; }
    }

    public void TriggerMeleeHitbox()
    {
        if (attackPoint == null) return;

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, meleeHitRange);
        bool hitSomeone = false;

        foreach (Collider2D obj in hitObjects)
        {
            if (obj.gameObject == this.gameObject) continue;

            EnemyHealth enemyHealth = obj.GetComponent<EnemyHealth>();
            if (enemyHealth != null && !obj.CompareTag(gameObject.tag))
            {
                enemyHealth.TakeDamage(meleeDamage);
                hitSomeone = true;
            }

            PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
            if (playerHealth != null && !obj.CompareTag(gameObject.tag))
            {
                playerHealth.TakeDamage(meleeDamage);
                hitSomeone = true;
            }
        }

        if (hitSomeone && energySys != null)
        {
            energySys.AddEnergy(energyGainOnHit);
        }

        if (hitSomeone && GameFeelManager.instance != null)
        {
            GameFeelManager.instance.TriggerHitStop(0.05f);
            GameFeelManager.instance.TriggerCameraShake(0.1f, 0.15f);
        }
    }

    public void SpawnSkill2Projectile() { SpawnBullet(skill2ProjectilePrefab, castPointSkill2); }
    public void SpawnSkill3Projectile() { SpawnBullet(skill3ProjectilePrefab, castPointSkill3); }
    public void SpawnSkill4Projectile() { SpawnBullet(skill4ProjectilePrefab, castPointSkill4); }

    private void SpawnBullet(GameObject bulletPrefab, Transform specificCastPoint)
    {
        Transform finalSpawnPoint = specificCastPoint != null ? specificCastPoint : castPoint;
        if (bulletPrefab == null || finalSpawnPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, finalSpawnPoint.position, Quaternion.identity);

        UltimatePushSkill projUlt = bullet.GetComponent<UltimatePushSkill>();
        if (projUlt != null)
        {
            if (transform.localScale.x < 0)
            {
                projUlt.speed = -Mathf.Abs(projUlt.speed);
                bullet.transform.localScale = new Vector3(-Mathf.Abs(bullet.transform.localScale.x), bullet.transform.localScale.y, bullet.transform.localScale.z);
            }
            else projUlt.speed = Mathf.Abs(projUlt.speed);
        }

        Projectile projNaruto = bullet.GetComponent<Projectile>();
        if (projNaruto != null)
        {
            if (transform.localScale.x < 0)
            {
                projNaruto.speed = -Mathf.Abs(projNaruto.speed);
                bullet.transform.localScale = new Vector3(-Mathf.Abs(bullet.transform.localScale.x), bullet.transform.localScale.y, bullet.transform.localScale.z);
            }
            else projNaruto.speed = Mathf.Abs(projNaruto.speed);
        }
        else
        {
            ProjectileMihawk projMihawk = bullet.GetComponent<ProjectileMihawk>();
            if (projMihawk != null)
            {
                if (transform.localScale.x < 0)
                {
                    projMihawk.speed = -Mathf.Abs(projMihawk.speed);
                    bullet.transform.localScale = new Vector3(-Mathf.Abs(bullet.transform.localScale.x), bullet.transform.localScale.y, bullet.transform.localScale.z);
                }
                else projMihawk.speed = Mathf.Abs(projMihawk.speed);
            }
        }
        bullet.tag = gameObject.tag;
    }
}