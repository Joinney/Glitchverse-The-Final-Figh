using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SingleCharSelection : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public Sprite portraitSprite; // Ảnh chân dung tĩnh
        public Sprite[] fightIdleSprites; // Chuỗi ảnh động toàn thân
    }

    [Header("UI References")]
    public Image p1PreviewImage;
    public Image p1ModelAnimImage;
    public GameObject startBattleButton;

    [Header("Characters List")]
    public CharacterData[] characters;

    private int p1SelectedIndex = -1;
    private bool isP1Locked = false;
    private Coroutine p1AnimCoroutine;

    // ==========================================
    // --- HÀM MỚI: TỰ ĐỘNG RESET KHI MỞ BẢNG ---
    // ==========================================
    void OnEnable()
    {
        // 1. Mở khóa cho phép người chơi chọn lại
        isP1Locked = false;
        p1SelectedIndex = -1;

        // 2. Dừng các animation cũ đang chạy
        if (p1AnimCoroutine != null)
        {
            StopCoroutine(p1AnimCoroutine);
            p1AnimCoroutine = null;
        }

        // 3. Giấu ảnh nhân vật của lần chọn trước đi
        SetImageAlpha(p1PreviewImage, 0f);
        SetImageAlpha(p1ModelAnimImage, 0f);

        // 4. Giấu nút START đi
        if (startBattleButton != null) startBattleButton.SetActive(false);

        Debug.Log("Đã reset Bảng Chọn 1 Người về trạng thái ban đầu!");
    }

    void Start()
    {
        // Mới vào ẩn ảnh chân dung và mô hình động đi
        SetImageAlpha(p1PreviewImage, 0f);
        SetImageAlpha(p1ModelAnimImage, 0f);
        if (startBattleButton != null) startBattleButton.SetActive(false);
    }

    // RÊ CHUỘT XEM THỬ CHÂN DUNG + ANIMATION
    public void HoverCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length || isP1Locked) return;

        // Hiện chân dung mờ 60%
        p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
        SetImageAlpha(p1PreviewImage, 0.6f);

        // Chạy animation toàn thân mờ 60%
        if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
        p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 0.6f));
    }

    // CLICK ĐỂ CHỐT KHÓA NHÂN VẬT P1
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length || isP1Locked) return;

        p1SelectedIndex = characterIndex;
        p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
        SetImageAlpha(p1PreviewImage, 1f); // Sáng rõ 100%
        isP1Locked = true;

        // Ép animation sáng rõ 100%
        if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
        p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));

        // Lưu nhân vật người chơi chọn
        PlayerPrefs.SetString("P1_Selection", characters[characterIndex].characterName);
        Debug.Log("Chơi đơn - P1 đã khóa: " + characters[characterIndex].characterName);

        // HIỆN NÚT START TRẬN ĐẤU LUÔN VÌ CHỈ CẦN 1 NGƯỜI CHỌN!
        if (startBattleButton != null)
        {
            startBattleButton.SetActive(true);
        }
    }

    private IEnumerator PlayModelAnimation(Image targetImage, Sprite[] sprites, float alpha)
    {
        if (targetImage == null || sprites == null || sprites.Length == 0) yield break;

        SetImageAlpha(targetImage, alpha);
        int frameIndex = 0;

        while (true)
        {
            targetImage.sprite = sprites[frameIndex];
            frameIndex = (frameIndex + 1) % sprites.Length;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    // Gọi khi bấm nút START vào trận đấu
    public void LoadBattleScene(string sceneName)
    {
        // Tự động gán ngẫu nhiên đối thủ AI cho Player 2 trước khi vào trận
        int randomAIIndex = Random.Range(0, characters.Length);
        PlayerPrefs.SetString("P2_Selection", characters[randomAIIndex].characterName);
        Debug.Log("AI ngẫu nhiên được chọn là: " + characters[randomAIIndex].characterName);

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}