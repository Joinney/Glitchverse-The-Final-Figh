using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform p1SpawnPoint;
    public Transform aiSpawnPoint; // Dùng chung điểm này làm điểm spawn cho P2 luôn

    [Header("Character Prefabs (Bản Người Chơi Bấm Phím)")]
    public GameObject luffyPrefab;
    public GameObject zenitsuPrefab;
    public GameObject zoroPrefab;
    public GameObject narutoPrefab;
    public GameObject mihawkPrefab;
    public GameObject gojoPrefab;
    public GameObject tomPrefab;
    public GameObject tatsumakiPrefab;

    [Header("AI Prefabs (Bản Đối Thủ Tự Động)")]
    public GameObject zenitsuEnemyAIPrefab;

    void Start()
    {
        // Kiểm tra xem người chơi vào map bằng chế độ nào
        string gameMode = PlayerPrefs.GetString("GameMode", "Single");

        if (gameMode == "PvP")
        {
            SpawnPvPPlayers(); // Chạy chế độ Đối Kháng 2 Người
        }
        else
        {
            SpawnSinglePlayer(); // Chạy chế độ Leo Tháp (cũ của bạn)
        }
    }

    // ==========================================
    // CHẾ ĐỘ 2 NGƯỜI CHƠI (PVP)
    // ==========================================
    void SpawnPvPPlayers()
    {
        Debug.Log("--- KHỞI ĐỘNG CHẾ ĐỘ ĐỐI KHÁNG PVP ---");

        // 1. LẤY TÊN 2 NHÂN VẬT TỪ MENU
        string p1Name = PlayerPrefs.GetString("P1_Selection", "Luffy");
        string p2Name = PlayerPrefs.GetString("P2_Selection", "Tom");

        // 2. SINH RA PLAYER 1 (Bên trái)
        GameObject p1PrefabToSpawn = GetPrefabByName(p1Name);
        if (p1PrefabToSpawn != null && p1SpawnPoint != null)
        {
            GameObject p1 = Instantiate(p1PrefabToSpawn, p1SpawnPoint.position, Quaternion.identity);
            p1.name = "P1_" + p1Name;
            p1.tag = "Player"; // Tag cho P1

            CharacterController2D controller = p1.GetComponent<CharacterController2D>();
            if (controller != null)
            {
                controller.isAI = false;
                controller.playerIndex = 1; // P1 DÙNG NÚT WASD
            }
        }

        // 3. SINH RA PLAYER 2 (Bên phải - Dùng chung vị trí aiSpawnPoint)
        GameObject p2PrefabToSpawn = GetPrefabByName(p2Name);
        if (p2PrefabToSpawn != null && aiSpawnPoint != null)
        {
            GameObject p2 = Instantiate(p2PrefabToSpawn, aiSpawnPoint.position, Quaternion.identity);
            p2.name = "P2_" + p2Name;
            p2.tag = "Enemy"; // Tạm thời để tag Enemy để hệ thống máu của bạn (nếu có) không bị lỗi

            // Lật mặt P2 quay sang trái
            p2.transform.localScale = new Vector3(-Mathf.Abs(p2.transform.localScale.x), p2.transform.localScale.y, p2.transform.localScale.z);

            CharacterController2D controller = p2.GetComponent<CharacterController2D>();
            if (controller != null)
            {
                controller.isAI = false;    // 🔥 ÉP BUỘC TẮT AI
                controller.playerIndex = 2; // 🔥 GÁN INDEX = 2 ĐỂ P2 DÙNG NÚT MŨI TÊN
            }
        }
    }

    // ==========================================
    // CHẾ ĐỘ 1 NGƯỜI CHƠI LEO THÁP (GIỮ NGUYÊN CODE CŨ CỦA BẠN)
    // ==========================================
    void SpawnSinglePlayer()
    {
        Debug.Log("--- KHỞI ĐỘNG CHẾ ĐỘ LEO THÁP AI ---");

        string p1Name = PlayerPrefs.GetString("P1_Selection", "Luffy");
        GameObject p1PrefabToSpawn = GetPrefabByName(p1Name);

        if (p1PrefabToSpawn != null && p1SpawnPoint != null)
        {
            GameObject p1 = Instantiate(p1PrefabToSpawn, p1SpawnPoint.position, Quaternion.identity);
            p1.name = "Player_" + p1Name;
            p1.tag = "Player";

            CharacterController2D controller = p1.GetComponent<CharacterController2D>();
            if (controller != null)
            {
                controller.isAI = false;
                controller.playerIndex = 1; // Đảm bảo P1 đi ải cũng dùng WASD
            }
        }

        // SINH ĐỐI THỦ AI
        GameObject enemyPrefabToSpawn = zenitsuEnemyAIPrefab != null ? zenitsuEnemyAIPrefab : zenitsuPrefab;

        if (enemyPrefabToSpawn != null && aiSpawnPoint != null)
        {
            GameObject ai = Instantiate(enemyPrefabToSpawn, aiSpawnPoint.position, Quaternion.identity);
            ai.name = "AI_Zenitsu";
            ai.tag = "Enemy";

            CharacterController2D controller = ai.GetComponent<CharacterController2D>();
            if (controller != null)
            {
                controller.isAI = true; // BẬT NÃO AI
            }

            ai.transform.localScale = new Vector3(-Mathf.Abs(ai.transform.localScale.x), ai.transform.localScale.y, ai.transform.localScale.z);
        }
    }

    /// <summary>
    /// Lấy Prefab nhân vật dựa vào tên
    /// </summary>
    GameObject GetPrefabByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return luffyPrefab;

        string lowerName = name.ToLower().Trim();

        if (lowerName == "luffy") return luffyPrefab;
        if (lowerName == "zenitsu") return zenitsuPrefab;
        if (lowerName == "zoro") return zoroPrefab;
        if (lowerName == "naruto") return narutoPrefab;
        if (lowerName == "mihawk") return mihawkPrefab;
        if (lowerName == "gojo") return gojoPrefab;
        if (lowerName == "tom") return tomPrefab;
        if (lowerName == "tatsumaki") return tatsumakiPrefab;

        return luffyPrefab;
    }
}