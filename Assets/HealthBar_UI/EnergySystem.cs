using UnityEngine;
using UnityEngine.UI;

public class EnergySystem : MonoBehaviour
{
    [Header("Thông số Năng Lượng")]
    public int maxEnergy = 100;
    public int currentEnergy;

    [Header("Tên của Thanh UI ngoài màn hình")]
    public string energyUIName = "Energy_Fill_P1"; // Gõ tên UI vào đây

    private Image energyFill; // Đã giấu đi vì code sẽ tự tìm

    void Start()
    {
        // Bắt đầu game với 0 năng lượng
        currentEnergy = 50; 
        
        // Bật Radar tự động tìm thanh UI dựa vào tên bạn gõ
        GameObject uiObject = GameObject.Find(energyUIName);
        if (uiObject != null)
        {
            energyFill = uiObject.GetComponent<Image>();
        }

        UpdateEnergyUI();
    }

    public void AddEnergy(int amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        UpdateEnergyUI();
    }

    public bool UseEnergy(int cost)
    {
        if (currentEnergy >= cost)
        {
            currentEnergy -= cost;
            UpdateEnergyUI();
            return true; 
        }
        
        Debug.Log(gameObject.name + " không đủ năng lượng!");
        return false;
    }

    private void UpdateEnergyUI()
    {
        if (energyFill != null)
        {
            energyFill.fillAmount = (float)currentEnergy / maxEnergy;
        }
    }
}