using UnityEngine;

public class ZenitsuAI : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Cấu Hình AI Tạm Thời")]
    public GameObject hitboxSword; 
    private float actionTimer = 0f;
    public float timeBetweenActions = 2.5f; // Cứ 2.5 giây AI tự tung 1 chiêu ngẫu nhiên

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        // Đảm bảo AI đứng im lúc vào trận
        if(anim != null) anim.SetFloat("Speed", 0f);
    }

    void Update()
    {
        // AI TỰ ĐỘNG ĐÁNH THEO THỜI GIAN (Không mượn phím bàn phím nữa)
        actionTimer += Time.deltaTime;
        if (actionTimer >= timeBetweenActions)
        {
            TriggerRandomSkill();
            actionTimer = 0f;
        }
    }

    // Hàm cho AI tự chọn chiêu ngẫu nhiên để test mạch hoạt ảnh
    void TriggerRandomSkill()
    {
        if (anim == null) return;

        int randomChoice = Random.Range(1, 5); // Ra số ngẫu nhiên từ 1 đến 4
        string skillName = "Skill" + randomChoice;
        
        anim.SetTrigger(skillName);
        rb.linearVelocity = Vector2.zero; // Khựng người lại gồng chiêu
        
        Debug.Log("🤖 AI Zenitsu tự động tung chiêu: " + skillName);
    }

    // Xử lý khi Kiếm của AI chém trúng Luffy
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Contains("Luffy") || collision.CompareTag("Player"))
        {
            Animator luffyAnim = collision.GetComponent<Animator>();
            if (luffyAnim != null)
            {
                luffyAnim.SetTrigger("Hit"); // Đấm Luffy giật người!
                Debug.Log("⚡ Kiếm của AI Zenitsu đã chém trúng Luffy!");
            }
        }
    }
}