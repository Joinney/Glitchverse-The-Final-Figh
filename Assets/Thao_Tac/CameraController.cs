using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Mục tiêu bám theo")]
    public Transform player1;
    public Transform player2;

    [Header("Cài đặt Camera")]
    public float smoothSpeed = 5f; 
    public Vector3 offset = new Vector3(0f, 2f, -10f); 

    [Header("Cấu hình Tự Động Zoom (NEW)")]
    public float minZoom = 5f;          // Kích thước camera tối thiểu khi 2 đứa đứng sát nhau (mặc định gốc của bạn là 5)
    public float maxZoom = 7.5f;        // Kích thước camera tối đa khi ra xa nhau (tránh lùi quá rộng lòi viền)
    public float zoomFactor = 0.25f;    // Độ nhạy của zoom (số càng lớn camera zoom ra càng nhanh khi nhân vật lùi lại)
    private Camera cam;                 // Biến nội bộ để điều khiển component Camera

    [Header("Khóa Tầm Nhìn (Tránh lòi viền đen)")]
    public float minX = -20f; 
    public float maxX = 20f;  
    public float minY = 0f;  
    public float maxY = 3f;  

    void Start()
    {
        // Tự động lấy linh hồn Camera 2D gắn trên chính Object này
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

        // 1. Logic di chuyển camera theo điểm chính giữa (Giữ nguyên)
        Vector3 middlePoint = (player1.position + player2.position) / 2f;
        Vector3 targetPosition = middlePoint + offset;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 2. LOGIC TỰ ĐỘNG ZOOM THEO KHOẢNG CÁCH (XỬ LÝ MỚI)
        if (cam != null)
        {
            // Tính toán khoảng cách thực tế giữa Player và Enemy theo trục X (chiều ngang)
            float distance = Mathf.Abs(player1.position.x - player2.position.x);

            // Tính toán size camera cần đạt tới: Khoảng cách càng xa, size camera càng to lên
            float targetZoom = minZoom + (distance * zoomFactor);

            // Chốt chặn ép không cho phép kích thước camera vượt quá ngưỡng min và max định sẵn
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

            // Nội suy Lerp giúp camera thay đổi kích thước mượt mà, không bị khựng giật khi nhân vật dash nhanh
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, smoothSpeed * Time.deltaTime);
        }
    }
}