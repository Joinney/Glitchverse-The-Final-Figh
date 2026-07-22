using UnityEngine;

public class AuraSkill : MonoBehaviour
{
    [Header("Thông số Vòng Cầu")]
    public int damagePerTick = 100;      // Lượng máu trừ mỗi lần giật dame
    public float tickRate = 0.5f;       // Tốc độ giật dame (Cứ 0.5 giây giật 1 lần)
    public float lifeTime = 5f;         // Thời gian vòng cầu tồn tại

    private float timer = 0f;

    void Start()
    {
        // 1. Quét tìm chủ nhân (Những người cùng Tag)
        GameObject[] potentialMasters = GameObject.FindGameObjectsWithTag(gameObject.tag);
        Transform trueMaster = null;
        float minDistance = Mathf.Infinity;

        // Tìm nhân vật ở gần nhất
        foreach (GameObject obj in potentialMasters)
        {
            // Bắt buộc bỏ qua chính bản thân quả cầu!
            if (obj == gameObject) continue;

            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                trueMaster = obj.transform;
            }
        }

        // 2. Ép quả cầu làm con của chủ nhân để nó tự động đi theo
        if (trueMaster != null)
        {
            transform.SetParent(trueMaster);
        }

        // 3. Hẹn giờ tự hủy
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Không tự đốt máu bản thân hoặc đồng đội
        if (other.CompareTag(gameObject.tag)) return;

        timer += Time.deltaTime;

        // Giật máu liên tục theo nhịp
        if (timer >= tickRate)
        {
            // Đặt lại đồng hồ đếm ngược ngay lập tức để chuẩn bị cho nhịp giật tiếp theo
            timer = 0f;

            // --- KIỂM TRA MỤC TIÊU CÓ ĐANG ĐỠ ĐÒN KHÔNG ---
            CharacterController2D targetController = other.GetComponent<CharacterController2D>();

            if (targetController != null && targetController.isBlocking)
            {
                // Vì đây là SKILL 2 -> Đỡ đòn sẽ kháng 100% sát thương.
                // Bỏ qua và không gọi lệnh trừ máu ở dưới nữa.
                return;
            }

            // --- GÂY SÁT THƯƠNG (Nếu không đỡ) ---
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage(damagePerTick);

            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null) player.TakeDamage(damagePerTick);
        }
    }
}