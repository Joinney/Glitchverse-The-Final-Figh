using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PlayerSkillHUD : MonoBehaviour
{
    [Header("Cài Đặt Phím Tắt Ẩn/Hiện")]
    public KeyCode toggleKey = KeyCode.F1; // ⌨️ Phím F1 để bật/tắt HUD
    public CanvasGroup canvasGroup;       // (Tùy chọn) Để ẩn/hiện mượt mà hoặc tắt trực tiếp

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

    void Start()
    {
        FindPlayer();
        SetupButtonEvents();
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

    void Update()
    {
        // ⌨️ BẤM F1 ĐỂ BẬT / TẮT ẨN GIAO DIỆN NÚT BẤM
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleHUD();
        }

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

    // Hàm chuyển đổi bật / tắt hiển thị
    public void ToggleHUD()
    {
        // Cách 1: Dùng CanvasGroup nếu có gán
        if (canvasGroup != null)
        {
            bool isVisible = canvasGroup.alpha > 0;
            canvasGroup.alpha = isVisible ? 0 : 1;
            canvasGroup.interactable = !isVisible;
            canvasGroup.blocksRaycasts = !isVisible;
        }
        else
        {
            // Cách 2: Bật tắt trực tiếp các nhóm nút con bên trong
            Transform leftGroup = transform.Find("LeftControlGroup");
            Transform rightGroup = transform.Find("RightSkillGroup");

            if (leftGroup != null) leftGroup.gameObject.SetActive(!leftGroup.gameObject.activeSelf);
            if (rightGroup != null) rightGroup.gameObject.SetActive(!rightGroup.gameObject.activeSelf);
        }
    }

    void UpdateSkillColor(Button btn, Image icon, bool canCast)
    {
        if (btn != null) btn.interactable = canCast;
        if (icon != null) icon.color = canCast ? normalColor : disableColor;
    }
}