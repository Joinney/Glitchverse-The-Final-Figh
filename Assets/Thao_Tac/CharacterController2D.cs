using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Transform enemyTarget;

    [Header("Cấu Hình Phân Loại")]
    public bool isAI = false;
    public int playerIndex = 1; // 1 = P1 (WASD), 2 = P2 (Mũi tên)

    [Header("Thông Số Thuộc Tính")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float dashForce = 25f;
    public float attackRange = 2f;
    public float timeBetweenActions = 2f;

    [Header("Cấu Hình Nhảy (Double Jump)")]
    public int maxJumps = 2;
    private int jumpsRemaining;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Cấu Hình Lướt (Né Đòn)")]
    public float dashDuration = 0.25f;
    public bool isDashing = false;

    [Header("Hệ Thống Combo Đánh Thường")]
    public int comboStep = 0;           // Đang ở đòn thứ mấy
    public float comboWindow = 0.8f;    // Thời gian cho phép gõ phím tiếp theo
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
    public float meleeHitRange = 0.8f;
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
    public bool canMoveAndFight = false;

    private float actionTimer = 0f;
    private float horizontalInput;
    private Vector3 originalScale;

    private int aiCurrentStrategy = 0;
    private bool aiIsRepositioning = false;

    void Start()
{
    // Luôn mở khóa di chuyển cho MapSinhTon
    CountdownManager.isCountdownFinished = true;

    anim = GetComponent<Animator>();
    rb = GetComponent<Rigidbody2D>();
    energySys = GetComponent<EnergySystem>();
    originalScale = transform.localScale;
    jumpsRemaining = maxJumps;

    if (isAI)
    {
        FindEnemyTarget();
        string difficulty = PlayerPrefs.GetString("GameDifficulty", "Normal");
        if (difficulty == "Hard")
        {
            timeBetweenActions = 0.3f;
            moveSpeed = moveSpeed * 1.5f;
        }
        else
        {
            timeBetweenActions = 1.8f;
        }
    }
}

    void Update()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (isGrounded && rb.linearVelocity.y <= 0.1f)
            {
                jumpsRemaining = maxJumps;
            }
        }

        // Bỏ qua kiểm tra đếm ngược nếu ở MapSinhTon, các map đối kháng còn lại vẫn khóa khi chưa xong đếm ngược
        bool isSurvivalMap = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("MapSinhTon");
        if (!isSurvivalMap && !CountdownManager.isCountdownFinished)
        {
            if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        // ==================================================
        // ĐẾM THỜI GIAN RỚT COMBO (Dành cho cả Player và AI)
        // ==================================================
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

    void HandlePlayerInput()
    {
        if (isDashing || isStunned) return;

        bool keyLeft = false, keyRight = false, keyJump = false, keyDash = false;
        bool keyS1 = false, keyS2 = false, keyS3 = false, keyS4 = false;
        bool keyBlock = false;

        if (playerIndex == 1)
        {
            keyLeft = Input.GetKey(KeyCode.A);
            keyRight = Input.GetKey(KeyCode.D);
            keyJump = Input.GetKeyDown(KeyCode.W);
            keyBlock = Input.GetKey(KeyCode.K);
            keyDash = Input.GetKeyDown(KeyCode.L);
            keyS1 = Input.GetKeyDown(KeyCode.U);
            keyS2 = Input.GetKeyDown(KeyCode.I);
            keyS3 = Input.GetKeyDown(KeyCode.O);
            keyS4 = Input.GetKeyDown(KeyCode.P);
        }
        else if (playerIndex == 2)
        {
            keyLeft = Input.GetKey(KeyCode.LeftArrow);
            keyRight = Input.GetKey(KeyCode.RightArrow);
            keyJump = Input.GetKeyDown(KeyCode.UpArrow);
            keyBlock = Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.Alpha2);
            keyDash = Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3);
            keyS1 = Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1);
            keyS2 = Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4);
            keyS3 = Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5);
            keyS4 = Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6);
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

        // --- GỌI HÀM COMBO ---
        if (keyS1) PerformComboAttack();
        if (keyS2) TryUseSkill("Skill2", skill2Cost);
        if (keyS3) TryUseSkill("Skill3", skill3Cost);
        if (keyS4) TryUseSkill("Skill4", skill4Cost);
    }

    void HandleAILogic()
    {
        if (isDashing || isStunned) return;

        if (isBlocking)
        {
            isBlocking = false;
            if (anim != null) anim.SetBool("IsBlocking", false);
        }

        if (enemyTarget == null)
        {
            FindEnemyTarget();
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        float distanceToEnemy = Vector2.Distance(transform.position, enemyTarget.position);
        float moveDirection = enemyTarget.position.x > transform.position.x ? 1f : -1f;

        if (!aiIsRepositioning)
        {
            transform.localScale = new Vector3(moveDirection * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        actionTimer += Time.deltaTime;

        string difficulty = PlayerPrefs.GetString("GameDifficulty", "Normal");
        int blockChance = (difficulty == "Hard") ? 150 : 10;

        if (distanceToEnemy < attackRange && Random.Range(0, 1000) < blockChance)
        {
            StartCoroutine(AIBlockRoutine());
            return;
        }

        if (actionTimer < timeBetweenActions)
        {
            if (Random.Range(0, 1000) < (difficulty == "Hard" ? 8 : 3) && isGrounded && distanceToEnemy > attackRange)
            {
                if (Random.value > 0.5f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    if (anim != null) anim.SetTrigger("Jump");
                }
                else
                {
                    StartCoroutine(DashRoutine(moveDirection));
                }
            }
            return;
        }

        if (aiCurrentStrategy == 0)
        {
            if (difficulty == "Hard")
            {
                if (distanceToEnemy <= meleeHitRange)
                {
                    if (Random.value < 0.7f) aiCurrentStrategy = 1;
                    else
                    {
                        StartCoroutine(DashRoutine(-moveDirection));
                        actionTimer = 0f;
                        return;
                    }
                }
                else if (distanceToEnemy > attackRange) aiCurrentStrategy = Random.value > 0.5f ? 2 : 3;
                else aiCurrentStrategy = Random.Range(1, 5);
            }
            else
            {
                aiCurrentStrategy = Random.Range(1, 5);
            }
            aiIsRepositioning = false;
        }

        if (aiCurrentStrategy == 1)
        {
            if (distanceToEnemy > meleeHitRange)
            {
                rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
                if (anim != null) anim.SetFloat("Speed", 1f);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (anim != null) anim.SetFloat("Speed", 0f);

                // --- ĐỂ CHO AI CŨNG BIẾT MÚA COMBO ---
                PerformComboAttack();

                aiCurrentStrategy = 0;
                actionTimer = 0f;
            }
        }
        else
        {
            if (distanceToEnemy > attackRange)
            {
                rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
                if (anim != null) anim.SetFloat("Speed", 1f);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (anim != null) anim.SetFloat("Speed", 0f);

                if ((aiCurrentStrategy == 3 || aiCurrentStrategy == 4) && !aiIsRepositioning && distanceToEnemy < attackRange - 0.5f)
                {
                    aiIsRepositioning = true;
                    StartCoroutine(DashRoutine(-moveDirection));
                    return;
                }

                int cost = 0;
                switch (aiCurrentStrategy)
                {
                    case 2: cost = skill2Cost; break;
                    case 3: cost = skill3Cost; break;
                    case 4: cost = skill4Cost; break;
                }

                bool castSuccess = TryUseSkill("Skill" + aiCurrentStrategy, cost);
                if (!castSuccess) aiCurrentStrategy = 1;
                else { aiCurrentStrategy = 0; actionTimer = 0f; }
                aiIsRepositioning = false;
            }
        }
    }

    [Header("Cấu Hình Độ Trễ Combo")]
    public float attackCooldown = 0.35f; // Thời gian khựng lại giữa mỗi đòn (giây)
    private bool isAttacking = false;   // Biến khóa để chống spam nhanh quá mức

    // ==========================================
    // HÀM MÚA COMBO ĐÁNH THƯỜNG (ĐÃ CÓ KHOÁ CHỜ)
    // ==========================================
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

    private IEnumerator AIBlockRoutine()
    {
        isBlocking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
            anim.SetBool("IsBlocking", true);
        }

        string difficulty = PlayerPrefs.GetString("GameDifficulty", "Normal");
        float blockDuration = (difficulty == "Hard") ? Random.Range(0.8f, 2.0f) : Random.Range(0.3f, 1.0f);

        yield return new WaitForSeconds(blockDuration);

        isBlocking = false;
        if (anim != null) anim.SetBool("IsBlocking", false);
    }

    public void TakeKnockback(Vector2 force, float stunTime)
    {
        if (isBlocking)
        {
            rb.linearVelocity = new Vector2(force.x * 0.2f, rb.linearVelocity.y);
            return;
        }

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
        string targetTag = gameObject.CompareTag("Enemy") ? "Player" : "Enemy";
        GameObject targetObj = GameObject.FindWithTag(targetTag);
        if (targetObj != null) enemyTarget = targetObj.transform;
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
            // 1. Không tự đánh chính mình
            if (obj.gameObject == this.gameObject) continue;

            // 2. Nếu là nhân vật đối kháng (có CharacterController2D), kiểm tra né đòn/đỡ đòn
            CharacterController2D targetObj = obj.GetComponent<CharacterController2D>();
            if (targetObj != null && (targetObj.isDashing || targetObj.isBlocking)) continue;

            // 3. Xử lý sát thương quái / đối thủ
            EnemyHealth enemyHealth = obj.GetComponent<EnemyHealth>();
            if (enemyHealth != null && !obj.CompareTag(gameObject.tag))
            {
                enemyHealth.TakeDamage(meleeDamage);
                hitSomeone = true;
            }

            // 4. Xử lý sát thương Player (khi ở chế độ 2 người chơi)
            PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
            if (playerHealth != null && !obj.CompareTag(gameObject.tag))
            {
                playerHealth.TakeDamage(meleeDamage);
                hitSomeone = true;
            }
        }

        // Hồi năng lượng khi trúng bất kỳ mục tiêu nào
        if (hitSomeone && energySys != null)
        {
            energySys.AddEnergy(energyGainOnHit);
        }

        // Hiệu ứng va chạm
        if (hitSomeone && GameFeelManager.instance != null)
        {
            GameFeelManager.instance.TriggerHitStop(0.05f);
            GameFeelManager.instance.TriggerCameraShake(0.1f, 0.15f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, meleeHitRange);

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
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