using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class TwoPlayerCharSelection : MonoBehaviour
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
    public Image p2PreviewImage;
    public GameObject nextButton; // Nút chuyển sang chọn Map

    [Header("UI Models (Animations)")]
    public Image p1ModelAnimImage;
    public Image p2ModelAnimImage;

    [Header("UI Panels")]
    public GameObject characterSelectPanel;
    public GameObject mapSelectPanel;

    [Header("Characters List")]
    public CharacterData[] characters;

    private int p1SelectedIndex = -1;
    private int p2SelectedIndex = -1;

    // --- THÊM BIẾN KHÓA (CHỐT ĐƠN) ---
    private bool isP1Locked = false;
    private bool isP2Locked = false;

    private Coroutine p1AnimCoroutine;
    private Coroutine p2AnimCoroutine;

    void Start()
    {
        ResetSelection();
    }

    public void ResetSelection()
    {
        p1SelectedIndex = -1;
        p2SelectedIndex = -1;
        isP1Locked = false;
        isP2Locked = false;

        SetImageAlpha(p1PreviewImage, 0f);
        SetImageAlpha(p1ModelAnimImage, 0f);
        SetImageAlpha(p2PreviewImage, 0f);
        SetImageAlpha(p2ModelAnimImage, 0f);

        if (nextButton != null) nextButton.SetActive(false);
    }

    // ==========================================
    // 1. RÀ CHUỘT: XEM THỬ CẢ ẢNH TO LẪN HOẠT ẢNH
    // ==========================================
    public void HoverCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        // Nếu P1 CHƯA KHÓA -> Cập nhật màn hình xem thử cho P1
        if (!isP1Locked)
        {
            p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p1PreviewImage, 1f);

            // Chạy Animation cho model nhỏ của P1
            if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
            p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));
        }
        // Nếu P1 ĐÃ KHÓA, và P2 CHƯA KHÓA -> Cập nhật màn hình xem thử cho P2
        else if (isP1Locked && !isP2Locked)
        {
            p2PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p2PreviewImage, 1f);

            // Lật mặt model P2 quay sang trái nhìn P1
            p2ModelAnimImage.transform.localScale = new Vector3(-Mathf.Abs(p2ModelAnimImage.transform.localScale.x), p2ModelAnimImage.transform.localScale.y, p2ModelAnimImage.transform.localScale.z);

            // Chạy Animation cho model nhỏ của P2
            if (p2AnimCoroutine != null) StopCoroutine(p2AnimCoroutine);
            p2AnimCoroutine = StartCoroutine(PlayModelAnimation(p2ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));
        }
    }

    // ==========================================
    // 2. CLICK CHUỘT: CHỐT NHÂN VẬT VÀ KHÓA LẠI
    // ==========================================
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        // Lượt P1 chọn (Chỉ nhận lệnh Click khi P1 chưa khóa)
        if (!isP1Locked)
        {
            p1SelectedIndex = characterIndex;
            isP1Locked = true; // KHÓA P1 LẠI NGAY LẬP TỨC

            // Đảm bảo ảnh hiển thị đúng nhân vật vừa chốt
            HoverCharacter(characterIndex);

            Debug.Log("Player 1 ĐÃ KHÓA CHỌN: " + characters[characterIndex].characterName);
        }
        // Lượt P2 chọn (Chỉ nhận lệnh Click khi P1 đã khóa và P2 chưa khóa)
        else if (isP1Locked && !isP2Locked)
        {
            p2SelectedIndex = characterIndex;
            isP2Locked = true; // KHÓA P2 LẠI NGAY LẬP TỨC

            HoverCharacter(characterIndex);

            // Cả 2 đã khóa -> Hiện nút NEXT
            if (nextButton != null) nextButton.SetActive(true);

            Debug.Log("Player 2 ĐÃ KHÓA CHỌN: " + characters[characterIndex].characterName);
        }
        // Nếu cả 2 đã khóa rồi (isP1Locked = true và isP2Locked = true) thì việc bấm Click sẽ bị code ngó lơ, không thay đổi được nữa.
    }

    // ==========================================
    // 3. CHUYỂN SANG BẢNG CHỌN MAP
    // ==========================================
    public void OnNextButtonClicked()
    {
        if (!isP1Locked || !isP2Locked) return; // Phải khóa cả 2 mới được đi tiếp

        PlayerPrefs.SetString("P1_Selection", characters[p1SelectedIndex].characterName);
        PlayerPrefs.SetString("P2_Selection", characters[p2SelectedIndex].characterName);
        PlayerPrefs.SetString("GameMode", "PvP");
        PlayerPrefs.Save();

        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (mapSelectPanel != null) mapSelectPanel.SetActive(true);
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
            yield return new WaitForSeconds(0.1f); // Tốc độ chạy ảnh múa
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