using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer = 0.6f;
    private Color textColor;
    private Vector3 moveVector;

    public static DamagePopup Create(Vector3 position, int damageAmount, bool isCritical = false)
    {
        GameObject popupObj = new GameObject("DamagePopup");
        popupObj.transform.position = position + new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0);

        DamagePopup damagePopup = popupObj.AddComponent<DamagePopup>();
        damagePopup.Setup(damageAmount, isCritical);

        return damagePopup;
    }

    private void Awake()
    {
        textMesh = gameObject.AddComponent<TextMeshPro>();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 5;
        textMesh.sortingOrder = 50; // Luôn hiển thị trên cùng
    }

    public void Setup(int damageAmount, bool isCritical)
    {
        textMesh.text = damageAmount.ToString();
        if (isCritical)
        {
            textMesh.fontSize = 7;
            textColor = new Color(1f, 0.2f, 0f); // Màu cam đỏ chí mạng
        }
        else
        {
            textMesh.fontSize = 5;
            textColor = Color.yellow; // Màu vàng sát thương thường
        }

        textMesh.color = textColor;
        moveVector = new Vector3(Random.Range(-0.5f, 0.5f), 2f, 0) * 1.5f;
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 4f * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            // Mờ dần rồi hủy
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}