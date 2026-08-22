using System.Collections;
using UnityEngine;
using TMPro;

public class BossAnnouncementUI : MonoBehaviour
{
    private static BossAnnouncementUI instance;
    private TextMeshProUGUI warningText;
    private GameObject bannerCanvasObj;

    public static void ShowAnnouncement(string title, Color color, float duration = 2.5f)
    {
        if (instance == null)
        {
            GameObject managerObj = new GameObject("BossAnnouncementManager");
            instance = managerObj.AddComponent<BossAnnouncementUI>();
            instance.InitCanvas();
        }

        instance.StopAllCoroutines();
        instance.StartCoroutine(instance.AnimateBannerSmooth(title, color, duration));
    }

    private void InitCanvas()
    {
        bannerCanvasObj = new GameObject("BossAnnouncementCanvas");
        Canvas canvas = bannerCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject textObj = new GameObject("WarningText");
        textObj.transform.SetParent(bannerCanvasObj.transform, false);

        warningText = textObj.AddComponent<TextMeshProUGUI>();
        warningText.alignment = TextAlignmentOptions.Center;
        warningText.fontSize = 38;
        warningText.fontStyle = FontStyles.Bold;

        RectTransform rect = warningText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.65f);
        rect.anchorMax = new Vector2(1f, 0.85f);
        rect.sizeDelta = Vector2.zero;

        warningText.gameObject.SetActive(false);
    }

    private IEnumerator AnimateBannerSmooth(string text, Color col, float duration)
    {
        if (warningText == null) yield break;

        warningText.text = text;
        warningText.gameObject.SetActive(true);

        float fadeInTime = 0.4f;
        float fadeOutTime = 0.5f;
        float stayTime = duration - (fadeInTime + fadeOutTime);
        if (stayTime < 0.5f) stayTime = 0.5f;

        // 1. Xuất hiện mượt mà (Fade In & Scale từ 0.8 -> 1.0)
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInTime);
            
            Color c = col;
            c.a = t;
            warningText.color = c;
            warningText.transform.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, t);
            yield return null;
        }

        // 2. Đứng yên tĩnh lặng
        Color fullColor = col;
        fullColor.a = 1f;
        warningText.color = fullColor;
        warningText.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(stayTime);

        // 3. Mờ dần êm ái (Fade Out)
        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutTime);
            
            Color c = col;
            c.a = 1f - t;
            warningText.color = c;
            yield return null;
        }

        warningText.gameObject.SetActive(false);
    }
}