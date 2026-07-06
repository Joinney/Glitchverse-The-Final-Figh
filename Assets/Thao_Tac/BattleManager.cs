using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform p1SpawnPoint;
    public Transform aiSpawnPoint;

    [Header("Character Prefabs")]
    public GameObject luffyPrefab;
    public GameObject zenitsuPrefab;
    public GameObject zoroPrefab;

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
            p1.name = "Player_" + p1Name.ToLower(); // Đặt tên chuẩn theo chuỗi nhận được
            p1.tag = "Player"; // Gán tag tự động để AI tìm mục tiêu

            // 🔥 TỰ ĐỘNG GÁN SCRIPT ĐIỀU KHIỂN BẰNG TAY CHO NGƯỜI CHƠI (Nếu chưa có)
            if (p1.GetComponent<PlayerMovement>() == null)
            {
                p1.AddComponent<PlayerMovement>();
            }
            
            Debug.Log($"[HỆ THỐNG] Đã gán quyền điều khiển bằng tay cho: {p1.name}");
        }
        else
        {
            Debug.LogError("[HỆ THỐNG LỖI] Không tìm thấy Prefab người chơi hoặc P1 Spawn Point!");
        }

        // 2. ẢI 1 CỐ ĐỊNH ĐỐI THỦ LÀ ZENITSU
        if (zenitsuPrefab != null && aiSpawnPoint != null)
        {
            // Sinh ra thể xác đối thủ Zenitsu
            GameObject ai = Instantiate(zenitsuPrefab, aiSpawnPoint.position, Quaternion.identity);
            ai.name = "AI_Zenitsu";
            
            // Lật mặt đối thủ hướng về phía Player (bên trái)
            ai.transform.localScale = new Vector3(-Mathf.Abs(ai.transform.localScale.x), ai.transform.localScale.y, ai.transform.localScale.z);
            
            // 🔥 TỰ ĐỘNG GÁN SCRIPT AI TỰ ĐỘNG CHO ĐỐI THỦ (Nếu chưa có)
            if (ai.GetComponent<SimpleAI>() == null)
            {
                ai.AddComponent<SimpleAI>();
            }

            Debug.Log("[HỆ THỐNG] Đã gán bộ não tự động AI cho: AI_Zenitsu");
        }
        else
        {
            Debug.LogError("[HỆ THỐNG LỖI] Không tìm thấy Zenitsu Prefab hoặc AI Spawn Point!");
        }
    }

    /// <summary>
    /// Lấy Prefab nhân vật dựa vào tên (Không phân biệt chữ hoa/thường)
    /// </summary>
    GameObject GetPrefabByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return luffyPrefab;

        // Chuyển hết chuỗi truyền vào thành chữ thường để so sánh chính xác 100%
        string lowerName = name.ToLower().Trim(); 

        if (lowerName == "luffy") return luffyPrefab;
        if (lowerName == "zenitsu") return zenitsuPrefab;
        if (lowerName == "zoro") return zoroPrefab;

        // Nếu người chơi lưu một tên lạ hoắc không có trong danh sách, mặc định trả về Luffy
        Debug.LogWarning($"[HỆ THỐNG] Không tìm thấy prefab cho tên '{name}'. Tự động chọn Luffy.");
        return luffyPrefab;
    }
}