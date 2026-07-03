using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform p1SpawnPoint;
    public Transform aiSpawnPoint;

    [Header("Character Prefabs")]
    public GameObject luffyPrefab;
    public GameObject zenitsuPrefab;

    void Start()
    {
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        // 1. ĐỌC NHÂN VẬT BẠN ĐÃ CHỌN TỪ MENU (Mặc định nếu trống là Luffy)
        string p1Name = PlayerPrefs.GetString("P1_Selection", "Luffy");
        GameObject p1PrefabToSpawn = GetPrefabByName(p1Name);

        if (p1PrefabToSpawn != null && p1SpawnPoint != null)
        {
            // Sinh ra thể xác người chơi
            GameObject p1 = Instantiate(p1PrefabToSpawn, p1SpawnPoint.position, Quaternion.identity);
            p1.name = "Player_" + p1Name;
            p1.tag = "Player"; // Gán tag tự động để AI tìm mục tiêu

            // 🔥 TỰ ĐỘNG GÁN SCRIPT ĐIỀU KHIỂN BẰNG TAY CHO NGƯỜI CHƠI
            p1.AddComponent<PlayerMovement>(); 
            
            Debug.Log($"[HỆ THỐNG] Đã gán quyền điều khiển bằng tay cho: {p1.name}");
        }

        // 2. ẢI 1 CỐ ĐỊNH ĐỐI THỦ LÀ ZENITSU
        if (zenitsuPrefab != null && aiSpawnPoint != null)
        {
            // Sinh ra thể xác đối thủ Zenitsu
            GameObject ai = Instantiate(zenitsuPrefab, aiSpawnPoint.position, Quaternion.identity);
            ai.name = "AI_Zenitsu";
            
            // Lật mặt đối thủ hướng về phía Player
            ai.transform.localScale = new Vector3(-Mathf.Abs(ai.transform.localScale.x), ai.transform.localScale.y, ai.transform.localScale.z);
            
            // 🔥 TỰ ĐỘNG GÁN SCRIPT AI TỰ ĐỘNG CHO ĐỐI THỦ
            ai.AddComponent<SimpleAI>();

            Debug.Log("[HỆ THỐNG] Đã gán bộ não tự động AI cho: AI_Zenitsu");
        }
    }

    GameObject GetPrefabByName(string name)
    {
        if (name == "Luffy") return luffyPrefab;
        if (name == "Zenitsu") return zenitsuPrefab;
        return luffyPrefab;
    }
}