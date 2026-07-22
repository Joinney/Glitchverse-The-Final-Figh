using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountdownManager : MonoBehaviour
{
    [Header("UI Đếm Ngược")]
    public Image countdownImage;

    [Header("Các Khung Hình Animation")]
    public Sprite[] frames3;
    public Sprite[] frames2;
    public Sprite[] frames1;
    public Sprite[] framesFight;

    [Header("Âm Thanh")]
    public AudioSource audioSource;
    public AudioClip tickSound;
    public AudioClip fightSound;

    // CÔNG TẮC TOÀN CẦU: Dùng để báo cho các hệ thống khác biết đã đếm ngược xong chưa
    public static bool isCountdownFinished = false;

    private void Start()
    {
        isCountdownFinished = false;
        StartCoroutine(StartCountdownRoutine());
    }

    private IEnumerator StartCountdownRoutine()
    {
        countdownImage.gameObject.SetActive(true);

        // --- 1. TÌM TẤT CẢ NHÂN VẬT (CẢ PLAYER VÀ AI) ---
        CharacterController2D[] allCharacters = FindObjectsByType<CharacterController2D>(FindObjectsSortMode.None);

        while (allCharacters.Length < 2)
        {
            yield return null;
            allCharacters = FindObjectsByType<CharacterController2D>(FindObjectsSortMode.None);
        }

        // --- 2. KHÓA CỨNG TOÀN BỘ (PLAYER & AI ĐỨNG IM TUYỆT ĐỐI) ---
        foreach (var chara in allCharacters)
        {
            if (chara != null)
            {
                chara.canMoveAndFight = false; // Tắt khả năng di chuyển và đánh của tất cả
            }
        }

        // --- 3. CHIẾU ANIMATION 3 - 2 - 1 - FIGHT ---
        if (tickSound != null && audioSource != null) audioSource.PlayOneShot(tickSound);
        yield return StartCoroutine(PlayFrames(frames3, 1f));

        if (tickSound != null && audioSource != null) audioSource.PlayOneShot(tickSound);
        yield return StartCoroutine(PlayFrames(frames2, 1f));

        if (tickSound != null && audioSource != null) audioSource.PlayOneShot(tickSound);
        yield return StartCoroutine(PlayFrames(frames1, 1f));

        if (fightSound != null && audioSource != null) audioSource.PlayOneShot(fightSound);
        yield return StartCoroutine(PlayFrames(framesFight, 0.8f));

        // --- 4. KẾT THÚC ĐẾM NGƯỢC ---
        countdownImage.gameObject.SetActive(false);

        // --- 5. MỞ KHÓA CHO TẤT CẢ CÙNG XÔNG VÀO ĐÁNH ---
        foreach (var chara in allCharacters)
        {
            if (chara != null)
            {
                chara.canMoveAndFight = true;
            }
        }

        // Bật đồng hồ đếm giờ 60s
        isCountdownFinished = true;
    }

    private IEnumerator PlayFrames(Sprite[] frames, float duration)
    {
        if (frames == null || frames.Length == 0) yield break;

        float timePerFrame = duration / frames.Length;

        for (int i = 0; i < frames.Length; i++)
        {
            countdownImage.sprite = frames[i];
            yield return new WaitForSeconds(timePerFrame);
        }
    }
}