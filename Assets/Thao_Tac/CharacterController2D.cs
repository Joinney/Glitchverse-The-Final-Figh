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

    [Header("Cấu Hình Nhảy (Double Jump)")] // --- PHẦN MỚI THÊM VÀO ---
    public int maxJumps = 2; // Số lần nhảy tối đa (2 = double jump)
    private int jumpsRemaining;
    public Transform groundCheck; // Vị trí kiểm tra chạm đất (Kéo thả object ở dưới chân nhân vật vào đây)
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer; // Layer của mặt đất
    private bool isGrounded;

    [Header("Cấu Hình Lướt (Né Đòn)")]
    public float dashDuration = 0.25f;
    public bool isDashing = false;

    [Header("Năng Lượng Tiêu Hao & Hồi Phục")]
    public int skill1Cost = 0;
    public int skill2Cost = 25;
    public int skill3Cost = 50;
    public int skill4Cost = 100;
    public int energyGainOnHit = 15;
    private EnergySystem energySys;

    [Header("Cấu Hình Cận Chiến (Skill 1)")]
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
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        energySys = GetComponent<EnergySystem>();
        originalScale = transform.localScale;

        jumpsRemaining = maxJumps; // Cấp số lần nhảy ban đầu

        if (isAI) FindEnemyTarget();
    }

    void Update()
    {
        // --- KIỂM TRA CHẠM ĐẤT ---
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            // Nếu chạm đất và đang rơi xuống (velocity.y <= 0) thì reset số lần nhảy
            if (isGrounded && rb.linearVelocity.y <= 0.1f)
            {
                jumpsRemaining = maxJumps;
            }
        }

        if (!isAI) HandlePlayerInput();
        else HandleAILogic();
    }

    void HandlePlayerInput()
    {
        if (!canMoveAndFight)
        {
            if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

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

        // --- XỬ LÝ NHẢY CÓ GIỚI HẠN (DOUBLE JUMP) ---
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

        if (keyS1) TryUseSkill("Skill1", skill1Cost);
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

        if (distanceToEnemy < attackRange && Random.Range(0, 1000) < 5)
        {
            StartCoroutine(AIBlockRoutine());
            return;
        }

        if (actionTimer < timeBetweenActions)
        {
            // Sửa lại cách AI nhảy để nó cũng xài GroundCheck cho chuẩn
            if (Random.Range(0, 1000) < 3 && isGrounded && distanceToEnemy > attackRange)
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

        // ... Phần logic AI còn lại giữ nguyên ...
        if (aiCurrentStrategy == 0)
        {
            aiCurrentStrategy = Random.Range(1, 5);
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
                TryUseSkill("Skill1", skill1Cost);
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

    private IEnumerator AIBlockRoutine()
    {
        isBlocking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
            anim.SetBool("IsBlocking", true);
        }
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
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
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, meleeHitRange, enemyLayers);
        bool hitSomeone = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            CharacterController2D targetObj = enemy.GetComponent<CharacterController2D>();

            if (targetObj != null && targetObj.isDashing) continue;

            if (targetObj != null && targetObj.isBlocking) continue;

            EnemyHealth aiHealth = enemy.GetComponent<EnemyHealth>();
            if (aiHealth != null) { aiHealth.TakeDamage(meleeDamage); hitSomeone = true; }

            PlayerHealth playerHealth = enemy.GetComponent<PlayerHealth>();
            if (playerHealth != null) { playerHealth.TakeDamage(meleeDamage); hitSomeone = true; }
        }

        if (hitSomeone && energySys != null) energySys.AddEnergy(energyGainOnHit);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, meleeHitRange);

        // Vẽ thêm vòng tròn để dễ hình dung Ground Check dưới chân
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