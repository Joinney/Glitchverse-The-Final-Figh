using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform p1SpawnPoint;
    public Transform aiSpawnPoint;

    [Header("Character Prefabs (Bản Người Chơi Bấm Phím)")]
    public GameObject luffyPrefab;
    public GameObject zenitsuPrefab;
    public GameObject zoroPrefab;
    public GameObject narutoPrefab;
    public GameObject mihawkPrefab;
    public GameObject gojoPrefab;
    public GameObject tomPrefab;

    [Header("AI Prefabs (Bản Đối Thủ Tự Động)")]
    public GameObject zenitsuEnemyAIPrefab; 

    void Start()
    {
        SpawnPlayers();
    }

void SpawnPlayers()
    {
        // 1. ĐỌC NHÂN VẬT BẠN ĐÃ CHỌN TỪ MENU
        string p1Name = PlayerPrefs.GetString("P1_Selection", "Luffy");
        GameObject p1PrefabToSpawn = GetPrefabByName(p1Name);

        if (p1PrefabToSpawn != null && p1SpawnPoint != null)
        {
            GameObject p1 = Instantiate(p1PrefabToSpawn, p1SpawnPoint.position, Quaternion.identity);
            p1.name = "Player_" + p1Name;
            p1.tag = "Player"; 

            // 🔥 ÉP BUỘC PHE TA PHẢI TẮT AI ĐỂ ĐIỀU KHIỂN BẰNG TAY
            CharacterController2D controller = p1.GetComponent<CharacterController2D>();
            if (controller != null)
            {
                controller.isAI = false;
            }
            
            Debug.Log($"[HỆ THỐNG] Đã sinh ra Người chơi: {p1.name}");
        }
        else
        {
            Debug.LogError("[HỆ THỐNG LỖI] Không tìm thấy Prefab người chơi hoặc P1 Spawn Point!");
        }

        // 2. SỬ THẾ ĐỐI THỦ AI
        GameObject enemyPrefabToSpawn = zenitsuEnemyAIPrefab != null ? zenitsuEnemyAIPrefab : zenitsuPrefab;

        if (enemyPrefabToSpawn != null && aiSpawnPoint != null)
        {
            GameObject ai = Instantiate(enemyPrefabToSpawn, aiSpawnPoint.position, Quaternion.identity);
            ai.name = "AI_Zenitsu";
            ai.tag = "Enemy";
            
            // 🔥 ÉP BUỘC ĐỐI THỦ PHẢI BẬT AI ĐỂ TỰ ĐỘNG CHẠY/ĐÁNH
            CharacterController2D controller = ai.GetComponent<CharacterController2D>();
            if (controller != null)
            {
                controller.isAI = true;
            }

            ai.transform.localScale = new Vector3(-Mathf.Abs(ai.transform.localScale.x), ai.transform.localScale.y, ai.transform.localScale.z);
            
            Debug.Log("[HỆ THỐNG] Đã sinh ra Đối thủ AI và ép kích hoạt bộ não tự động.");
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
        if (lowerName == "naruto") return narutoPrefab;
        if (lowerName == "mihawk") return mihawkPrefab;
        if (lowerName == "gojo") return gojoPrefab;
        if (lowerName == "tom") return tomPrefab;
        // Nếu người chơi lưu một tên lạ hoắc không có trong danh sách, mặc định trả về Luffy
        Debug.LogWarning($"[HỆ THỐNG] Không tìm thấy prefab cho tên '{name}'. Tự động chọn Luffy.");
        return luffyPrefab;
    }
}