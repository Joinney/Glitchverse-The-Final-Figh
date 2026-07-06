using UnityEngine;

public class ZenitsuController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Transform playerTransform; // Mắt radar dùng để định vị vị trí của Luffy

    [Header("Cấu Hình Phân Loại")]
    public bool isAI = false; 

    [Header("Di Chuyển & Cơ Động (Cả 2 phe)")]
    public float moveSpeed = 4f;
    public float jumpForce = 10f;
    public float dashForce = 15f;
    private float horizontalInput;

    [Header("Hệ Thống Chiến Đấu")]
    public GameObject hitboxSword; 

    [Header("Cấu Hình AI Dí Theo Mục Tiêu")]
    public float attackRange = 2f; // Khoảng cách đủ gần để dừng lại đánh (2 mét)
    public float timeBetweenActions = 2f; // Tốc độ xả chiêu của AI (cứ 2 giây một chiêu)
    private float actionTimer = 0f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Nếu là AI, tự động quét tìm mục tiêu có tên là "Player_Luffy" hoặc mang Tag "Player" trên sàn đấu
        if (isAI)
        {
            FindPlayerTarget();
        }
    }

    void Update()
    {
        // === KHÔNG CHƠI TỰ ĐỘNG - NGƯỜI CHƠI ĐIỀU KHIỂN BẰNG TAY ===
        if (!isAI)
        {
            HandlePlayerMovement();
        }
        // === HỆ THỐNG AI TỰ ĐỘNG DÍ VÀ ĐÁNH ===
        else
        {
            HandleAIMovementAndAttack();
        }
    }

// --- LOGIC ĐIỀU KHIỂN CỦA NGƯỜI CHƠI ---
    void HandlePlayerMovement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        // ĐỔI SỐ 5 THÀNH SỐ 1 TẠI ĐÂY
        if (horizontalInput > 0) transform.localScale = new Vector3(1, 1, 1); 
        else if (horizontalInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        if (Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            float dashDirection = transform.localScale.x > 0 ? 1 : -1;
            rb.linearVelocity = new Vector2(dashDirection * dashForce, rb.linearVelocity.y);
            anim.SetTrigger("Dash");
        }

        if (Input.GetKeyDown(KeyCode.U)) TriggerSkill("Skill1");
        if (Input.GetKeyDown(KeyCode.I)) TriggerSkill("Skill2");
        if (Input.GetKeyDown(KeyCode.O)) TriggerSkill("Skill3");
        if (Input.GetKeyDown(KeyCode.P)) TriggerSkill("Skill4");
    }

    // --- LOGIC SĂN ĐUỔI CỦA AI ---
    void HandleAIMovementAndAttack()
    {
        if (playerTransform == null)
        {
            FindPlayerTarget();
            anim.SetFloat("Speed", 0f);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // ĐỔI SỐ 5 THÀNH SỐ 1 TẠI ĐÂY CHO AI LUÔN
        if (playerTransform.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1); 
        else
            transform.localScale = new Vector3(-1, 1, 1); 

        if (distanceToPlayer > attackRange)
        {
            float moveDirection = playerTransform.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
            anim.SetFloat("Speed", 1f); 
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
            anim.SetFloat("Speed", 0f); 

            actionTimer += Time.deltaTime;
            if (actionTimer >= timeBetweenActions)
            {
                int randomChoice = Random.Range(1, 5); 
                TriggerSkill("Skill" + randomChoice);
                actionTimer = 0f;
            }
        }
    }

    void FindPlayerTarget()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void TriggerSkill(string skillName)
    {
        if (anim == null) return;
        anim.SetTrigger(skillName);
        if (isAI) Debug.Log("🤖 AI Zenitsu dí sát và tự xả chiêu: " + skillName);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != this.gameObject)
        {
            Animator targetAnim = collision.GetComponent<Animator>();
            if (targetAnim != null)
            {
                targetAnim.SetTrigger("Hit"); 
                Debug.Log("⚡ Đòn đánh trúng mục tiêu!");
            }
        }
    }
}