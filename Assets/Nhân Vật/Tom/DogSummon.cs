using UnityEngine;

public class DogSummon : MonoBehaviour
{
    [Header("Thông số sát thương")]
    public int damage = 50;           // Lượng máu trừ đi khi chó cắn
    public float lifeTime = 1.5f;     // Thời gian chó tồn tại trên sân (1.5 giây)

    void Start()
    {
        // 1. Vừa xuất hiện là đếm ngược thời gian để tự biến mất
        Destroy(gameObject, lifeTime);
    }

    // 2. Bất kỳ ai chạm vào vùng kích hoạt của chó sẽ bị cắn
    void OnTriggerEnter2D(Collider2D other)
    {
        // Tránh trường hợp chó cắn luôn cả người gọi nó ra (Phe mình)
        if (other.CompareTag(gameObject.tag)) return; 

        // Nếu đối thủ là AI
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Nếu đối thủ là Player khác
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }
}