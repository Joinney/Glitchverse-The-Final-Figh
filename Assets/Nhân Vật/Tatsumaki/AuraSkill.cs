using UnityEngine;

public class AuraSkill : MonoBehaviour
{
    [Header("Thông số Vòng Cầu")]
    public int damagePerTick = 100;
    public float tickRate = 0.5f;
    public float lifeTime = 5f;

    // ==========================================
    // 💡 LOGIC ÂM THANH CHUẨN Ý BẠN
    // ==========================================
    [Header("1. Âm thanh Xuất Chiêu (Phát ngay lập tức)")]
    [Tooltip("Giọng nói/Tiếng hô chiêu của nhân vật")]
    public AudioClip castSound;
    [Range(0f, 1f)] public float castVolume = 1f;

    [Tooltip("Tiếng Vòng Cầu bùng nổ khi vừa bật lên")]
    public AudioClip hitSound;
    [Range(0f, 1f)] public float hitVolume = 1f;

    [Header("2. Âm thanh Va Chạm (Chỉ phát khi địch dính vòng)")]
    [Tooltip("Tiếng 'Xẹt xẹt' giật điện mỗi 0.5 giây khi chạm vào địch")]
    public AudioClip impactSound;
    [Range(0f, 1f)] public float impactVolume = 0.8f;

    [Tooltip("Tiếng Keng khi bị đối thủ đỡ đòn")]
    public AudioClip blockSound;
    [Range(0f, 1f)] public float blockVolume = 0.8f;

    private float timer = 0f;

    void Start()
    {
        GameObject[] potentialMasters = GameObject.FindGameObjectsWithTag(gameObject.tag);
        Transform trueMaster = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject obj in potentialMasters)
        {
            if (obj == gameObject) continue;

            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                trueMaster = obj.transform;
            }
        }

        if (trueMaster != null) transform.SetParent(trueMaster);

        // ==========================================
        // 💡 PHÁT CẢ GIỌNG NÓI LẪN TIẾNG CHIÊU THỨC NGAY KHI VỪA BẬT
        // ==========================================
        Vector3 camPos = Camera.main != null ? Camera.main.transform.position : transform.position;

        if (castSound != null) AudioSource.PlayClipAtPoint(castSound, camPos, castVolume);
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, camPos, hitVolume);

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(gameObject.tag)) return;

        if (timer >= tickRate)
        {
            CharacterController2D targetController = other.GetComponent<CharacterController2D>();
            bool isBlocked = (targetController != null && targetController.isBlocking);
            Vector3 camPos = Camera.main != null ? Camera.main.transform.position : transform.position;

            // NẾU KẺ ĐỊCH ĐANG ĐỠ ĐÒN -> KÊU TIẾNG KENG RỒI BỎ QUA SÁT THƯƠNG
            if (isBlocked)
            {
                if (blockSound != null) AudioSource.PlayClipAtPoint(blockSound, camPos, blockVolume);
                return;
            }

            bool hitSomeone = false;

            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null) { enemy.TakeDamage(damagePerTick); hitSomeone = true; }

            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null) { player.TakeDamage(damagePerTick); hitSomeone = true; }

            // ==========================================
            // 💡 CHỈ PHÁT TIẾNG "XẸT ĐIỆN" VÀ RUNG MÀN HÌNH KHI ĐỊCH ĐANG TRONG VÒNG
            // ==========================================
            if (hitSomeone)
            {
                if (impactSound != null)
                {
                    AudioSource.PlayClipAtPoint(impactSound, camPos, impactVolume);
                }

                if (GameFeelManager.instance != null)
                {
                    GameFeelManager.instance.TriggerHitStop(0.02f);
                    GameFeelManager.instance.TriggerCameraShake(0.1f, 0.1f);
                }
            }
        }
    }

    void LateUpdate()
    {
        if (timer >= tickRate) timer = 0f;
    }
}