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
    public Image p2PreviewImage;
    public GameObject startBattleButton;

    [Header("UI Models (Animations)")]
    public Image p1ModelAnimImage;
    public Image p2ModelAnimImage;

    [Header("Characters List")]
    public CharacterData[] characters;

    [Header("Cấu hình Scene")]
    // Đã đổi sang SampleScene (Màn hình chọn Map)
    public string nextSceneName = "SampleScene";

    private int currentSelectedIndex = -1;
    private Coroutine p1AnimCoroutine;

    void Start()
    {
        currentSelectedIndex = -1;
        SetImageAlpha(p1PreviewImage, 0f);
        SetImageAlpha(p1ModelAnimImage, 0f);
        SetImageAlpha(p2PreviewImage, 0f);
        SetImageAlpha(p2ModelAnimImage, 0f);

        if (startBattleButton != null) startBattleButton.SetActive(false);
    }

    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        currentSelectedIndex = characterIndex;

        // 1. Cập nhật ảnh đại diện
        p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
        SetImageAlpha(p1PreviewImage, 1f);

        // 2. Chạy Animation
        if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
        p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));

        // 3. Kích hoạt nút START
        if (startBattleButton != null) startBattleButton.SetActive(true);

        Debug.Log("Đang xem thử: " + characters[characterIndex].characterName);
    }

    public void OnStartButtonClicked()
    {
        if (currentSelectedIndex == -1) return;

        string chosenP1 = characters[currentSelectedIndex].characterName;
        
        // Lưu Tên và ID số thứ tự của nhân vật (để MapSinhTon1 load đúng tướng)
        PlayerPrefs.SetString("P1_Selection", chosenP1);
        PlayerPrefs.SetInt("SelectedCharacter", currentSelectedIndex);

        // Chọn ngẫu nhiên đối thủ AI
        int randomAI = Random.Range(0, characters.Length);
        PlayerPrefs.SetString("P2_Selection", characters[randomAI].characterName);
        PlayerPrefs.SetInt("SelectedAI", randomAI);

        PlayerPrefs.Save();

        Debug.Log($"Đã chọn: {chosenP1} (ID: {currentSelectedIndex}) -> Chuyển sang: {nextSceneName}");

        // Chuyển sang SampleScene
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