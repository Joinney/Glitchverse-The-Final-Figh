using UnityEngine;
using UnityEngine.SceneManagement;

public class BossStageTrigger : MonoBehaviour
{
    [Header("1. Cấu Hình Chuyển Cảnh")]
    public string fightStageSceneName = "Fight_Stage1";
    public float triggerDistance = 2.0f; // Khoảng cách Player chạy tới gần là chuyển màn

    private Transform playerTransform;
    private bool isTriggered = false;

    private void Start()
    {
        // 1. TẮT HOÀN TOÀN CharacterController2D và EnemyHealth trên Boss để tránh bị điều khiển/bắt chước
        CharacterController2D charCtrl = GetComponent<CharacterController2D>();
        if (charCtrl != null)
        {
            charCtrl.isAI = true; // Đặt là true để không bao giờ nhận phím bấm của người chơi
            charCtrl.enabled = false;
        }

        EnemyHealth eHealth = GetComponent<EnemyHealth>();
        if (eHealth != null)
        {
            eHealth.enabled = false; // Tắt luôn máu để không kích hoạt hàm StunRoutine bật lại script di chuyển
        }

        // 2. Khóa vị trí đứng yên trên nền đất
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 3. Đảm bảo đứng dáng Idle
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
        }

        // 4. Tìm đối tượng Player
        FindPlayer();
    }

    private void Update()
    {
        if (isTriggered) return;

        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // 💥 KIỂM TRA KHOẢNG CÁCH: Chạy tới gần là tự động chuyển Scene ngay lập tức!
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= triggerDistance)
        {
            EnterBossFight();
        }
    }

    // Dự phòng va chạm vật lý / Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) EnterBossFight();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) EnterBossFight();
    }

    private void EnterBossFight()
    {
        if (isTriggered) return;
        isTriggered = true;

        Debug.Log("🎯 Đã chạm trán Boss! Đang vào màn đấu: " + fightStageSceneName);
        SceneManager.LoadScene(fightStageSceneName);
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    // Vẽ vòng bán kính nhận diện trong tab Scene để dễ quan sát
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}