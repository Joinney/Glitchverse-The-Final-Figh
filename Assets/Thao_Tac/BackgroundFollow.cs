using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    private Transform camTransform;
    private Vector3 lastCamPos;

    [Header("Tốc độ trượt (0 = dính chặt camera, 0.5 = trượt chậm chiều sâu)")]
    public float parallaxSpeedX = 0.8f; 
    public bool lockY = true;

    void Start()
    {
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
            lastCamPos = camTransform.position;
        }
    }

    void LateUpdate()
    {
        if (camTransform == null) return;

        Vector3 deltaMovement = camTransform.position - lastCamPos;
        
        // Di chuyển nền theo camera
        float newX = transform.position.x + (deltaMovement.x * parallaxSpeedX);
        float newY = lockY ? transform.position.y : transform.position.y + deltaMovement.y;

        transform.position = new Vector3(newX, newY, transform.position.z);
        lastCamPos = camTransform.position;
    }
}