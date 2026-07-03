using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator anim;

    [Header("Combo Settings")]
    private int comboStep = 0;        // Đếm xem đang ở đòn đánh thứ mấy
    private float lastAttackTime = 0f; // Thời gian bấm cú đánh trước
    public float comboResetWindow = 1f; // Quá 1 giây không bấm tiếp sẽ reset combo về 0

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.freezeRotation = true; 
    }

    void Update()
    {
        // 1. ĐỌC PHÍM DI CHUYỂN & GÁN HOẠT ẢNH RUN
        moveInput.x = Input.GetAxisRaw("Horizontal");
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveInput.x));
        }

        // Quay mặt nhân vật theo hướng di chuyển
        if (moveInput.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (moveInput.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // TỰ ĐỘNG RESET COMBO NẾU NGƯỜI CHƠI BẤM CHẬM QUÁ TIME WINDOW
        if (Time.time - lastAttackTime > comboResetWindow)
        {
            comboStep = 0;
        }

        // 2. BẤM PHÍM J ĐỂ ĐẤM COMBO (Attack1 -> Attack2)
        if (Input.GetKeyDown(KeyCode.J))
        {
            TriggerComboAttack();
        }

        // 3. BẤM PHÍM U ĐỂ TUNG CHIÊU CUỐI (Ultimate)
        if (Input.GetKeyDown(KeyCode.U))
        {
            TriggerUltimate();
        }

        // 4. PHÍM TEST NHANH TRÚNG ĐÒN / ĐỒNG TỬ (Nhấn H để test trúng chiêu, G để chết)
        if (Input.GetKeyDown(KeyCode.H)) { anim.SetTrigger("Hit"); }
        if (Input.GetKeyDown(KeyCode.G)) { anim.SetTrigger("Die"); }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    // HÀM XỬ LÝ COMBO ĐẤM LIÊN HOÀN
    void TriggerComboAttack()
    {
        if (anim == null) return;

        lastAttackTime = Time.time; // Ghi nhận thời gian bấm phím
        comboStep++;                // Tiến lên đòn tiếp theo

        if (comboStep == 1)
        {
            anim.SetTrigger("Attack"); // Kích hoạt Attack1 (Từ Idle chuyển qua)
            Debug.Log("Luffy: ĐẤM PHÁT THỨ 1!");
        }
        else if (comboStep == 2)
        {
            anim.SetTrigger("Attack"); // Kích hoạt Attack2 (Từ Attack1 chuyển tiếp qua)
            Debug.Log("Luffy: ĐẤM TIẾP PHÁT THỨ 2!");
            comboStep = 0;             // Đánh hết combo 2 đòn thì reset về 0
        }
    }

    // HÀM TUNG CHIÊU CUỐI ULTIMATE
    void TriggerUltimate()
    {
        if (anim != null)
        {
            anim.SetTrigger("Ultimate");
            Debug.Log("Luffy: TUNG CHIÊU CUỐI - UNTIL VŨ TRỤ!");
        }
    }
    // HÀM TỰ ĐỘNG KÍCH HOẠT KHI VÙNG ĐẤM CHẠM VÀO NGƯỜI ĐỐI THỦ
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem có phải va chạm với Zenitsu không (hoặc Tag là Enemy)
        if (collision.gameObject.name.Contains("Zenitsu") || collision.CompareTag("Enemy"))
        {
            // Lấy thành phần Animator của đối thủ để bắt nó giật người
            Animator enemyAnim = collision.GetComponent<Animator>();
            if (enemyAnim != null)
            {
                enemyAnim.SetTrigger("Hit"); // Kích hoạt ngay khối Luffy_TrungChieu của đối thủ!
                Debug.Log("💥 ĐẤM TRÚNG RỒI! ĐỐI THỦ BỊ DÍNH ĐÒN!");
            }
        }
    }
}
