using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PlayerSkillHUD : MonoBehaviour
{
    public static PlayerSkillHUD instance;

    [Header("Cài Đặt Phím Tắt Ẩn/Hiện")]
    public KeyCode toggleKey = KeyCode.F1;
    public CanvasGroup canvasGroup;

    [Header("Các Bảng Tự Động Ẩn Nút Bấm")]
    public GameObject gameOverPanel;   // Bảng Kết Thúc (Victory/Defeat)
    public GameObject pausePanel;      // ⏸️ Bảng Pause
    public GameObject settingsPanel;   // ⚙️ Bảng Settings

    [Header("Nút Tấn Công & Kỹ Năng")]
    public Button attackBtn;
    public Image attackIconImg;

    public Button skill2Btn;
    public Image skill2IconImg;

    public Button skill3Btn;
    public Image skill3IconImg;

    public Button skill4Btn;
    public Image skill4IconImg;

    public Button dashBtn;
    public Image dashIconImg;

    [Header("Nút Di Chuyển Trái / Phải / Nhảy")]
    public EventTrigger moveLeftTrigger;
    public EventTrigger moveRightTrigger;
    public Button jumpBtn;

    [Header("Cài Đặt Màu Sắc Khi Thiếu Mana")]
    public Color normalColor = Color.white;
    public Color disableColor = new Color(0.35f, 0.35f, 0.35f, 0.65f);

    private CharacterController2D playerCtrl;
    private EnergySystem playerEnergy;
    private bool isGameOver = false;

    private Transform leftGroup;
    private Transform rightGroup;

    void Awake()
    {
        instance = this;
        leftGroup = transform.Find("LeftControlGroup");
        rightGroup = transform.Find("RightSkillGroup");
    }

    void Start()
    {
        FindPlayer();
        SetupButtonEvents();
        AutoFindPanels();
    }

    void AutoFindPanels()
    {
        Transform canvasTrans = transform.parent;
        if (canvasTrans != null)
        {
            if (gameOverPanel == null)
            {
                Transform go = canvasTrans.Find("GameOverPanel");
                if (go != null) gameOverPanel = go.gameObject;
            }
            if (pausePanel == null)
            {
                Transform p = canvasTrans.Find("PausePanel");
                if (p != null) pausePanel = p.gameObject;
            }
            if (settingsPanel == null)
            {
                Transform s = canvasTrans.Find("SETTINGS");
                if (s == null) s = canvasTrans.Find("SettingsPanel");
                if (s != null) settingsPanel = s.gameObject;
            }
        }
    }

    void FindPlayer()
    {
        CharacterController2D[] allChars = FindObjectsByType<CharacterController2D>(FindObjectsSortMode.None);
        foreach (var c in allChars)
        {
            if (!c.isAI && c.playerIndex == 1)
            {
                playerCtrl = c;
                playerEnergy = c.GetComponent<EnergySystem>();
                break;
            }
        }

        if (playerCtrl != null)
        {
            if (playerCtrl.attackIcon != null && attackIconImg != null) attackIconImg.sprite = playerCtrl.attackIcon;
            if (playerCtrl.skill2Icon != null && skill2IconImg != null) skill2IconImg.sprite = playerCtrl.skill2Icon;
            if (playerCtrl.skill3Icon != null && skill3IconImg != null) skill3IconImg.sprite = playerCtrl.skill3Icon;
            if (playerCtrl.skill4Icon != null && skill4IconImg != null) skill4IconImg.sprite = playerCtrl.skill4Icon;
            if (playerCtrl.dashIcon != null && dashIconImg != null) dashIconImg.sprite = playerCtrl.dashIcon;
        }
    }

    void SetupButtonEvents()
    {
        if (attackBtn != null) attackBtn.onClick.AddListener(() => { if (playerCtrl != null) playerCtrl.TriggerAttack(); });
        if (skill2Btn != null) skill2Btn.onClick.AddListener(() => { if (playerCtrl != null) playerCtrl.TriggerSkill2(); });
        if (skill3Btn != null) skill3Btn.onClick.AddListener(() => { if (playerCtrl != null) playerCtrl.TriggerSkill3(); });
        if (skill4Btn != null) skill4Btn.onClick.AddListener(() => { if (playerCtrl != null) playerCtrl.TriggerSkill4(); });
        if (dashBtn != null) dashBtn.onClick.AddListener(() => { if (playerCtrl != null) playerCtrl.TriggerDash(); });
        if (jumpBtn != null) jumpBtn.onClick.AddListener(() => { if (playerCtrl != null) playerCtrl.TriggerJump(); });

        if (moveLeftTrigger != null)
        {
            AddTriggerEvent(moveLeftTrigger, EventTriggerType.PointerDown, () => { if (playerCtrl != null) playerCtrl.OnPointerDownLeft(); });
            AddTriggerEvent(moveLeftTrigger, EventTriggerType.PointerUp, () => { if (playerCtrl != null) playerCtrl.OnPointerUpLeft(); });
        }

        if (moveRightTrigger != null)
        {
            AddTriggerEvent(moveRightTrigger, EventTriggerType.PointerDown, () => { if (playerCtrl != null) playerCtrl.OnPointerDownRight(); });
            AddTriggerEvent(moveRightTrigger, EventTriggerType.PointerUp, () => { if (playerCtrl != null) playerCtrl.OnPointerUpRight(); });
        }
    }

    void AddTriggerEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => action.Invoke());
        trigger.triggers.Add(entry);
    }

    private bool isManuallyHidden = false; // 👁️ Biến nhớ trạng thái tắt thủ công bằng F1

    void Update()
    {
        // 🏁 1. KIỂM TRA HẾT TRẬN (GameOver) -> TẮT HẲN
        if (!isGameOver)
        {
            if (gameOverPanel != null && gameOverPanel.activeInHierarchy)
            {
                HideHUDPermanently();
                return;
            }
        }
        else
        {
            return;
        }

        // ⌨️ 2. BẤM F1 ĐỂ BẬT / TẮT THỦ CÔNG
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleHUD();
        }

        // ⏸️ 3. KIỂM TRA ĐANG MỞ PAUSE HOẶC SETTINGS
        bool isMenuOpen = (pausePanel != null && pausePanel.activeInHierarchy) || 
                         (settingsPanel != null && settingsPanel.activeInHierarchy);

        if (isMenuOpen)
        {
            SetControlsVisible(false);
            return;
        }
        else
        {
            // Chỉ hiện lại nếu người chơi KHÔNG bấm F1 để ẩn trước đó
            if (!isManuallyHidden)
            {
                SetControlsVisible(true);
            }
        }

        // 4. KIỂM TRA VÀ CẬP NHẬT MÀU SẮC SKILL THEO NĂNG LƯỢNG
        if (playerCtrl == null || playerEnergy == null)
        {
            FindPlayer();
            return;
        }

        int curEnergy = playerEnergy.currentEnergy;
        UpdateSkillColor(skill2Btn, skill2IconImg, curEnergy >= playerCtrl.skill2Cost);
        UpdateSkillColor(skill3Btn, skill3IconImg, curEnergy >= playerCtrl.skill3Cost);
        UpdateSkillColor(skill4Btn, skill4IconImg, curEnergy >= playerCtrl.skill4Cost);
    }

    // Hàm ẩn/hiện tạm thời cụm nút điều khiển khi Pause
    private void SetControlsVisible(bool visible)
    {
        if (leftGroup != null && leftGroup.gameObject.activeSelf != visible)
            leftGroup.gameObject.SetActive(visible);

        if (rightGroup != null && rightGroup.gameObject.activeSelf != visible)
            rightGroup.gameObject.SetActive(visible);
    }

    // Hàm tắt vĩnh viễn khi kết thúc game
    public void HideHUDPermanently()
    {
        isGameOver = true;
        gameObject.SetActive(false);

        GameObject potion = GameObject.Find("PotionHUD");
        if (potion != null) potion.SetActive(false);

        GameObject pauseBtn = GameObject.Find("Btn_PauseSettings");
        if (pauseBtn != null) pauseBtn.SetActive(false);
    }

    public void ToggleHUD()
    {
        if (isGameOver) return;

        isManuallyHidden = !isManuallyHidden;
        SetControlsVisible(!isManuallyHidden);
    }

    void UpdateSkillColor(Button btn, Image icon, bool canCast)
    {
        if (btn != null) btn.interactable = canCast;
        if (icon != null) icon.color = canCast ? normalColor : disableColor;
    }
}