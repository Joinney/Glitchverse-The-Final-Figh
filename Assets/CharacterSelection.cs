using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;  
        public Sprite portraitSprite; 
    }

    [Header("UI Previews")]
    public Image p1PreviewImage; 
    public Image p2PreviewImage; 
    public GameObject startBattleButton;

    [Header("Characters List")]
    public CharacterData[] characters; 

    private int p1SelectedIndex = -1;
    private int p2SelectedIndex = -1;
    private bool isP1Locked = false;
    private bool isP2Locked = false;

    void Start()
    {
        SetImageAlpha(p1PreviewImage, 0f);
        SetImageAlpha(p2PreviewImage, 0f);
    }

    // ==========================================
    // LOGIC 1: RÊ CHUỘT QUA Ô (HOVER) ĐỂ XEM TRƯỚC
    // ==========================================
    public void HoverCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;
        Sprite previewSprite = characters[characterIndex].portraitSprite;

        // Nếu P1 chưa chốt, hiển thị ảnh xem trước mờ mờ cho P1
        if (!isP1Locked)
        {
            p1PreviewImage.sprite = previewSprite;
            SetImageAlpha(p1PreviewImage, 0.6f); 
        }
        // Nếu P1 chốt rồi và P2 chưa chốt, hiển thị ảnh xem trước mờ mờ cho P2
        else if (!isP2Locked) 
        {
            p2PreviewImage.sprite = previewSprite;
            SetImageAlpha(p2PreviewImage, 0.6f); 
        }
    }

    // ==========================================
    // LOGIC 2: CLICK CHUỘT ĐỂ CHỐT CHỌN (LOCK IN)
    // ==========================================
    public void SelectCharacter(int characterIndex)
    {
        Debug.Log("==> LỆNH CLICK ĐÃ NHẬN! Index nhân vật là: " + characterIndex);

        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        // LƯỢT CỦA PLAYER 1
        if (!isP1Locked)
        {
            p1SelectedIndex = characterIndex;
            p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p1PreviewImage, 1f); // Sáng rõ 100%
            isP1Locked = true;
            
            PlayerPrefs.SetString("P1_Selection", characters[characterIndex].characterName);
            Debug.Log("P1 đã khóa thành công: " + characters[characterIndex].characterName);
            return; 
        }
        
        // LƯỢT CỦA PLAYER 2
        if (!isP2Locked)
        {
            p2SelectedIndex = characterIndex;
            p2PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p2PreviewImage, 1f); // Sáng rõ 100%
            isP2Locked = true;

            PlayerPrefs.SetString("P2_Selection", characters[characterIndex].characterName);
            Debug.Log("P2 đã khóa thành công: " + characters[characterIndex].characterName);

            // BẬT NÚT START
            if (startBattleButton != null)
            {
                startBattleButton.SetActive(true);
                Debug.Log("Nút START trận đấu đã được kích hoạt thành công!");
            }
        }
    }

    public void CancelSelection()
    {
        if (isP2Locked)
        {
            isP2Locked = false;
            p2SelectedIndex = -1;
            SetImageAlpha(p2PreviewImage, 0f);
            if (startBattleButton != null) startBattleButton.SetActive(false);
            Debug.Log("P2 đã hủy chọn!");
        }
        else if (isP1Locked)
        {
            isP1Locked = false;
            p1SelectedIndex = -1;
            SetImageAlpha(p1PreviewImage, 0f);
            Debug.Log("P1 đã hủy chọn!");
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
        Debug.Log("Đang tải sàn đấu: " + sceneName);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}