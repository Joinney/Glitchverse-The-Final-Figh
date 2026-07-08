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

    [Header("Cấu Hình Cận Chiến (Skill 1)")]
    public Transform attackPoint;      // Điểm vung đòn (Kéo Object Melee_AttackPoint vào đây)
    public float meleeHitRange = 0.8f; // Bán kính (độ to) của Hitbox cận chiến
    public LayerMask enemyLayers;      // Layer của Kẻ địch (Để đánh không bị trượt)
    public int meleeDamage = 15;       // Sát thương Chiêu 1

    [Header("Cấu Hình Hiệu Ứng Phóng Chiêu (Skill 2, 3, 4)")]
    public Transform castPoint; // Kéo Object tay nhân vật vào đây
    public GameObject skill2ProjectilePrefab; // Prefab đạn Skill 2 (Rasengan)
    public GameObject skill3ProjectilePrefab; // Prefab đạn Skill 3
    public GameObject skill4ProjectilePrefab; // Prefab đạn Skill 4 (Ultimate)

    [Header("Trạng Thái Chiến Đấu")]
    private float actionTimer = 0f;
    private float horizontalInput;

    // Tự động lấy kích thước chuẩn ban đầu của nhân vật ngoài Inspector
    private Vector3 originalScale; 

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        // Ghi nhớ Scale gốc được cấu hình trên Prefab (Đảm bảo không bị co giãn lỗi)
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
    // 🎮 ĐIỀU KHIỂN BẰNG TAY (CHO NGƯỜI CHƠI - BẤM PHÍM CHUNG)
    // =================================================================
    void HandlePlayerInput()
    {
        // 1. Di chuyển Trục Ngang (A/D hoặc Mũi tên)
        horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        
        // Gọi tham số "Speed" chung (Tất cả nhân vật đều dùng chung tên này)
        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        // Lật mặt dựa vào thuộc tính ban đầu
        if (horizontalInput > 0) 
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else if (horizontalInput < 0) 
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        // 2. Nút Nhảy (Space)
        if (Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (anim != null) anim.SetTrigger("Jump");
        }

        // 3. Nút Dash Lướt nhanh (Phím L)
        if (Input.GetKeyDown(KeyCode.L))
        {
            float dashDirection = transform.localScale.x > 0 ? 1 : -1;
            rb.linearVelocity = new Vector2(dashDirection * dashForce, rb.linearVelocity.y);
            if (anim != null) anim.SetTrigger("Dash");
        }

        // 4. Hệ thống Phím Kỹ Năng Đồng Bộ (U, I, O, P)
        if (Input.GetKeyDown(KeyCode.U)) TriggerSkill("Skill1");
        if (Input.GetKeyDown(KeyCode.I)) TriggerSkill("Skill2");
        if (Input.GetKeyDown(KeyCode.O)) TriggerSkill("Skill3");
        if (Input.GetKeyDown(KeyCode.P)) TriggerSkill("Skill4");
    }

    // =================================================================
    // 🤖 ĐIỀU KHIỂN TỰ ĐỘNG (CHO AI ĐỐI THỦ - DÍ THEO MỤC TIÊU)
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

        // AI tự động lật mặt hướng về phía đối thủ
        if (enemyTarget.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        // HÀNH ĐỘNG 1: Ở xa -> Chạy dí theo
        if (distanceToEnemy > attackRange)
        {
            float moveDirection = enemyTarget.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
            if (anim != null) anim.SetFloat("Speed", 1f); 
        }
        // HÀNH ĐỘNG 2: Vào tầm đánh -> Dừng lại và tự xả Skill ngẫu nhiên
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetFloat("Speed", 0f); 

            actionTimer += Time.deltaTime;
            if (actionTimer >= timeBetweenActions)
            {
                int randomSkillIndex = Random.Range(1, 5); 
                TriggerSkill("Skill" + randomSkillIndex);
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

    void TriggerSkill(string skillParameterName)
    {
        if (anim == null) return;
        anim.SetTrigger(skillParameterName);
    }

    // =================================================================
    // 🎬 ANIMATION EVENTS (GỌI TỰ ĐỘNG KHI VUNG TAY NÉM CHIÊU)
    // =================================================================
    
    // --- SKILL 1: CẬN CHIẾN (Gài Event vào hoạt ảnh chém/đấm) ---
    public void TriggerMeleeHitbox()
    {
        if (attackPoint == null) return;

        // Tung ra một vòng tròn tàng hình quét trúng ai nằm trong Layer địch
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, meleeHitRange, enemyLayers);

        foreach(Collider2D enemy in hitEnemies)
        {
            // Nếu bạn đấm AI -> Trừ máu AI
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(meleeDamage);
            
            // Nếu AI đấm bạn -> Trừ máu bạn
            enemy.GetComponent<PlayerHealth>()?.TakeDamage(meleeDamage);
        }
    }

    // Hàm vẽ vòng tròn Hitbox màu đỏ trên Scene để căn chỉnh cho dễ
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, meleeHitRange);
    }

    // --- SKILL 2, 3, 4: BẮN ĐẠN XA ---
    public void SpawnSkill2Projectile()
    {
        SpawnBullet(skill2ProjectilePrefab);
    }

    public void SpawnSkill3Projectile()
    {
        SpawnBullet(skill3ProjectilePrefab);
    }

    public void SpawnSkill4Projectile()
    {
        SpawnBullet(skill4ProjectilePrefab);
    }

    private void SpawnBullet(GameObject bulletPrefab)
    {
        if (bulletPrefab == null || castPoint == null) return;

        // 1. Sinh ra quả cầu tại đúng vị trí tay CastPoint, giữ nguyên góc xoay mặc định
        GameObject bullet = Instantiate(bulletPrefab, castPoint.position, Quaternion.identity);
        
        // 2. Lấy script di chuyển của đạn ra để cấu hình hướng bay
        Projectile projectileScript = bullet.GetComponent<Projectile>();
        
        if (projectileScript != null)
        {
            // Kiểm tra hướng mặt hiện tại của Naruto qua Scale X
            if (transform.localScale.x < 0)
            {
                // Nếu Naruto quay trái: Ép tốc độ đạn mang dấu ÂM để bay sang trái
                projectileScript.speed = -Mathf.Abs(projectileScript.speed);
                
                // Lật ngược hình ảnh quả cầu lại cho đúng hướng
                bullet.transform.localScale = new Vector3(-Mathf.Abs(bullet.transform.localScale.x), bullet.transform.localScale.y, bullet.transform.localScale.z);
            }
            else
            {
                // Nếu Naruto quay phải: Ép tốc độ đạn mang dấu DƯƠNG để bay sang phải
                projectileScript.speed = Mathf.Abs(projectileScript.speed);
            }
        }
        
        // 3. Gán tag đồng bộ để tính sát thương
        bullet.tag = gameObject.tag;
    }
}