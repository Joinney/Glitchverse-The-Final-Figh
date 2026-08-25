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

    [Header("UI Buttons Chế Độ (Hiện khi chọn xong 2 tướng)")]
    public GameObject pvpButton; // Kéo Btn_PvP vào đây
    public GameObject pveButton; // Kéo Btn_PvE vào đây

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

    // --- BIẾN KHÓA (CHỐT ĐƠN) ---
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

        // Ẩn 2 nút bắt đầu khi chưa chọn xong tướng
        if (pvpButton != null) pvpButton.SetActive(false);
        if (pveButton != null) pveButton.SetActive(false);
    }

    // ==========================================
    // 1. RÀ CHUỘT: XEM THỬ ẢNH VÀ HOẠT ẢNH
    // ==========================================
    public void HoverCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        // Nếu P1 CHƯA KHÓA -> Cập nhật màn hình xem thử cho P1
        if (!isP1Locked)
        {
            p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p1PreviewImage, 1f);

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

            if (p2AnimCoroutine != null) StopCoroutine(p2AnimCoroutine);
            p2AnimCoroutine = StartCoroutine(PlayModelAnimation(p2ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));
        }
    }

    // ==========================================
    // 2. CLICK CHUỘT: CHỐT NHÂN VẬT VÀ KHÓA
    // ==========================================
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        // Lượt P1 chọn
        if (!isP1Locked)
        {
            p1SelectedIndex = characterIndex;
            isP1Locked = true; // Khóa P1

            HoverCharacter(characterIndex);
            Debug.Log("Player 1 ĐÃ KHÓA: " + characters[characterIndex].characterName);
        }
        // Lượt P2 chọn
        else if (isP1Locked && !isP2Locked)
        {
            p2SelectedIndex = characterIndex;
            isP2Locked = true; // Khóa P2

            HoverCharacter(characterIndex);
            Debug.Log("Player 2 ĐÃ KHÓA: " + characters[characterIndex].characterName);

            // Cả 2 đã khóa xong -> Hiển thị cả 2 nút PvP và PvE
            if (pvpButton != null) pvpButton.SetActive(true);
            if (pveButton != null) pveButton.SetActive(true);
        }
    }

    // ==========================================
    // 3. XỬ LÝ 2 NÚT RẼ NHÁNH CHẾ ĐỘ CHƠI
    // ==========================================

    // ⚔️ Nút 1: ĐẤU PvP (2 Người bấm 2 bàn phím)
    public void OnStartPvPClick()
    {
        if (!isP1Locked || !isP2Locked) return;

        PlayerPrefs.SetString("P1_Selection", characters[p1SelectedIndex].characterName);
        PlayerPrefs.SetString("P2_Selection", characters[p2SelectedIndex].characterName);
        PlayerPrefs.SetString("GameMode", "PvP");
        PlayerPrefs.SetInt("IsP2AI", 0); // P2 là người thật
        PlayerPrefs.Save();

        ChuyenSangBangChonMap();
    }

    // 🤖 Nút 2: ĐẤU PvE (P1 vs P2 Máy AI Tự Đánh)
    public void OnStartPvEClick()
    {
        if (!isP1Locked || !isP2Locked) return;

        PlayerPrefs.SetString("P1_Selection", characters[p1SelectedIndex].characterName);
        PlayerPrefs.SetString("P2_Selection", characters[p2SelectedIndex].characterName);
        PlayerPrefs.SetString("GameMode", "PvE");
        PlayerPrefs.SetInt("IsP2AI", 1); // P2 là Bot AI
        PlayerPrefs.Save();

        ChuyenSangBangChonMap();
    }

    private void ChuyenSangBangChonMap()
    {
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