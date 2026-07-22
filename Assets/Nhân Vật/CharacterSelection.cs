using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelection : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public Sprite portraitSprite;
        public Sprite[] fightIdleSprites;
    }

    [Header("UI Previews (Portraits)")]
    public Image p1PreviewImage;
    public Image p2PreviewImage; // Giữ biến này lại để Unity không báo lỗi (nếu bạn lỡ kéo thả)
    public GameObject startBattleButton;

    [Header("UI Models (Animations)")]
    public Image p1ModelAnimImage;
    public Image p2ModelAnimImage;

    [Header("Characters List")]
    public CharacterData[] characters;

    private int p1SelectedIndex = -1;
    private Coroutine p1AnimCoroutine;

    void Start()
    {
        // Ẩn tất cả khi vừa mở bảng chọn tướng
        SetImageAlpha(p1PreviewImage, 0f);
        SetImageAlpha(p2PreviewImage, 0f);
        SetImageAlpha(p1ModelAnimImage, 0f);
        SetImageAlpha(p2ModelAnimImage, 0f);

        // Giấu nút Start đi
        if (startBattleButton != null) startBattleButton.SetActive(false);
        Debug.Log("CODE MOI DA CHAY 100%!");
    }

    // ==========================================
    // LOGIC CLICK CHUỘT: CHỈ ĐỊNH VÀ VÀO GAME
    // ==========================================
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        // 1. Chỉ định nhân vật (Không cần khóa)
        p1SelectedIndex = characterIndex;

        // 2. Hiển thị ảnh chân dung rõ 100%
        p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
        SetImageAlpha(p1PreviewImage, 1f);

        // 3. Chạy animation toàn thân
        if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
        p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));

        // 4. Lưu tên nhân vật vào hệ thống để mang vào màn Fight
        PlayerPrefs.SetString("P1_Selection", characters[characterIndex].characterName);
        Debug.Log("Đang chọn: " + characters[characterIndex].characterName);

        // 5. Bật nút Start Game lên
        if (startBattleButton != null) startBattleButton.SetActive(true);
    }

    // ==========================================
    // LOGIC RÊ CHUỘT (Chỉ áp dụng khi chưa chọn ai)
    // ==========================================
    public void HoverCharacter(int characterIndex)
    {
        // Nếu ĐÃ CLICK chọn 1 nhân vật rồi -> KHÔNG cho hover làm phiền màn hình nữa
        if (p1SelectedIndex != -1) return;

        // Nếu CHƯA CHỌN ai -> Hiển thị bóng mờ mờ 60% cho người chơi xem trước
        if (characterIndex >= 0 && characterIndex < characters.Length)
        {
            p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p1PreviewImage, 0.6f);

            if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
            p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 0.6f));
        }
    }

    // ==========================================
    // HÀM HỖ TRỢ
    // ==========================================
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

    public void CancelSelection()
    {
        // Hàm này gọi khi bấm nút "BACK"
        p1SelectedIndex = -1;
        SetImageAlpha(p1PreviewImage, 0f);
        SetImageAlpha(p1ModelAnimImage, 0f);
        if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);

        if (startBattleButton != null) startBattleButton.SetActive(false);
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

    public void LoadBattleScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}