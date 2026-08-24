using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemQuickUseHUD : MonoBehaviour
{
    [Header("1. UI Bình Máu")]
    public Button hpButton;
    public TextMeshProUGUI hpCountText;

    [Header("2. UI Bình Mana")]
    public Button manaButton;
    public TextMeshProUGUI manaCountText;

    [Header("3. Phím Tắt Sử Dụng")]
    public KeyCode useHpKey = KeyCode.Alpha1;    // Phím số 1
    public KeyCode useManaKey = KeyCode.Alpha2;  // Phím số 2

    private PlayerHealth playerHealth;
    private EnergySystem energySys;

    void Start()
    {
        FindPlayerComponents();
        UpdateItemDisplay();

        if (hpButton != null) hpButton.onClick.AddListener(UseHealthPotion);
        if (manaButton != null) manaButton.onClick.AddListener(UseEnergyPotion);
    }

    void Update()
    {
        // ⌨️ BẤM PHÍM TẮT ĐỂ DÙNG VẬT PHẨM
        if (Input.GetKeyDown(useHpKey))
        {
            UseHealthPotion();
        }

        if (Input.GetKeyDown(useManaKey))
        {
            UseEnergyPotion();
        }
    }

    void FindPlayerComponents()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            energySys = player.GetComponent<EnergySystem>();
        }
    }

    // 🩸 HỒI 25% MÁU TỐI ĐA
    public void UseHealthPotion()
    {
        int count = PlayerPrefs.GetInt("Item_HealthPotion", 0);
        if (count <= 0) return;

        if (playerHealth == null) FindPlayerComponents();
        if (playerHealth == null || playerHealth.currentHealth <= 0) return;

        // Nếu máu đã đầy 100% thì không dùng lãng phí
        if (playerHealth.currentHealth >= playerHealth.maxHealth) return;

        int healAmount = Mathf.RoundToInt(playerHealth.maxHealth * 0.25f);
        playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth + healAmount, playerHealth.maxHealth);

        // Cập nhật thanh máu UI
        HealthBarUI p1Bar = GameObject.Find("HealthBar_P1")?.GetComponent<HealthBarUI>();
        if (p1Bar != null)
        {
            p1Bar.SetHealth(playerHealth.currentHealth, playerHealth.maxHealth);
        }

        // Hiện chữ hồi máu màu xanh lá
        DamagePopup.Create(playerHealth.transform.position + Vector3.up * 1.5f, healAmount);

        // Trừ 1 bình máu và lưu
        PlayerPrefs.SetInt("Item_HealthPotion", count - 1);
        PlayerPrefs.Save();
        UpdateItemDisplay();
    }

    // ⚡ HỒI 50% NĂNG LƯỢNG TỐI ĐA
    public void UseEnergyPotion()
    {
        int count = PlayerPrefs.GetInt("Item_EnergyPotion", 0);
        if (count <= 0) return;

        if (energySys == null) FindPlayerComponents();
        if (energySys == null) return;

        // Giả sử max energy là 100 -> hồi 50
        int energyAmount = 50;
        energySys.AddEnergy(energyAmount);

        // Trừ 1 bình mana và lưu
        PlayerPrefs.SetInt("Item_EnergyPotion", count - 1);
        PlayerPrefs.Save();
        UpdateItemDisplay();
    }

    public void UpdateItemDisplay()
    {
        int hpCount = PlayerPrefs.GetInt("Item_HealthPotion", 0);
        int manaCount = PlayerPrefs.GetInt("Item_EnergyPotion", 0);

        if (hpCountText != null) hpCountText.text = hpCount.ToString();
        if (manaCountText != null) manaCountText.text = manaCount.ToString();

        if (hpButton != null) hpButton.interactable = (hpCount > 0);
        if (manaButton != null) manaButton.interactable = (manaCount > 0);
    }
}