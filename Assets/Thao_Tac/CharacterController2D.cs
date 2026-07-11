using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Transform enemyTarget; // Radar tìm mục tiêu (nếu là AI)

    [Header("Cấu Hình Phân Loại")]
    public bool isAI = false; 

    [Header("Thông Số Thuộc Tính (Tự chỉnh theo từng nhân vật)")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float dashForce = 16f;
    public float attackRange = 2f; 
    public float timeBetweenActions = 2f; 

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
    [Tooltip("Điểm bắn chung mặc định cho các nhân vật cũ")]
    public Transform castPoint; 
    
    [Tooltip("Chỉ kéo vào nếu muốn Skill 2 có điểm xuất hiện riêng")]
    public Transform castPointSkill2; 
    [Tooltip("Chỉ kéo vào nếu muốn Skill 3 có điểm xuất hiện riêng")]
    public Transform castPointSkill3; 
    [Tooltip("Chỉ kéo vào nếu muốn Skill 4 có điểm xuất hiện riêng")]
    public Transform castPointSkill4; 

    public GameObject skill2ProjectilePrefab; 
    public GameObject skill3ProjectilePrefab; 
    public GameObject skill4ProjectilePrefab; 

    [Header("Trạng Thái Chiến Đấu")]
    private float actionTimer = 0f;
    private float horizontalInput;
    private Vector3 originalScale; 

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
            rb.linearVelocity = new Vector2(dashDirection * dashForce, rb.linearVelocity.y);
            if (anim != null) anim.SetTrigger("Dash");
        }

        if (Input.GetKeyDown(KeyCode.U)) TryUseSkill("Skill1", skill1Cost);
        if (Input.GetKeyDown(KeyCode.I)) TryUseSkill("Skill2", skill2Cost);
        if (Input.GetKeyDown(KeyCode.O)) TryUseSkill("Skill3", skill3Cost);
        if (Input.GetKeyDown(KeyCode.P)) TryUseSkill("Skill4", skill4Cost);
    }

    void HandleAILogic()
    {
        if (enemyTarget == null)
        {
            FindEnemyTarget();
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        float distanceToEnemy = Vector2.Distance(transform.position, enemyTarget.position);

        if (enemyTarget.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        if (distanceToEnemy > attackRange)
        {
            float moveDirection = enemyTarget.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
            if (anim != null) anim.SetFloat("Speed", 1f); 
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetFloat("Speed", 0f); 

            actionTimer += Time.deltaTime;
            if (actionTimer >= timeBetweenActions)
            {
                int randomSkillIndex = Random.Range(1, 5); 
                int cost = 0;
                
                switch(randomSkillIndex)
                {
                    case 1: cost = skill1Cost; break;
                    case 2: cost = skill2Cost; break;
                    case 3: cost = skill3Cost; break;
                    case 4: cost = skill4Cost; break;
                }

                TryUseSkill("Skill" + randomSkillIndex, cost);
                actionTimer = 0f;
            }
        }
    }

    void FindEnemyTarget()
    {
        string targetTag = gameObject.CompareTag("Enemy") ? "Player" : "Enemy";
        GameObject targetObj = GameObject.FindWithTag(targetTag);
        
        if (targetObj != null) enemyTarget = targetObj.transform;
    }

    void TryUseSkill(string skillParameterName, int cost)
    {
        if (energySys != null)
        {
            if (energySys.UseEnergy(cost)) anim.SetTrigger(skillParameterName);
        }
        else
        {
            anim.SetTrigger(skillParameterName);
        }
    }

    public void TriggerMeleeHitbox()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, meleeHitRange, enemyLayers);
        bool hitSomeone = false;

        foreach(Collider2D enemy in hitEnemies)
        {
            EnemyHealth aiHealth = enemy.GetComponent<EnemyHealth>();
            if (aiHealth != null)
            {
                aiHealth.TakeDamage(meleeDamage);
                hitSomeone = true;
            }
            
            PlayerHealth playerHealth = enemy.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(meleeDamage);
                hitSomeone = true;
            }
        }

        if (hitSomeone && energySys != null) energySys.AddEnergy(energyGainOnHit);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, meleeHitRange);
    }

    // GỌI HÀM VỚI ĐIỂM SPAWN TƯƠNG ỨNG
    public void SpawnSkill2Projectile() { SpawnBullet(skill2ProjectilePrefab, castPointSkill2); }
    public void SpawnSkill3Projectile() { SpawnBullet(skill3ProjectilePrefab, castPointSkill3); }
    public void SpawnSkill4Projectile() { SpawnBullet(skill4ProjectilePrefab, castPointSkill4); }

    // ĐÃ NÂNG CẤP: NHẬN THÊM BIẾN specificCastPoint
    private void SpawnBullet(GameObject bulletPrefab, Transform specificCastPoint)
    {
        // LOGIC FALLBACK: Nếu có điểm riêng thì xài điểm riêng, không có thì xài điểm chung (castPoint)
        Transform finalSpawnPoint = specificCastPoint != null ? specificCastPoint : castPoint;

        if (bulletPrefab == null || finalSpawnPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, finalSpawnPoint.position, Quaternion.identity);
        
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