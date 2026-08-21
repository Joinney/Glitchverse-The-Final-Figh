using UnityEngine;
using System.Collections;

public class MinionMonsterAI : MonoBehaviour
{
    public enum MonsterType { Skeleton, Goblin, DarkWolf, Custom }

    [Header("1. Chọn Loại Quái (Tự khớp Animation)")]
    public MonsterType monsterType = MonsterType.Skeleton;

    [Header("2. Tên Các State Animation (Khớp 100% với Animator)")]
    public string idleAnim = "Skeleton_Idle";
    public string walkAnim = "Skeleton_Walking";
    public string runAnim = "Skeleton_Running";
    public string attackAnim = "Skeleton_Attack";
    public string dyingAnim = "Skeleton_Dying";

    [Header("3. Chỉ Số Di Chuyển & Đuổi Bắt")]
    public float moveSpeed = 3.5f;
    public float detectRange = 10f;
    public float attackRange = 1.6f;
    public float attackCooldown = 1.4f;
    private float lastAttackTime = 0f;

    [Header("4. Sát Thương Đòn Đánh")]
    public int attackDamage = 25;
    public Transform attackPoint;
    public float hitRadius = 0.8f;
    public float damageDelay = 0.25f;
    public float attackAnimationDuration = 0.6f; // Thời gian phát trọn vẹn animation đánh

    [Header("5. Hiệu Ứng Bị Đẩy Lùi (Knockback)")]
    public float knockbackDuration = 0.15f;
    private bool isKnockedBack = false;

    [Header("6. Trạng Thái")]
    public bool isDead = false;
    private bool isAttacking = false; // Khóa animation khi đang tung đòn

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform playerTarget;
    private Vector3 originalScale;
    private string currentPlayingAnim = "";

    void Awake()
    {
        if (monsterType == MonsterType.Skeleton)
        {
            idleAnim = "Skeleton_Idle";
            walkAnim = "Skeleton_Walking";
            runAnim = "Skeleton_Running";
            attackAnim = "Skeleton_Attack";
            dyingAnim = "Skeleton_Dying";
        }
        else if (monsterType == MonsterType.Goblin)
        {
            idleAnim = "Goblin_Idle";
            walkAnim = "Goblin_Walking";
            runAnim = "Goblin_Running";
            attackAnim = "Goblin_Attack";
            dyingAnim = "Goblin_Dying";
        }
        else if (monsterType == MonsterType.DarkWolf)
        {
            idleAnim = "Idle";
            walkAnim = "Walk";
            runAnim = "Run";
            attackAnim = "Attack";
            dyingAnim = "Death";
        }
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        FindPlayer();
    }

    void LateUpdate()
    {
        if (isDead) return;
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        if (isDead || isKnockedBack || isAttacking) return;

        if (playerTarget == null)
        {
            FindPlayer();
            PlayAnim(idleAnim);
            return;
        }

        float deltaX = playerTarget.position.x - transform.position.x;
        float distance = Vector2.Distance(transform.position, playerTarget.position);

        // 1. Xoay mặt đa hướng (Hỗ trợ cả Scale lẫn FlipX)
        if (Mathf.Abs(deltaX) > 0.15f)
        {
            float direction = deltaX > 0 ? 1f : -1f;
            transform.localScale = new Vector3(direction * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

            if (spriteRenderer != null)
            {
                // Nếu sprite gốc quay mặt sang trái thì đảo dấu: deltaX < 0
                spriteRenderer.flipX = (deltaX < 0);
            }
        }

        // 2. Tiếp cận và Tấn công
        if (distance <= detectRange)
        {
            float moveDir = deltaX > 0 ? 1f : -1f;

            if (distance > attackRange)
            {
                rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
                PlayAnim(runAnim);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    StartCoroutine(AttackRoutine());
                }
                else
                {
                    PlayAnim(idleAnim);
                }
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            PlayAnim(idleAnim);
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Ép phát animation đánh dứt khoát
        currentPlayingAnim = "";
        PlayAnim(attackAnim);

        // Chờ thời điểm vung móng vuốt/cắn trúng mục tiêu
        yield return new WaitForSeconds(damageDelay);

        if (!isDead && playerTarget != null)
        {
            Vector2 checkPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, hitRadius);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player") || (hit.transform.parent != null && hit.transform.parent.CompareTag("Player")))
                {
                    PlayerHealth pHealth = hit.GetComponentInParent<PlayerHealth>();
                    if (pHealth != null)
                    {
                        pHealth.TakeDamage(attackDamage);
                        break;
                    }
                }
            }
        }

        // Chờ diễn nốt phần còn lại của animation đánh trước khi quay lại chạy/đứng
        float remainingTime = attackAnimationDuration - damageDelay;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        isAttacking = false;
    }

    public void TakeKnockback(Vector2 force)
    {
        if (isDead) return;
        StartCoroutine(KnockbackRoutine(force));
    }

    private IEnumerator KnockbackRoutine(Vector2 force)
    {
        isKnockedBack = true;
        isAttacking = false; // Ngắt đòn đánh nếu bị người chơi phản công
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = force;
        }

        yield return new WaitForSeconds(knockbackDuration);

        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isKnockedBack = false;
    }

    void PlayAnim(string animName)
    {
        if (anim != null && currentPlayingAnim != animName && !string.IsNullOrEmpty(animName))
        {
            anim.Play(animName, 0, 0f); // Luôn phát từ frame đầu tiên
            currentPlayingAnim = animName;
        }
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTarget = p.transform;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        isAttacking = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        currentPlayingAnim = "";
        PlayAnim(dyingAnim);
        Destroy(gameObject, 1.2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 checkPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Gizmos.DrawWireSphere(checkPos, hitRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}