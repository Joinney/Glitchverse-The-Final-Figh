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

    public void TriggerHitStop(float duration = 0.05f) { StartCoroutine(HitStopRoutine(duration)); }
    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public void TriggerCameraShake(float duration = 0.1f, float magnitude = 0.2f) { StartCoroutine(CameraShakeRoutine(duration, magnitude)); }
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

        // ==========================================
        // 1. TỰ ĐỘNG TÌM ĐÚNG NGƯỜI CHIẾN THẮNG (Sửa lỗi Zoom nhầm người)
        // ==========================================
        if (winner == null)
        {
            PlayerHealth[] allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
            foreach (PlayerHealth p in allPlayers)
            {
                if (p.transform != loser)
                {
                    winner = p.transform;
                    break;
                }
            }
        }

        // ==========================================
        // 2. ANTI-JUGGLING: CHỐNG GIẬT CAMERA KHI CHẾT TRÊN KHÔNG
        // ==========================================
        if (loser != null)
        {
            // Tắt mọi điểm neo sát thương (Triggers) để các skill đang bay tới sẽ xuyên qua luôn
            // Giúp xác chết rơi xuống mượt mà theo trọng lực mà không bị xóc nảy!
            Collider2D[] loserCols = loser.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D col in loserCols)
            {
                if (col.isTrigger) col.enabled = false;
            }
        }

        // Khóa người chiến thắng tạo dáng
        if (winner != null)
        {
            MonoBehaviour[] winnerScripts = winner.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in winnerScripts)
            {
                string sName = script.GetType().Name;
                if (sName.Contains("Movement") || sName.Contains("AI") || sName.Contains("Controller") || sName.Contains("Health"))
                    script.enabled = false;
            }
            Rigidbody2D winnerRb = winner.GetComponent<Rigidbody2D>();
            if (winnerRb != null) winnerRb.linearVelocity = Vector2.zero;
            Animator winnerAnim = winner.GetComponent<Animator>();
            if (winnerAnim != null) { winnerAnim.SetFloat("Speed", 0); winnerAnim.SetBool("IsBlocking", false); }
        }

        Time.timeScale = 0.2f;
        float transitionTime = 0.5f;
        float elapsed = 0f;
        Vector3 startCamPos = cam.transform.position;
        float targetZoomSize = originalOrthoSize * 0.5f;

        // ZOOM VÀO KẺ THUA NGÃ XUỐNG
        while (elapsed < transitionTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionTime;
            t = t * t * (3f - 2f * t);
            if (loser != null)
            {
                Vector3 loserFocusPos = new Vector3(loser.position.x, loser.position.y + 0.5f, startCamPos.z);
                cam.transform.position = Vector3.Lerp(startCamPos, loserFocusPos, t);
            }
            cam.orthographicSize = Mathf.Lerp(originalOrthoSize, targetZoomSize, t);
            yield return null;
        }

        // CHỜ XÁC CHẠM ĐẤT TRONG SLOW MOTION
        float maxFallWatchTime = 6f;
        float fallElapsed = 0f;
        Rigidbody2D loserRb = loser != null ? loser.GetComponent<Rigidbody2D>() : null;
        bool wasFalling = false; 

        while (fallElapsed < maxFallWatchTime)
        {
            fallElapsed += Time.unscaledDeltaTime;
            if (loser != null) cam.transform.position = new Vector3(loser.position.x, loser.position.y + 0.5f, startCamPos.z);

            if (loserRb != null)
            {
                if (loserRb.linearVelocity.y < -0.1f) wasFalling = true;
                if (wasFalling && Mathf.Abs(loserRb.linearVelocity.y) < 0.05f)
                {
                    loserRb.linearVelocity = Vector2.zero;
                    loserRb.simulated = false; // Tắt hẳn vật lý để xác nằm im
                    loser.position = new Vector3(loser.position.x, loser.position.y - 0.4f, loser.position.z);
                    yield return new WaitForSecondsRealtime(0.5f);
                    break;
                }
            }
            yield return null;
        }

        // CHUYỂN CẢNH SANG NGƯỜI CHIẾN THẮNG
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

        // TRẢ LẠI TRẠNG THÁI CAMERA
        Time.timeScale = 1f;
        cam.transform.position = originalCamPos;
        cam.orthographicSize = originalOrthoSize;
        foreach (MonoBehaviour script in camScripts)
        {
            if (script != this) script.enabled = true;
        }

        // ==========================================
        // 3. ĐIỀU HƯỚNG VỀ ĐÚNG CHẾ ĐỘ (SINGLE HAY PVP)
        // ==========================================
        MatchController matchCtrl = Object.FindFirstObjectByType<MatchController>();
        if (matchCtrl != null)
        {
            string mode = PlayerPrefs.GetString("GameMode", "Single");
            if (mode == "PvP")
            {
                int winnerIdx = 1;
                CharacterController2D cc = loser.GetComponent<CharacterController2D>();
                if (cc != null && cc.playerIndex == 1) winnerIdx = 2; // P1 chết -> P2 thắng
                matchCtrl.EndPvPMatch(winnerIdx);
            }
            else
            {
                matchCtrl.EndMatch(isPlayerWin);
            }
        }
    }
}