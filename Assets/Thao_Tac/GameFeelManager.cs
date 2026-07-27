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

        // KHÓA TAY CHÂN NGƯỜI CHIẾN THẮNG
        if (winner != null)
        {
            MonoBehaviour[] winnerScripts = winner.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in winnerScripts)
            {
                string sName = script.GetType().Name;
                if (sName.Contains("Movement") || sName.Contains("AI") || sName.Contains("Controller") || sName.Contains("Health"))
                {
                    script.enabled = false;
                }
            }

            Rigidbody2D winnerRb = winner.GetComponent<Rigidbody2D>();
            if (winnerRb != null) winnerRb.linearVelocity = Vector2.zero;

            Animator winnerAnim = winner.GetComponent<Animator>();
            if (winnerAnim != null)
            {
                winnerAnim.SetFloat("Speed", 0);
                winnerAnim.SetBool("IsBlocking", false);
            }
        }

        // Slow-Motion siêu ngầu
        Time.timeScale = 0.2f;

        float transitionTime = 0.5f;
        float elapsed = 0f;
        Vector3 startCamPos = cam.transform.position;
        float targetZoomSize = originalOrthoSize * 0.5f;

        // PHASE 1: ZOOM VÀO VÀ LIA MÁY BÁM THEO KẺ THUA
        while (elapsed < transitionTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionTime;
            t = t * t * (3f - 2f * t);

            Vector3 loserFocusPos = new Vector3(loser.position.x, loser.position.y + 0.5f, startCamPos.z);
            cam.transform.position = Vector3.Lerp(startCamPos, loserFocusPos, t);
            cam.orthographicSize = Mathf.Lerp(originalOrthoSize, targetZoomSize, t);
            yield return null;
        }

        // =======================================
        // CAMERA THÔNG MINH KẾT HỢP XỬ LÝ VẬT LÝ KHI CHẠM ĐẤT
        // =======================================
        float maxFallWatchTime = 6f;
        float fallElapsed = 0f;
        Rigidbody2D loserRb = loser.GetComponent<Rigidbody2D>();

        bool wasFalling = false; // Biến cờ theo dõi: Đã rơi chưa?

        while (fallElapsed < maxFallWatchTime)
        {
            fallElapsed += Time.unscaledDeltaTime;

            if (loser != null)
            {
                cam.transform.position = new Vector3(loser.position.x, loser.position.y + 0.5f, startCamPos.z);
            }

            if (loserRb != null)
            {
                // Xác nhận xác đang trên đà rớt xuống (Vận tốc Y âm)
                if (loserRb.linearVelocity.y < -0.1f)
                {
                    wasFalling = true;
                }

                // NẾU ĐÃ TỪNG RƠI XUỐNG, mà giờ vận tốc Y bằng 0 => Chính xác 100% là vừa chạm sàn
                if (wasFalling && Mathf.Abs(loserRb.linearVelocity.y) < 0.05f)
                {

                    // 1. TRIỆT TIÊU LỖI TRƯỢT: Đóng băng toàn bộ vật lý
                    loserRb.linearVelocity = Vector2.zero;
                    loserRb.simulated = false; // Tắt luôn vật lý, xác nằm im như tượng

                    // 2. TRIỆT TIÊU LỖI LƠ LỬNG: Dìm xác xuống mặt đất một chút
                    // MẸO: Bạn có thể thay đổi số -0.4f thành -0.3f hoặc -0.5f nếu thấy nó chìm quá sâu hoặc chưa đủ sát sàn
                    loser.position = new Vector3(loser.position.x, loser.position.y - 0.4f, loser.position.z);

                    yield return new WaitForSecondsRealtime(0.5f); // Ngắm xác nằm im thêm 0.5s rồi mới lướt đi
                    break;
                }
            }
            yield return null;
        }
        // =======================================

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