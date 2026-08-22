using UnityEngine;
using UnityEngine.SceneManagement;

public class BossStageTrigger : MonoBehaviour
{
    [Header("1. Cấu Hình Chuyển Cảnh")]
    public string fightStageSceneName = "Fight_Stage1";
    public float triggerDistance = 2.0f;

    private Transform playerTransform;
    private bool isTriggered = false;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        // 1. Tắt AI và máu trên bản sao Trigger
        CharacterController2D charCtrl = GetComponent<CharacterController2D>();
        if (charCtrl != null)
        {
            charCtrl.isAI = true;
            charCtrl.enabled = false;
        }

        EnemyHealth eHealth = GetComponent<EnemyHealth>();
        if (eHealth != null)
        {
            eHealth.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 2. Tắt toàn bộ FlipX để không bị lật kép
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer r in allRenderers)
        {
            r.flipX = false;
        }

        // 3. ÉP SCALE X ÂM ĐỂ QUAY MẶT SANG TRÁI NHÌN PLAYER
        ForceFaceLeft();

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
        }

        FindPlayer();
    }

    private void LateUpdate()
    {
        // Giữ vững tư thế quay mặt sang trái, chống Animation ghi đè Scale
        ForceFaceLeft();
    }

    private void ForceFaceLeft()
    {
        float targetScaleX = -Mathf.Abs(originalScale.x);
        if (transform.localScale.x != targetScaleX)
        {
            transform.localScale = new Vector3(targetScaleX, originalScale.y, originalScale.z);
        }
    }

    private void Update()
    {
        if (isTriggered) return;

        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // Chạy tới gần là chuyển sang Scene đấu Boss ngay lập tức
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= triggerDistance)
        {
            EnterBossFight();
        }
    }

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

        Debug.Log("🎯 Đã chạm trán Boss! Vào trận đấu: " + fightStageSceneName);
        SceneManager.LoadScene(fightStageSceneName);
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}