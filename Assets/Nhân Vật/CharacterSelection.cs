using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelection : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;  
        public Sprite portraitSprite; // Ảnh chân dung tĩnh nửa thân
        public Sprite[] fightIdleSprites; // DANH SÁCH KHUNG HÌNH TOÀN THÂN CHIẾN ĐẤU (Kéo thả cụm ảnh động vào đây)
    }

    [Header("UI Previews (Portraits)")]
    public Image p1PreviewImage; 
    public Image p2PreviewImage; 
    public GameObject startBattleButton;

    [Header("UI Models (Animations)")]
    public Image p1ModelAnimImage; // Kéo Object Luffy_ModelAnim vào đây
    public Image p2ModelAnimImage; // Kéo Object Zenitsu_ModelAnim vào đây

    [Header("Characters List")]
    public CharacterData[] characters; 

    private int p1SelectedIndex = -1;
    private int p2SelectedIndex = -1;
    private bool isP1Locked = false;
    private bool isP2Locked = false;

    // Quản lý Coroutine chạy animation tự chế bằng code để đổi hình liên tục
    private Coroutine p1AnimCoroutine;
    private Coroutine p2AnimCoroutine;

    void Start()
    {
        // Ẩn toàn bộ ảnh chân dung và ảnh động lúc chưa chọn
        SetImageAlpha(p1PreviewImage, 0f);
        SetImageAlpha(p2PreviewImage, 0f);
        SetImageAlpha(p1ModelAnimImage, 0f);
        SetImageAlpha(p2ModelAnimImage, 0f);
    }

    // ==========================================
    // LOGIC 1: RÊ CHUỘT QUA Ô (HOVER) ĐỂ XEM TRƯỚC
    // ==========================================
    public void HoverCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        if (!isP1Locked)
        {
            // Đổi ảnh chân dung mờ 60%
            p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p1PreviewImage, 0.6f); 

            // Chạy animation toàn thân tương ứng cho P1 (mờ 60%)
            if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
            p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 0.6f));
        }
        else if (!isP2Locked) 
        {
            // Đổi ảnh chân dung mờ 60%
            p2PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p2PreviewImage, 0.6f); 

            // Chạy animation toàn thân tương ứng cho P2 (mờ 60%)
            if (p2AnimCoroutine != null) StopCoroutine(p2AnimCoroutine);
            p2AnimCoroutine = StartCoroutine(PlayModelAnimation(p2ModelAnimImage, characters[characterIndex].fightIdleSprites, 0.6f));
        }
    }

    // ==========================================
    // LOGIC 2: CLICK CHUỘT ĐỂ CHỐT CHỌN (LOCK IN)
    // ==========================================
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        if (!isP1Locked)
        {
            p1SelectedIndex = characterIndex;
            p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p1PreviewImage, 1f); // Sáng rõ 100%
            isP1Locked = true;
            
            // Ép animation của P1 sáng rõ 100%
            if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
            p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));

            PlayerPrefs.SetString("P1_Selection", characters[characterIndex].characterName);
            return; 
        }
        
        if (!isP2Locked)
        {
            p2SelectedIndex = characterIndex;
            p2PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p2PreviewImage, 1f); // Sáng rõ 100%
            isP2Locked = true;

            // Ép animation của P2 sáng rõ 100%
            if (p2AnimCoroutine != null) StopCoroutine(p2AnimCoroutine);
            p2AnimCoroutine = StartCoroutine(PlayModelAnimation(p2ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));

            PlayerPrefs.SetString("P2_Selection", characters[characterIndex].characterName);

            if (startBattleButton != null) startBattleButton.SetActive(true);
        }
    }

    // Coroutine tự động lặp các khung hình để tạo chuyển động mượt mà
    private IEnumerator PlayModelAnimation(Image targetImage, Sprite[] sprites, float alpha)
    {
        if (targetImage == null || sprites == null || sprites.Length == 0) yield break;
        
        SetImageAlpha(targetImage, alpha);
        int frameIndex = 0;

        while (true)
        {
            targetImage.sprite = sprites[frameIndex];
            frameIndex = (frameIndex + 1) % sprites.Length;
            yield return new WaitForSeconds(0.1f); // Tốc độ đổi khung hình (0.1 giây = 10fps)
        }
    }

    public void CancelSelection()
    {
        if (isP2Locked)
        {
            isP2Locked = false;
            p2SelectedIndex = -1;
            SetImageAlpha(p2PreviewImage, 0f);
            SetImageAlpha(p2ModelAnimImage, 0f);
            if (p2AnimCoroutine != null) StopCoroutine(p2AnimCoroutine);
            if (startBattleButton != null) startBattleButton.SetActive(false);
        }
        else if (isP1Locked)
        {
            isP1Locked = false;
            p1SelectedIndex = -1;
            SetImageAlpha(p1PreviewImage, 0f);
            SetImageAlpha(p1ModelAnimImage, 0f);
            if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
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

    public void LoadBattleScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}