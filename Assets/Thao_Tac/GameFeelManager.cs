using UnityEngine;
using System.Collections;

public class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager instance;

    private float originalOrthoSize;
    private Vector3 originalCamPos;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Lưu lại kích thước và vị trí ban đầu của Camera
        if (Camera.main != null)
        {
            originalOrthoSize = Camera.main.orthographicSize;
            originalCamPos = Camera.main.transform.position;
        }
    }

    // ==========================================
    // 1. HIỆU ỨNG HIT-STOP (KHỰNG HÌNH)
    // ==========================================
    public void TriggerHitStop(float duration = 0.05f)
    {
        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    // ==========================================
    // 2. HIỆU ỨNG RUNG MÀN HÌNH (CAMERA SHAKE)
    // ==========================================
    public void TriggerCameraShake(float duration = 0.1f, float magnitude = 0.2f)
    {
        StartCoroutine(CameraShakeRoutine(duration, magnitude));
    }

    private IEnumerator CameraShakeRoutine(float duration, float magnitude)
    {
        Transform camTransform = Camera.main.transform;
        Vector3 origPos = camTransform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            camTransform.localPosition = new Vector3(origPos.x + x, origPos.y + y, origPos.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        camTransform.localPosition = origPos;
    }

    // ==========================================
    // 3. GÓC MÁY KẾT LIỄU ĐỐI THỦ (CINEMATIC)
    // ==========================================
    public void TriggerCinematicFinish(Transform loser, Transform winner, bool isPlayerWin)
    {
        StartCoroutine(CinematicFinishRoutine(loser, winner, isPlayerWin));
    }

    private IEnumerator CinematicFinishRoutine(Transform loser, Transform winner, bool isPlayerWin)
    {
        Camera cam = Camera.main;
        MonoBehaviour[] camScripts = cam.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in camScripts)
        {
            if (script != this) script.enabled = false;
        }

        // 1. Slow-Motion siêu ngầu
        Time.timeScale = 0.2f;

        float transitionTime = 0.5f;
        float elapsed = 0f;
        Vector3 startCamPos = cam.transform.position;
        float targetZoomSize = originalOrthoSize * 0.5f; // Zoom sát 50%

        // PHASE 1: ZOOM VÀO VÀ LIA MÁY BÁM THEO KẺ THUA
        while (elapsed < transitionTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionTime;
            t = t * t * (3f - 2f * t);

            // UPDATE: Cập nhật vị trí kẻ thua LIÊN TỤC để camera không bị mất dấu khi xác văng đi
            Vector3 loserFocusPos = new Vector3(loser.position.x, loser.position.y + 0.5f, startCamPos.z);

            cam.transform.position = Vector3.Lerp(startCamPos, loserFocusPos, t);
            cam.orthographicSize = Mathf.Lerp(originalOrthoSize, targetZoomSize, t);
            yield return null;
        }

        // PHASE 2: NGẮM KẺ THUA RƠI XUỐNG ĐẤT
        float fallWatchTime = 2f; // UPDATE: Kéo dài hẳn 2 giây (realtime) để xem xác chạm đất
        float fallElapsed = 0f;
        while (fallElapsed < fallWatchTime)
        {
            fallElapsed += Time.unscaledDeltaTime;
            // Ép camera khóa chặt theo mục tiêu đang rơi
            if (loser != null)
            {
                cam.transform.position = new Vector3(loser.position.x, loser.position.y + 0.5f, startCamPos.z);
            }
            yield return null;
        }

        // PHASE 3: LƯỚT CAMERA SANG KẺ CHIẾN THẮNG
        elapsed = 0f;
        startCamPos = cam.transform.position;

        while (elapsed < transitionTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionTime;
            t = t * t * (3f - 2f * t);

            if (winner != null)
            {
                Vector3 winnerFocusPos = new Vector3(winner.position.x, winner.position.y + 0.5f, startCamPos.z);
                cam.transform.position = Vector3.Lerp(startCamPos, winnerFocusPos, t);
            }
            yield return null;
        }

        // Ngắm kẻ chiến thắng gáy / tạo dáng trong 1.5 giây
        yield return new WaitForSecondsRealtime(1.5f);

        // PHASE 4: TRẢ LẠI BÌNH THƯỜNG VÀ GỌI BẢNG VICTORY
        Time.timeScale = 1f;
        cam.transform.position = originalCamPos;
        cam.orthographicSize = originalOrthoSize;

        foreach (MonoBehaviour script in camScripts)
        {
            if (script != this) script.enabled = true;
        }

        MatchController matchCtrl = Object.FindFirstObjectByType<MatchController>();
        if (matchCtrl != null)
        {
            matchCtrl.EndMatch(isPlayerWin);
        }
    }
}