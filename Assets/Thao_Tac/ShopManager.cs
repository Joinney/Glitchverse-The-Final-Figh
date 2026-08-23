using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject shopPanel;          // Bảng Shop pop-up
    public TextMeshProUGUI coinText;      // Text hiển thị số xu

    [Header("Item 1: Bình Máu")]
    public int healthPotionPrice = 50;
    public TextMeshProUGUI hpPriceText;
    public TextMeshProUGUI hpCountText;

    [Header("Item 2: Bình Năng Lượng")]
    public int energyPotionPrice = 50;
    public TextMeshProUGUI manaPriceText;
    public TextMeshProUGUI manaCountText;

    void Start()
    {
        // Khởi tạo mặc định là 0 vàng cho người chơi mới
        if (!PlayerPrefs.HasKey("PlayerCoins"))
        {
            PlayerPrefs.SetInt("PlayerCoins", 0);
            PlayerPrefs.Save();
        }

        UpdateShopUI();
    }

    public void OpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        UpdateShopUI();
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    public void BuyHealthPotion()
    {
        int coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        if (coins >= healthPotionPrice)
        {
            coins -= healthPotionPrice;
            int hpCount = PlayerPrefs.GetInt("Item_HealthPotion", 0) + 1;
            PlayerPrefs.SetInt("PlayerCoins", coins);
            PlayerPrefs.SetInt("Item_HealthPotion", hpCount);
            PlayerPrefs.Save();
            UpdateShopUI();
        }
    }

    public void BuyEnergyPotion()
    {
        int coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        if (coins >= energyPotionPrice)
        {
            coins -= energyPotionPrice;
            int manaCount = PlayerPrefs.GetInt("Item_EnergyPotion", 0) + 1;
            PlayerPrefs.SetInt("PlayerCoins", coins);
            PlayerPrefs.SetInt("Item_EnergyPotion", manaCount);
            PlayerPrefs.Save();
            UpdateShopUI();
        }
    }

    public void UpdateShopUI()
    {
        int coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        int hpCount = PlayerPrefs.GetInt("Item_HealthPotion", 0);
        int manaCount = PlayerPrefs.GetInt("Item_EnergyPotion", 0);

        if (coinText != null) coinText.text = coins.ToString();
        if (hpPriceText != null) hpPriceText.text = healthPotionPrice.ToString();
        if (hpCountText != null) hpCountText.text = "Sở hữu: " + hpCount;

        if (manaPriceText != null) manaPriceText.text = energyPotionPrice.ToString();
        if (manaCountText != null) manaCountText.text = "Sở hữu: " + manaCount;
    }
}