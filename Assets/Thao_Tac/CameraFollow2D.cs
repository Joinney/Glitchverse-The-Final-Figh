using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(2f, 1.5f, -10f);

    [Header("Trạng Thái Khóa Đấu Trường")]
    public bool isLocked = false;
    private float lockedCameraX;
    private Camera cam;

    private GameObject leftBorderWall;
    private GameObject rightBorderWall;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        isLocked = false; // Reset trạng thái khóa khi mới tải scene
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (!isLocked)
        {
            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, offset.y, offset.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        }
        else
        {
            // Cố định vị trí Camera
            transform.position = new Vector3(lockedCameraX, offset.y, offset.z);
        }
    }

    public void LockCameraAtCurrentPosition()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        // Căn tâm Camera theo vị trí hiện tại
        lockedCameraX = transform.position.x;
        isLocked = true;

        CreateScreenBorderWalls();
    }

    public void UnlockCamera()
    {
        isLocked = false;
        RemoveScreenBorderWalls();
    }

    private void CreateScreenBorderWalls()
    {
        RemoveScreenBorderWalls();

        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        // Tính khoảng cách nửa chiều rộng màn hình camera
        float camHalfWidth = cam.orthographicSize * cam.aspect;

        // Đặt 2 tường chắn nằm ở NGOÀI mép màn hình (+1 mét ra ngoài rìa)
        float leftX = lockedCameraX - camHalfWidth - 1f;
        float rightX = lockedCameraX + camHalfWidth + 1f;

        leftBorderWall = CreateWall("Arena_Border_Left", leftX);
        rightBorderWall = CreateWall("Arena_Border_Right", rightX);
    }

    private void RemoveScreenBorderWalls()
    {
        if (leftBorderWall != null) Destroy(leftBorderWall);
        if (rightBorderWall != null) Destroy(rightBorderWall);
    }

    private GameObject CreateWall(string wallName, float posX)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.position = new Vector3(posX, offset.y, 0f);
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 30f);
        return wall;
    }

    private void OnDestroy()
    {
        RemoveScreenBorderWalls();
    }
}