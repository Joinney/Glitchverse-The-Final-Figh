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
    public float attackRange = 2f; // Tầm nhìn AI bắt đầu dừng lại xả chiêu
    public float timeBetweenActions = 2f; 

    [Header("Năng Lượng Tiêu Hao & Hồi Phục (NEW)")]
    public int skill1Cost = 0;   // Đánh thường không tốn năng lượng
    public int skill2Cost = 25;  // Tốn 25 Mana
    public int skill3Cost = 50;  // Tốn 50 Mana
    public int skill4Cost = 100; // Chiêu cuối tốn 100 Mana (Đầy thanh)
    public int energyGainOnHit = 15; // Đánh thường (Skill 1) TRÚNG ĐỊCH sẽ hồi 15 Mana
    private EnergySystem energySys; // Liên kết với file EnergySystem

    [Header("Cấu Hình Cận Chiến (Skill 1)")]
    public Transform attackPoint;      
    public float meleeHitRange = 0.8f; 
    public LayerMask enemyLayers;      
    public int meleeDamage = 35;       

    [Header("Cấu Hình Hiệu Ứng Phóng Chiêu (Skill 2, 3, 4)")]
    public Transform castPoint; 
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
        
        // Bắt lấy hệ thống Năng Lượng đang gắn trên người nhân vật
        energySys = GetComponent<EnergySystem>(); 

        originalScale = transform.localScale; 

        if (isAI)
        {
            FindEnemyTarget();
        }
    }

    void Update()
    {
        if (!isAI)
        {
            HandlePlayerInput();
        }
        else
        {
            HandleAILogic();
        }
    }

    // =================================================================
    // 🎮 ĐIỀU KHIỂN BẰNG TAY (CHO NGƯỜI CHƠI)
    // =================================================================
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

        // Đã sửa: Thay vì xả chiêu ngay, sẽ đưa vào hàm TryUseSkill để check Mana trước
        if (Input.GetKeyDown(KeyCode.U)) TryUseSkill("Skill1", skill1Cost);
        if (Input.GetKeyDown(KeyCode.I)) TryUseSkill("Skill2", skill2Cost);
        if (Input.GetKeyDown(KeyCode.O)) TryUseSkill("Skill3", skill3Cost);
        if (Input.GetKeyDown(KeyCode.P)) TryUseSkill("Skill4", skill4Cost);
    }

    // =================================================================
    // 🤖 ĐIỀU KHIỂN TỰ ĐỘNG (CHO AI ĐỐI THỦ)
    // =================================================================
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
                // Máy ngẫu nhiên chọn chiêu
                int randomSkillIndex = Random.Range(1, 5); 
                int cost = 0;
                
                // Trích xuất đúng số mana cần thiết cho chiêu đó
                switch(randomSkillIndex)
                {
                    case 1: cost = skill1Cost; break;
                    case 2: cost = skill2Cost; break;
                    case 3: cost = skill3Cost; break;
                    case 4: cost = skill4Cost; break;
                }

                // Nếu có đủ Mana thì xả chiêu, nếu thiếu Mana máy sẽ đứng im chờ lượt sau (hoặc bạn có thể cho nó tự động xài Skill 1 thay thế)
                TryUseSkill("Skill" + randomSkillIndex, cost);
                actionTimer = 0f;
            }
        }
    }

    void FindEnemyTarget()
    {
        string targetTag = gameObject.CompareTag("Enemy") ? "Player" : "Enemy";
        GameObject targetObj = GameObject.FindWithTag(targetTag);
        
        if (targetObj != null)
        {
            enemyTarget = targetObj.transform;
        }
    }

    // --- HÀM MỚI: KIỂM TRA MÀNG LỌC NĂNG LƯỢNG TRƯỚC KHI ĐÁNH ---
    void TryUseSkill(string skillParameterName, int cost)
    {
        // Nếu nhân vật có gắn ống Năng lượng
        if (energySys != null)
        {
            // Hàm UseEnergy sẽ trả về TRUE nếu trừ mana thành công
            if (energySys.UseEnergy(cost))
            {
                anim.SetTrigger(skillParameterName);
            }
        }
        else
        {
            // Nếu lỡ quên gắn ống năng lượng thì vẫn cho xả chiêu bình thường để không bị lỗi game
            anim.SetTrigger(skillParameterName);
        }
    }

    // =================================================================
    // 🎬 ANIMATION EVENTS (GỌI TỰ ĐỘNG KHI VUNG TAY NÉM CHIÊU)
    // =================================================================
    
    public void TriggerMeleeHitbox()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, meleeHitRange, enemyLayers);
        
        // Đếm xem có chém trúng ai không
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

        // NẾU ĐÁNH TRÚNG ĐỊCH -> HỒI NĂNG LƯỢNG!
        if (hitSomeone && energySys != null)
        {
            energySys.AddEnergy(energyGainOnHit);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, meleeHitRange);
    }

    public void SpawnSkill2Projectile() { SpawnBullet(skill2ProjectilePrefab); }
    public void SpawnSkill3Projectile() { SpawnBullet(skill3ProjectilePrefab); }
    public void SpawnSkill4Projectile() { SpawnBullet(skill4ProjectilePrefab); }

    private void SpawnBullet(GameObject bulletPrefab)
    {
        if (bulletPrefab == null || castPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, castPoint.position, Quaternion.identity);
        
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