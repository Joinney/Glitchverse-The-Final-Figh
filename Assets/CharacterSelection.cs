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

    [Header("Characters List")]
    public CharacterData[] characters; 

    // Quản lý trạng thái chọn của 2 người chơi
    private int p1SelectedIndex = -1;
    private int p2SelectedIndex = -1;
    private bool isP1Locked = false;
    private bool isP2Locked = false;

    void Start()
    {
        // Lúc mới vào game, đặt 2 khung Preview về trạng thái trong suốt (không bị ô trắng che)
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

        // Nếu P1 chưa chốt, hiển thị ảnh xem trước cho P1
        if (!isP1Locked)
        {
            p1PreviewImage.sprite = previewSprite;
            SetImageAlpha(p1PreviewImage, 0.6f); // Hiện mờ mờ 60% khi đang xem thử
        }
        // Nếu P1 chốt rồi và P2 chưa chốt, hiển thị ảnh xem trước cho P2
        else if (!isP2Locked) 
{
    Debug.Log("Đang cố gắng gán cho P2..."); // Thêm dòng này để kiểm tra
    
    if (p2PreviewImage == null) Debug.LogError("P2 Preview Image chưa được gán trong Inspector!");
    
    p2SelectedIndex = characterIndex;
    p2PreviewImage.sprite = characters[characterIndex].portraitSprite;
    SetImageAlpha(p2PreviewImage, 1f); 
    isP2Locked = true;

    PlayerPrefs.SetString("P2_Selection", characters[characterIndex].characterName);
    Debug.Log("P2 ĐÃ KHÓA: " + characters[characterIndex].characterName);
}
    }

// ==========================================
    // LOGIC 2: CLICK CHUỘT ĐỂ CHỐT CHỌN (LOCK IN)
    // ==========================================
    public void SelectCharacter(int characterIndex)
    {
        // ÉP UNITY IN RA LOG NGAY KHI VỪA CLICK CHUỘT
        Debug.Log("HÀM SELECT CHARACTER ĐÃ ĐƯỢC KÍCH HOẠT! Index truyền vào là: " + characterIndex);

        if (characterIndex < 0 || characterIndex >= characters.Length) 
        {
            Debug.LogError("LỖI: Index bị vượt quá danh sách nhân vật!");
            return;
        }
        if (!isP1Locked)
        {
            p1SelectedIndex = characterIndex;
            p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p1PreviewImage, 1f); // Hiện rõ nét 100% khi đã chốt
            isP1Locked = true;
            
            PlayerPrefs.SetString("P1_Selection", characters[characterIndex].characterName);
            Debug.Log("P1 ĐÃ KHÓA: " + characters[characterIndex].characterName);
        }
        else if (!isP2Locked && characterIndex != p1SelectedIndex) // Tránh P2 chọn trùng ô P1 vừa chọn nếu muốn
        {
            p2SelectedIndex = characterIndex;
            p2PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p2PreviewImage, 1f); // Hiện rõ nét 100% khi đã chốt
            isP2Locked = true;

            PlayerPrefs.SetString("P2_Selection", characters[characterIndex].characterName);
            Debug.Log("P2 ĐÃ KHÓA: " + characters[characterIndex].characterName);
            
            // Cả 2 chọn xong -> Kích hoạt nút START trận đấu tại đây
        }
    }

    // ==========================================
    // LOGIC 3: HỦY CHỌN ĐỂ CHỌN LẠI (CANCEL)
    // ==========================================
    void Update()
    {
        // Người chơi bấm nút ESC hoặc chuột phải để HỦY CHỌN quay lại lượt trước
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            CancelSelection();
        }
    }

    public void CancelSelection()
    {
        // Nếu P2 đã khóa -> Hủy khóa P2 trước
        if (isP2Locked)
        {
            isP2Locked = false;
            p2SelectedIndex = -1;
            SetImageAlpha(p2PreviewImage, 0f); // Trở lại trong suốt
            Debug.Log("P2 đã hủy chọn!");
        }
        // Nếu P2 chưa khóa nhưng P1 đã khóa -> Hủy khóa P1
        else if (isP1Locked)
        {
            isP1Locked = false;
            p1SelectedIndex = -1;
            SetImageAlpha(p1PreviewImage, 0f); // Trở lại trong suốt
            Debug.Log("P1 đã hủy chọn!");
        }
    }

    // Hàm phụ trợ xử lý độ trong suốt (Alpha) của ảnh UI
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