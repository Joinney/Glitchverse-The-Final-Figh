using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class WorldSelectManager : MonoBehaviour
{
    [System.Serializable]
    public class WorldData
    {
        public string worldName;              // Tên Map (vd: ẢI 1: BÃI BIỂN NGUYÊN SINH)
        public string sceneName;              // Tên Scene (MapSinhTon1 -> MapSinhTon5)
        public Sprite islandSprite;           // Ảnh hòn đảo
        [TextArea] public string description; // Mô tả ngắn
    }

    [Header("1. Danh Sách 5 Thế Giới")]
    public WorldData[] worlds;

    [Header("2. UI Hiển Thị 3 Đảo (Vòng Tròn Khép Kín)")]
    public Image currentIslandImage;          // Đảo chính ở giữa
    public Image leftIslandImage;             // Đảo bên trái
    public Image rightIslandImage;            // Đảo bên phải

    [Header("3. UI Thông Tin & Nút Bấm")]
    public TextMeshProUGUI worldNameText;     // Tên Map
    public TextMeshProUGUI descriptionText;   // Mô tả Map
    public Button playButton;                 // Nút PLAY
    public GameObject lockIcon;               // Ổ khóa đảo chính
    public Button nextButton;                 // Nút >
    public Button prevButton;                 // Nút <

    [Header("4. Cài Đặt Khác")]
    public string mainMenuScene = "MainMenu";
    public float floatSpeed = 2f;             // Tốc độ bồng bềnh
    public float floatAmount = 8f;            // Biên độ bồng bềnh

    private int currentIndex = 0;             // Luôn bắt đầu từ Map 1 (Index 0)
    private Vector3 origCenterPos;
    private Vector3 origLeftPos;
    private Vector3 origRightPos;

    void Start()
    {
        currentIndex = 0;

        if (currentIslandImage != null) origCenterPos = currentIslandImage.transform.localPosition;
        if (leftIslandImage != null) origLeftPos = leftIslandImage.transform.localPosition;
        if (rightIslandImage != null) origRightPos = rightIslandImage.transform.localPosition;

        UpdateWorldUI();
    }

    void Update()
    {
        // Hiệu ứng bồng bềnh cho cả 3 đảo
        float wave = Mathf.Sin(Time.time * floatSpeed) * floatAmount;

        if (currentIslandImage != null)
            currentIslandImage.transform.localPosition = new Vector3(origCenterPos.x, origCenterPos.y + wave, origCenterPos.z);

        if (leftIslandImage != null && leftIslandImage.gameObject.activeSelf)
            leftIslandImage.transform.localPosition = new Vector3(origLeftPos.x, origLeftPos.y + Mathf.Sin((Time.time + 0.5f) * floatSpeed) * (floatAmount * 0.7f), origLeftPos.z);

        if (rightIslandImage != null && rightIslandImage.gameObject.activeSelf)
            rightIslandImage.transform.localPosition = new Vector3(origRightPos.x, origRightPos.y + Mathf.Sin((Time.time + 1f) * floatSpeed) * (floatAmount * 0.7f), origRightPos.z);
    }

    // 🔄 BẤM NEXT (>) -> XOAY VÒNG VỀ ẢI 1 KHI HẾT ẢI 5
    public void OnNextButtonClick()
    {
        if (worlds == null || worlds.Length == 0) return;

        currentIndex = (currentIndex + 1) % worlds.Length;
        UpdateWorldUI();
    }

    // 🔄 BẤM PREV (<) -> XOAY VÒNG VỀ ẢI 5 KHI Ở ẢI 1
    public void OnPrevButtonClick()
    {
        if (worlds == null || worlds.Length == 0) return;

        currentIndex = (currentIndex - 1 + worlds.Length) % worlds.Length;
        UpdateWorldUI();
    }

    void UpdateWorldUI()
    {
        if (worlds == null || worlds.Length == 0) return;

        int total = worlds.Length;
        int unlockedStage = PlayerPrefs.GetInt("UnlockedStage", 1);
        bool isUnlocked = (currentIndex + 1) <= unlockedStage;

        // 🏝️ 1. ĐẢO CHÍNH (Ở GIỮA)
        WorldData cur = worlds[currentIndex];
        if (worldNameText != null) worldNameText.text = cur.worldName;
        if (descriptionText != null) descriptionText.text = cur.description;

        if (currentIslandImage != null)
        {
            currentIslandImage.sprite = cur.islandSprite;
            currentIslandImage.color = isUnlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.9f);
        }

        if (lockIcon != null) lockIcon.SetActive(!isUnlocked);
        if (playButton != null)
        {
            playButton.interactable = isUnlocked;
            Image btnImg = playButton.GetComponent<Image>();
            if (btnImg != null) btnImg.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.6f);
        }

        // 👈 2. ĐẢO BÊN TRÁI (LẤY MAP TRƯỚC ĐÓ THEO VÒNG TRÒN, SÁNG 100%)
        int prevIndex = (currentIndex - 1 + total) % total;
        if (leftIslandImage != null)
        {
            leftIslandImage.gameObject.SetActive(true);
            leftIslandImage.sprite = worlds[prevIndex].islandSprite;
            leftIslandImage.color = Color.white;
        }

        // 👉 3. ĐẢO BÊN PHẢI (LẤY MAP TIẾP THEO THEO VÒNG TRÒN, SÁNG 100%)
        int nextIndex = (currentIndex + 1) % total;
        if (rightIslandImage != null)
        {
            rightIslandImage.gameObject.SetActive(true);
            rightIslandImage.sprite = worlds[nextIndex].islandSprite;
            rightIslandImage.color = Color.white;
        }

        // 2 nút mũi tên luôn bật sáng để xoay vòng thoải mái
        if (prevButton != null) prevButton.interactable = true;
        if (nextButton != null) nextButton.interactable = true;
    }

    public void OnPlayClick()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("Current_Stage_Index", currentIndex + 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(worlds[currentIndex].sceneName);
    }

    public void OnBackClick()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}