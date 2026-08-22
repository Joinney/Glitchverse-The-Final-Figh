using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("Khoảng cách trên đầu")]
    public Vector3 offset = new Vector3(0, 1.6f, 0);

    private Slider slider;
    private Transform targetTransform;
    private Vector3 initialScale;

    public void Setup(Transform target, float currentHealth, float maxHealth, Vector3 customOffset)
    {
        targetTransform = target;
        offset = customOffset;

        // Tạo Canvas World Space động
        GameObject canvasObj = new GameObject("HealthBar_Canvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = offset;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 40;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1.2f, 0.25f);
        canvasRect.localScale = Vector3.one;

        // Tạo Background thanh máu (Màu xám/đen)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Tạo Fill Area và Fill (Màu đỏ/xanh)
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(canvasObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-0.04f, -0.04f);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0.9f, 0.15f, 0.15f, 1f); // Màu đỏ máu
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        // Gắn Slider component
        slider = canvasObj.AddComponent<Slider>();
        slider.targetGraphic = fillImg;
        slider.fillRect = fillRect;
        slider.minValue = 0;
        slider.maxValue = maxHealth;
        slider.value = currentHealth;

        initialScale = canvasRect.localScale;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (slider != null)
        {
            slider.maxValue = maxHealth;
            slider.value = currentHealth;
        }
    }

    void LateUpdate()
    {
        if (targetTransform != null)
        {
            // Luôn đi theo vị trí của quái
            transform.position = targetTransform.position + offset;

            // Chống bị xoay lật ngược khi quái quay mặt trái/phải
            transform.rotation = Quaternion.identity;
            transform.localScale = initialScale;
        }
    }
}