using UnityEngine;
using System.Collections;

public class CoinPickup : MonoBehaviour
{
    public int coinValue = 1;
    private Transform playerTransform;
    private bool isAttracted = false;
    private float flySpeed = 10f;
    private bool isCollected = false; // ✨ Cờ chống kích hoạt nhặt 2 lần

    void Start()
    {
        transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float randomX = Random.Range(-1.5f, 1.5f);
            rb.linearVelocity = new Vector2(randomX, 3.5f);
        }

        StartCoroutine(SeekPlayerRoutine());
    }

    private IEnumerator SeekPlayerRoutine()
    {
        yield return new WaitForSeconds(0.4f);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            isAttracted = true;
        }
        else
        {
            yield return new WaitForSeconds(2.6f);
            CollectCoin();
        }
    }

    void Update()
    {
        if (isCollected) return;

        if (isAttracted && playerTransform != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position + Vector3.up * 0.8f, flySpeed * Time.deltaTime);
            flySpeed += Time.deltaTime * 15f;

            if (Vector3.Distance(transform.position, playerTransform.position + Vector3.up * 0.8f) < 0.5f)
            {
                CollectCoin();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CollectCoin();
        }
    }

    void CollectCoin()
    {
        if (isCollected) return; // Nếu đã nhặt rồi thì bỏ qua ngay
        isCollected = true;

        // Vô hiệu hóa Collider để không nhận thêm va chạm
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        PlayerPrefs.SetInt("PlayerCoins", currentCoins + coinValue);
        PlayerPrefs.Save();

        if (CoinHUDManager.instance != null)
        {
            CoinHUDManager.instance.TriggerAddCoinEffect(coinValue);
        }

        Destroy(gameObject);
    }
}