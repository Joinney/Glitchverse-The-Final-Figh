using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundAutoFit : MonoBehaviour
{
    private SpriteRenderer sr;
    private Camera cam;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        FitToScreen();
    }

    void LateUpdate()
    {
        // Neo vị trí ảnh nền luôn trượt theo trục X của Camera
        if (cam != null)
        {
            transform.position = new Vector3(cam.transform.position.x, transform.position.y, transform.position.z);
        }
    }

    public void FitToScreen()
    {
        if (cam == null) cam = Camera.main;
        if (sr == null || sr.sprite == null || cam == null) return;

        // Reset scale về 1
        transform.localScale = Vector3.one;

        // Kích thước ảnh gốc
        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;

        // Chiều cao và chiều rộng của khung nhìn Camera trong thế giới 2D
        float worldScreenHeight = cam.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        // Tính tỉ lệ co giãn để phủ kín hoàn toàn màn hình (+thêm 5% để không bị lộ mép)
        Vector3 newScale = transform.localScale;
        newScale.x = (worldScreenWidth / width) * 1.05f;
        newScale.y = (worldScreenHeight / height) * 1.05f;

        transform.localScale = newScale;
    }
}