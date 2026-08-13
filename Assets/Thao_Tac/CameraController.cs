using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Mục tiêu bám theo")]
    public Transform player1;
    public Transform player2;

    [Header("Cài đặt Camera")]
    public float smoothSpeed = 5f; 
    public Vector3 offset = new Vector3(0f, 2f, -10f); 

    [Header("Cấu hình Tự Động Zoom")]
    public float minZoom = 5f;            // Kích thước camera tối thiểu khi 2 đứa đứng sát nhau
    public float maxZoom = 7.5f;          // Kích thước camera tối đa khi ra xa nhau
    public float zoomFactor = 0.25f;      // Độ nhạy của zoom
    private Camera cam;                   // Biến nội bộ để điều khiển component Camera

    [Header("Khóa Tầm Nhìn (Tránh lòi viền đen)")]
    public float minX = -20f; 
    public float maxX = 20f;  
    public float minY = 0f;  
    public float maxY = 3f;  

    void Start()
    {
        cam = GetComponent<Camera>();

        if (player1 == null)
        {
            GameObject p1Obj = GameObject.FindWithTag("Player");
            if (p1Obj != null) player1 = p1Obj.transform;
        }
        
        if (player2 == null)
        {
            GameObject p2Obj = GameObject.FindWithTag("Enemy");
            if (p2Obj != null) player2 = p2Obj.transform;
        }
    }

    void LateUpdate()
    {
        if (player1 == null)
        {
            GameObject p1Obj = GameObject.FindWithTag("Player");
            if (p1Obj != null) player1 = p1Obj.transform;
        }
        if (player2 == null)
        {
            GameObject p2Obj = GameObject.FindWithTag("Enemy");
            if (p2Obj != null) player2 = p2Obj.transform;
        }

        if (player1 == null || player2 == null) return;

        // 1. Tính toán khoảng cách ngang giữa 2 nhân vật
        float distance = Mathf.Abs(player1.position.x - player2.position.x);

        // =========================================================================
        // 💡 XỬ LÝ THÔNG MINH CHO LỖI MẤT CHÂN KHI LẠI GẦN:
        // Khi 2 nhân vật lại gần nhau (distance nhỏ), ta tự động cộng thêm chiều cao 
        // vào offset.y để camera ngước lên trên, giữ trọn chân nhân vật trong khung hình.
        // =========================================================================
        float dynamicYOffset = offset.y + Mathf.Lerp(1.5f, 0f, distance / 5f); 
        Vector3 dynamicOffset = new Vector3(offset.x, dynamicYOffset, offset.z);

        // 2. Logic di chuyển camera theo điểm chính giữa kết hợp offset động
        Vector3 middlePoint = (player1.position + player2.position) / 2f;
        Vector3 targetPosition = middlePoint + dynamicOffset;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY + 1.5f); // Cho phép nhỉnh lên một chút khi cận chiến

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 3. LOGIC TỰ ĐỘNG ZOOM THEO KHOẢNG CÁCH
        if (cam != null)
        {
            float targetZoom = minZoom + (distance * zoomFactor);
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, smoothSpeed * Time.deltaTime);
        }
    }
}