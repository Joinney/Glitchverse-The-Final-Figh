using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(2f, 1.5f, -10f);

    [Header("Trạng Thái Khóa")]
    public bool isLocked = false;
    private float lockedCameraX;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (!isLocked)
        {
            // Trượt theo nhân vật bình thường
            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, offset.y, offset.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        }
        else
        {
            // 🔒 Khóa cứng camera tại vị trí đấu trường
            transform.position = new Vector3(lockedCameraX, offset.y, offset.z);

            // Giữ chân Player không cho chạy ra khỏi mép trái/phải của màn hình
            ClampPlayerInsideScreen();
        }
    }

    public void LockCameraAtCurrentPosition()
    {
        lockedCameraX = transform.position.x;
        isLocked = true;
    }

    public void UnlockCamera()
    {
        isLocked = false;
    }

    private void ClampPlayerInsideScreen()
    {
        if (cam == null || target == null) return;

        // Tính khoảng cách từ tâm camera ra 2 mép màn hình
        float camHalfWidth = cam.orthographicSize * cam.aspect;
        float minX = transform.position.x - camHalfWidth + 0.8f;
        float maxX = transform.position.x + camHalfWidth - 0.8f;

        // Giữ vị trí người chơi nằm trọn trong màn hình
        float clampedX = Mathf.Clamp(target.position.x, minX, maxX);
        target.position = new Vector3(clampedX, target.position.y, target.position.z);
    }
}