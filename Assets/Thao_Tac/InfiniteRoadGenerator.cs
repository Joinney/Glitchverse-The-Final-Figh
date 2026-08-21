using System.Collections.Generic;
using UnityEngine;

public class InfiniteRoadGenerator : MonoBehaviour
{
    [Header("1. Danh Sách Nhân Vật Chọn Từ Menu")]
    [Tooltip("Thứ tự Prefab phải khớp với index khi chọn ở MainMenu (0: Gojo, 1: Luffy, 2: Mihawk...)")]
    public GameObject[] characterPrefabs;
    public Transform spawnPoint;

    [Header("2. Cài Đặt Sinh Đường")]
    public GameObject groundPrefab;
    public float pieceWidth = 20f;
    public int initialPieces = 3;
    public float spawnDistance = 15f;
    public float totalRoadLength = 120f;

    [Header("3. Cài Đặt Quái Nhỏ")]
    public GameObject[] minionPrefabs; // Kéo Prefab Goblin, Skeleton vào đây
    public float minionSpawnInterval = 12f;
    private float nextMinionX = 15f;
    private List<GameObject> spawnedMinions = new List<GameObject>();

    [Header("4. Cài Đặt Mini Boss (Dark Wolf)")]
    public GameObject miniBossPrefab; // Ô kéo Prefab Sói Mini Boss
    private GameObject activeMiniBoss;
    private bool miniBossSpawned = false;
    private bool miniBossDefeated = false;

    [Header("5. Cài Đặt Boss Cuối Đường")]
    public GameObject mainBossPrefab; // Ô kéo Prefab Boss chính (Tatsumaki_AI)
    private bool mainBossSpawned = false;

    private Transform activePlayer;
    private float currentSpawnX = 0f;

    void Awake()
    {
        SpawnSelectedPlayer();
        CreateInvisibleWall("Wall_Start", -3f); // Tường chặn đầu map
    }

    void Start()
    {
        for (int i = 0; i < initialPieces; i++)
        {
            SpawnGroundPiece();
        }
    }

    void Update()
    {
        if (activePlayer == null) return;

        // 1. Sinh đất cho tới khi vượt qua mốc kết thúc 1 mảnh
        if (currentSpawnX - activePlayer.position.x < spawnDistance && currentSpawnX <= totalRoadLength + pieceWidth)
        {
            SpawnGroundPiece();
        }

        // 2. Sinh quái nhỏ dọc đường (dừng sinh trước mốc xuất hiện Mini Boss)
        if (activePlayer.position.x > nextMinionX - 20f && nextMinionX < totalRoadLength - 25f)
        {
            SpawnMinion();
        }

        // 3. Tự động loại bỏ quái đã bị tiêu diệt khỏi danh sách
        spawnedMinions.RemoveAll(m => m == null);

        // 4. KIỂM TRA ĐIỀU KIỆN XUẤT HIỆN MINI BOSS:
        // Đi gần tới cuối đường VÀ đã dọn sạch toàn bộ quái nhỏ
        if (!miniBossSpawned && currentSpawnX >= totalRoadLength && activePlayer.position.x >= totalRoadLength - 30f)
        {
            if (spawnedMinions.Count == 0)
            {
                SpawnMiniBoss();
            }
        }

        // 5. KIỂM TRA ĐIỀU KIỆN XUẤT HIỆN BOSS CHÍNH:
        // Khi Mini Boss đã sinh ra và bị tiêu diệt
        if (miniBossSpawned && !miniBossDefeated)
        {
            if (activeMiniBoss == null)
            {
                miniBossDefeated = true;
                SpawnMainBoss();
            }
        }
    }

    void SpawnSelectedPlayer()
    {
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);

        if (characterPrefabs != null && characterPrefabs.Length > selectedIndex && characterPrefabs[selectedIndex] != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : new Vector3(0, 1.5f, 0);
            GameObject playerObj = Instantiate(characterPrefabs[selectedIndex], pos, Quaternion.identity);
            
            playerObj.tag = "Player";
            activePlayer = playerObj.transform;

            CharacterController2D controller = playerObj.GetComponent<CharacterController2D>();
            if (controller != null)
            {
                controller.isAI = false;
                controller.playerIndex = 1;
            }

            if (Camera.main != null)
            {
                CameraFollow2D camFollow = Camera.main.gameObject.GetComponent<CameraFollow2D>();
                if (camFollow == null) camFollow = Camera.main.gameObject.AddComponent<CameraFollow2D>();
                camFollow.target = activePlayer;
            }
        }
    }

    void SpawnGroundPiece()
    {
        Instantiate(groundPrefab, new Vector3(currentSpawnX, 0, 0), Quaternion.identity, transform);
        currentSpawnX += pieceWidth;
    }

    void SpawnMinion()
    {
        if (minionPrefabs != null && minionPrefabs.Length > 0)
        {
            int randIndex = Random.Range(0, minionPrefabs.Length);
            if (minionPrefabs[randIndex] != null)
            {
                GameObject minion = Instantiate(minionPrefabs[randIndex], new Vector3(nextMinionX, 1.2f, 0), Quaternion.identity);
                spawnedMinions.Add(minion);
            }
        }
        nextMinionX += minionSpawnInterval;
    }

    void SpawnMiniBoss()
    {
        miniBossSpawned = true;
        if (miniBossPrefab != null)
        {
            float spawnX = totalRoadLength - 8f;
            activeMiniBoss = Instantiate(miniBossPrefab, new Vector3(spawnX, 1.8f, 0), Quaternion.identity);
            activeMiniBoss.tag = "Enemy";
            Debug.Log("🐺 Mini Boss Dark Wolf xuất hiện!");
        }
        else
        {
            // Nếu không gán Mini Boss thì chuyển thẳng sang Boss chính
            miniBossDefeated = true;
            SpawnMainBoss();
        }
    }

    void SpawnMainBoss()
    {
        if (mainBossSpawned) return;
        mainBossSpawned = true;

        if (mainBossPrefab != null)
        {
            float bossX = totalRoadLength - 5f;
            GameObject bossObj = Instantiate(mainBossPrefab, new Vector3(bossX, 2.2f, 0), Quaternion.identity);
            bossObj.tag = "Enemy";

            BossStageTrigger trigger = bossObj.GetComponent<BossStageTrigger>();
            if (trigger == null) trigger = bossObj.AddComponent<BossStageTrigger>();
            trigger.fightStageSceneName = "Fight_Stage1";

            // Tạo tường chặn cuối map sau lưng Boss
            CreateInvisibleWall("Wall_End", totalRoadLength + 3f);
            Debug.Log("👑 Boss chính đã xuất hiện! Hãy bước tới để vào trận đấu.");
        }
    }

    void CreateInvisibleWall(string wallName, float posX)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.position = new Vector3(posX, 5f, 0f);
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 20f);
    }
}