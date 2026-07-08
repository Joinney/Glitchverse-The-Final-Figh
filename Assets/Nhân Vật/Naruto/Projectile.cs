using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] 
    public float speed = 12f; 
    public float lifeTime = 3f;
    public int damage = 10;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Đẩy lực thẳng theo trục X
            rb.linearVelocity = new Vector2(speed, 0f);
        }
        
        // Tự hủy sau một khoảng thời gian
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chạm phe đối địch thì tính sát thương và tự hủy
        if ((gameObject.CompareTag("Player") && other.CompareTag("Enemy")) ||
            (gameObject.CompareTag("Enemy") && other.CompareTag("Player")))
        {
            Debug.Log($"[TRÚNG CHIÊU] Rasengan bắn trúng {other.name}!");
            Destroy(gameObject);
        }
    }
}