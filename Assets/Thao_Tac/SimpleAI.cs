using UnityEngine;

public class SimpleAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private float nextAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // Tự động quét tìm người chơi dựa trên tên Object được sinh ra từ BattleManager
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            // Dự phòng nếu không có Tag thì tìm theo tên cụm
            playerObj = GameObject.Find("Player 1_Luffy") ?? GameObject.Find("Player 1_Zenitsu");
        }

        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Tính khoảng cách tới Người chơi
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > attackRange)
        {
            // Đi theo hướng người chơi
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

            // Quay mặt AI về hướng di chuyển
            if (direction.x > 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (direction.x < 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            // Đến khoảng cách đánh $\rightarrow$ Đứng lại và vung đòn
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (Time.time >= nextAttackTime)
            {
                AIAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void AIAttack()
    {
        Debug.Log("AI Zenitsu tung chiêu: LÔI THẦN TỐC ĐỘ!");
    }
}