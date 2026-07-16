using UnityEngine;
using System.Collections; 

public class CharacterController2D : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Transform enemyTarget;

    [Header("Cấu Hình Phân Loại")]
    public bool isAI = false; 

    [Header("Thông Số Thuộc Tính")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float dashForce = 25f; 
    public float attackRange = 2f; 
    public float timeBetweenActions = 2f; 

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
    private float actionTimer = 0f;
    private float horizontalInput;
    private Vector3 originalScale; 
    
    // --- BIẾN MỚI: CHOÁNG ---
    public bool isStunned = false; 

    private int aiCurrentStrategy = 0; 
    private bool aiIsRepositioning = false; 

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        energySys = GetComponent<EnergySystem>(); 
        originalScale = transform.localScale; 

        if (isAI) FindEnemyTarget();
    }

    void Update()
    {
        if (!isAI) HandlePlayerInput();
        else HandleAILogic();
    }

    void HandlePlayerInput()
    {
        // 🔒 KHÓA ĐIỀU KHIỂN: Khi đang lướt hoặc BỊ CHOÁNG (Hất tung)
        if (isDashing || isStunned) return; 

        horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        
        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        if (horizontalInput > 0) 
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else if (horizontalInput < 0) 
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        if (Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (anim != null) anim.SetTrigger("Jump");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            float dashDirection = transform.localScale.x > 0 ? 1 : -1;
            StartCoroutine(DashRoutine(dashDirection));
        }

        if (Input.GetKeyDown(KeyCode.U)) TryUseSkill("Skill1", skill1Cost);
        if (Input.GetKeyDown(KeyCode.I)) TryUseSkill("Skill2", skill2Cost);
        if (Input.GetKeyDown(KeyCode.O)) TryUseSkill("Skill3", skill3Cost);
        if (Input.GetKeyDown(KeyCode.P)) TryUseSkill("Skill4", skill4Cost);
    }

    void HandleAILogic()
    {
        // 🔒 KHÓA ĐIỀU KHIỂN AI: AI cũng bất động khi bị hất tung
        if (isDashing || isStunned) return;

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

        if (actionTimer < timeBetweenActions)
        {
            if (Random.Range(0, 1000) < 3 && Mathf.Abs(rb.linearVelocity.y) < 0.1f && distanceToEnemy > attackRange)
            {
                if (Random.value > 0.5f) {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); 
                    if (anim != null) anim.SetTrigger("Jump");
                } else {
                    StartCoroutine(DashRoutine(moveDirection));
                }
            }
            return;
        }

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
                switch(aiCurrentStrategy)
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

    // =================================================================
    // 💥 CƠ CHẾ NHẬN LỰC HẤT TUNG & BỊ CHOÁNG
    // =================================================================
    public void TakeKnockback(Vector2 force, float stunTime)
    {
        StartCoroutine(KnockbackRoutine(force, stunTime));
    }

    private IEnumerator KnockbackRoutine(Vector2 force, float stunTime)
    {
        isStunned = true; // Bật cờ choáng, vô hiệu hóa nút bấm và AI

        // Hủy vận tốc rơi hiện tại để đảm bảo lực hất lên luôn chính xác
        rb.linearVelocity = Vector2.zero; 
        
        // Truyền lực hất văng đi
        rb.linearVelocity = force;

        // [ĐÃ SỬA TẠI ĐÂY] Kích hoạt Trigger "Hit" có sẵn trong Animator của bạn
        if (anim != null) anim.SetTrigger("Hit");

        // Chờ hết thời gian bị khóa
        yield return new WaitForSeconds(stunTime);

        isStunned = false; // Phục hồi trạng thái bình thường
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

        foreach(Collider2D enemy in hitEnemies)
        {
            CharacterController2D targetObj = enemy.GetComponent<CharacterController2D>();
            if (targetObj != null && targetObj.isDashing) continue; 

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
    }

    public void SpawnSkill2Projectile() { SpawnBullet(skill2ProjectilePrefab, castPointSkill2); }
    public void SpawnSkill3Projectile() { SpawnBullet(skill3ProjectilePrefab, castPointSkill3); }
    public void SpawnSkill4Projectile() { SpawnBullet(skill4ProjectilePrefab, castPointSkill4); }

    private void SpawnBullet(GameObject bulletPrefab, Transform specificCastPoint)
    {
        Transform finalSpawnPoint = specificCastPoint != null ? specificCastPoint : castPoint;
        if (bulletPrefab == null || finalSpawnPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, finalSpawnPoint.position, Quaternion.identity);
        
        // Cập nhật để hỗ trợ Đạn Hất Tung Mới (UltimatePushSkill)
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