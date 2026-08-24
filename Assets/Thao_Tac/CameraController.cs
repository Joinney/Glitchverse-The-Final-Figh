using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    [Header("Mục tiêu bám theo")]
    public Transform player1;
    public Transform player2;

    [Header("Cài đặt Camera")]
    public float smoothSpeed = 5f; 
    public Vector3 offset = new Vector3(0f, 1.5f, -10f); 

    [Header("Cấu hình Kích Thước Camera")]
    public float defaultZoom = 6f;      // Kích thước camera cố định khi đang thi đấu bình thường
    public float deathZoom = 3.2f;      // Kích thước phóng to cận cảnh khi có đứa chết
    public float deathZoomSpeed = 3.5f; // Tốc độ zoom khi kết liễu
    private Camera cam;

    [Header("Khóa Tầm Nhìn (Tránh lòi viền đen)")]
    public float minX = -20f; 
    public float maxX = 20f;  
    public float minY = 0f;  
    public float maxY = 3f;  

    private bool isCinematicDeath = false;
    private Transform deathTarget;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographicSize = defaultZoom;
        }

        FindPlayers();
    }

    void FindPlayers()
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
    }

    // 🎬 HÀM GỌI KHI CÓ NHÂN VẬT TỬ TRẬN
    public void TriggerDeathZoom(Transform deadTarget)
    {
        isCinematicDeath = true;
        deathTarget = deadTarget;
    }

    void LateUpdate()
    {
        // -------------------------------------------------------------
        // 1. CHẾ ĐỘ ZOOM CẬN CẢNH KHI CHẾT (CINEMATIC FINISH)
        // -------------------------------------------------------------
        if (isCinematicDeath && deathTarget != null)
        {
            Vector3 targetPos = deathTarget.position + offset;
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

            transform.position = Vector3.Lerp(transform.position, targetPos, deathZoomSpeed * Time.deltaTime);

            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, deathZoom, deathZoomSpeed * Time.deltaTime);
            }
            return;
        }

        // -------------------------------------------------------------
        // 2. CHẾ ĐỘ BÌNH THƯỜNG TRONG TRẬN ĐẤU (KHÔNG TỰ CO GIÃN ZOOM)
        // -------------------------------------------------------------
        FindPlayers();

        if (player1 == null && player2 == null) return;

        Vector3 targetPosition;

        if (player1 != null && player2 != null)
        {
            // Đi theo tâm điểm giữa 2 người chơi
            Vector3 middlePoint = (player1.position + player2.position) / 2f;
            targetPosition = middlePoint + offset;
        }
        else if (player1 != null)
        {
            targetPosition = player1.position + offset;
        }
        else
        {
            targetPosition = player2.position + offset;
        }

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Giữ camera ở kích thước tiêu chuẩn
        if (cam != null && Mathf.Abs(cam.orthographicSize - defaultZoom) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, defaultZoom, smoothSpeed * Time.deltaTime);
        }
    }
}