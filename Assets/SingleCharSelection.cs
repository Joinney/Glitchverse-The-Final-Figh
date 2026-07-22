using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SingleCharSelection : MonoBehaviour
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
    // Vẫn giữ biến p2PreviewImage để Inspector của bạn không bị mất tham chiếu
    public Image p2PreviewImage;
    public GameObject startBattleButton;

    [Header("UI Models (Animations)")]
    public Image p1ModelAnimImage;
    public Image p2ModelAnimImage;

    [Header("Characters List")]
    public CharacterData[] characters;

    [Header("Cấu hình Scene")]
    public string nextSceneName = "Fight_Stage1";

    private int currentSelectedIndex = -1;
    private Coroutine p1AnimCoroutine;

    void Start()
    {
        // Vừa vào màn hình là reset sạch sẽ mọi thứ (Giải quyết triệt để lỗi Back ra vô lại)
        currentSelectedIndex = -1;
        SetImageAlpha(p1PreviewImage, 0f);
        SetImageAlpha(p1ModelAnimImage, 0f);
        SetImageAlpha(p2PreviewImage, 0f); // Tạm ẩn P2
        SetImageAlpha(p2ModelAnimImage, 0f);

        // Ẩn nút Start, chỉ hiện khi đã click chọn ít nhất 1 người
        if (startBattleButton != null) startBattleButton.SetActive(false);
    }

    // Gắn hàm này vào sự kiện OnClick của các Nút Nhân Vật (Gojo, Naruto...)
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        currentSelectedIndex = characterIndex;

        // 1. Cập nhật ảnh chân dung (Sáng rõ 100%)
        p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
        SetImageAlpha(p1PreviewImage, 1f);

        // 2. Chạy Animation mượt mà
        if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
        p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));

        // 3. Đã có người được chọn -> Cho phép hiện nút START
        if (startBattleButton != null) startBattleButton.SetActive(true);

        Debug.Log("Đang xem thử: " + characters[characterIndex].characterName);
    }

    // ==========================================
    // GẮN HÀM NÀY VÀO NÚT "START" TRÊN MÀN HÌNH
    // ==========================================
    public void OnStartButtonClicked()
    {
        if (currentSelectedIndex == -1) return; // Tránh lỗi bấm Start khi chưa chọn ai

        string chosenP1 = characters[currentSelectedIndex].characterName;
        PlayerPrefs.SetString("P1_Selection", chosenP1);

        // --- CẤU HÌNH ĐỐI THỦ AI (P2) ---
        // Tự động chọn ngẫu nhiên 1 nhân vật làm Đối thủ AI
        int randomAI = Random.Range(0, characters.Length);
        PlayerPrefs.SetString("P2_Selection", characters[randomAI].characterName);

        // Lưu dữ liệu vào bộ nhớ máy
        PlayerPrefs.Save();

        Debug.Log($"Trận đấu bắt đầu! Player: {chosenP1} VS AI: {characters[randomAI].characterName}");

        // Chuyển Scene
        SceneManager.LoadScene(nextSceneName);
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
}