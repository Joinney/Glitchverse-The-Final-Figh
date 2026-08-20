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

    public static bool isCountdownFinished = false;

    private void Start()
    {
        isCountdownFinished = false;
        StartCoroutine(StartCountdownRoutine());
    }

    private IEnumerator StartCountdownRoutine()
    {
        if (countdownImage != null) countdownImage.gameObject.SetActive(true);

        // 1. Chỉ tìm CharacterController2D (Cập nhật cú pháp Unity 6 chống báo vàng)
        CharacterController2D[] allCharacters = FindObjectsByType<CharacterController2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        // Chờ đến khi đủ 2 nhân vật (Player và AI) xuất hiện trên sân
        while (allCharacters.Length < 2)
        {
            yield return null;
            allCharacters = FindObjectsByType<CharacterController2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        // 2. KHÓA CỨNG TOÀN BỘ CẢ 2 BÊN
        foreach (var chara in allCharacters)
        {
            if (chara != null) chara.canMoveAndFight = false;
        }

        // 3. CHIẾU ANIMATION (Đã loại bỏ Âm thanh)
        yield return StartCoroutine(PlayFrames(frames3, 1f));
        yield return StartCoroutine(PlayFrames(frames2, 1f));
        yield return StartCoroutine(PlayFrames(frames1, 1f));
        yield return StartCoroutine(PlayFrames(framesFight, 0.8f));

        if (countdownImage != null) countdownImage.gameObject.SetActive(false);

        // 4. MỞ KHÓA CHO CẢ 2 BÊN CÙNG ĐÁNH
        foreach (var chara in allCharacters)
        {
            if (chara != null) chara.canMoveAndFight = true;
        }

        isCountdownFinished = true;
    }

    private IEnumerator PlayFrames(Sprite[] frames, float duration)
    {
        if (frames == null || frames.Length == 0) yield break;
        float timePerFrame = duration / frames.Length;

        for (int i = 0; i < frames.Length; i++)
        {
            if (countdownImage != null) countdownImage.sprite = frames[i];
            yield return new WaitForSeconds(timePerFrame);
        }
    }
}