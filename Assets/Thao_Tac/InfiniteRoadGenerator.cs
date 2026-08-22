using System.Collections.Generic;
using UnityEngine;

public class InfiniteRoadGenerator : MonoBehaviour
{
    [Header("1. Danh Sách Nhân Vật Chọn Từ Menu")]
    public GameObject[] characterPrefabs;
    public Transform spawnPoint;

    [Header("2. Cài Đặt Sinh Đường")]
    public GameObject groundPrefab;
    public float pieceWidth = 0f; 
    public float spawnDistance = 35f;
    public float totalRoadLength = 100f;

    [Header("3. Cài Đặt Quái Nhỏ")]
    public GameObject[] minionPrefabs;
    public float minionSpawnInterval = 14f;
    private float nextMinionX = 15f;
    private List<GameObject> spawnedMinions = new List<GameObject>();

    [Header("4. Cài Đặt Mini Boss (Dark Wolf)")]
    public GameObject miniBossPrefab;
    private GameObject activeMiniBoss;
    private bool miniBossSpawned = false;
    private bool miniBossDefeated = false;

    [Header("5. Cài Đặt Boss Cuối Đường")]
    public GameObject mainBossPrefab;
    public float mainBossSpawnDistanceTrigger = 12f; // Khi người chơi còn cách Boss 12m thì Boss mới xuất hiện
    private bool mainBossSpawned = false;

    private Transform activePlayer;
    private float currentSpawnX = 0f;
    private CameraFollow2D camFollow;

    void Awake()
    {
        CalculatePieceWidth();
        SpawnSelectedPlayer();
        CreateInvisibleWall("Wall_Start", -3f);
    }

    void Start()
    {
        while (currentSpawnX <= totalRoadLength + pieceWidth)
        {
            SpawnGroundPiece();
        }
    }

    void Update()
    {
        if (activePlayer == null) return;

        // 1. Sinh quái nhỏ dọc đường
        if (activePlayer.position.x > nextMinionX - 20f && nextMinionX < totalRoadLength - 30f)
        {
            SpawnMinion();
        }

        spawnedMinions.RemoveAll(m => m == null);

        // 2. ĐIỀU KIỆN SINH MINI BOSS: Đi gần tới mốc 75m VÀ dọn sạch quái thường
        if (!miniBossSpawned && activePlayer.position.x >= totalRoadLength - 28f)
        {
            if (spawnedMinions.Count == 0)
            {
                SpawnMiniBoss();
            }
        }

        // 3. KIỂM TRA MINI BOSS ĐÃ CHẾT
        if (miniBossSpawned && !miniBossDefeated)
        {
            if (activeMiniBoss == null)
            {
                miniBossDefeated = true;
                UnlockArena();
                Debug.Log("🎯 Mini Boss đã bị tiêu diệt! Hãy tiến sâu vào cuối bản đồ để tìm Boss chính.");
            }
        }

        // 4. ĐIỀU KIỆN SINH BOSS CHÍNH:
        // Phải thỏa mãn CẢ 2: Đã diệt xong Mini Boss VÀ Player/Camera đã đi vào vùng cuối map
        if (miniBossDefeated && !mainBossSpawned)
        {
            float bossX = totalRoadLength - 4f;
            if (activePlayer.position.x >= bossX - mainBossSpawnDistanceTrigger)
            {
                SpawnMainBoss();
            }
        }
    }

    void CalculatePieceWidth()
    {
        if (pieceWidth <= 0f && groundPrefab != null)
        {
            SpriteRenderer sr = groundPrefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null) pieceWidth = sr.bounds.size.x;
            else pieceWidth = 20f;
        }
    }

    void SpawnGroundPiece()
    {
        GameObject newGround = Instantiate(groundPrefab, new Vector3(currentSpawnX, 0, 0), Quaternion.identity, transform);
        SpriteRenderer sr = newGround.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null) pieceWidth = sr.bounds.size.x;
        currentSpawnX += pieceWidth;
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
                camFollow = Camera.main.gameObject.GetComponent<CameraFollow2D>();
                if (camFollow == null) camFollow = Camera.main.gameObject.AddComponent<CameraFollow2D>();
                camFollow.target = activePlayer;
            }
        }
    }

    void SpawnMinion()
    {
        if (minionPrefabs != null && minionPrefabs.Length > 0)
        {
            int randIndex = Random.Range(0, minionPrefabs.Length);
            if (minionPrefabs[randIndex] != null)
            {
                GameObject minion = Instantiate(minionPrefabs[randIndex], new Vector3(nextMinionX, 1.2f, 0), Quaternion.identity);
                Vector3 s = minion.transform.localScale;
                minion.transform.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);
                spawnedMinions.Add(minion);
            }
        }
        nextMinionX += minionSpawnInterval;
    }

    void SpawnMiniBoss()
    {
        miniBossSpawned = true;

        if (camFollow != null)
        {
            camFollow.LockCameraAtCurrentPosition();
        }

        if (miniBossPrefab != null)
        {
            float spawnX = activePlayer.position.x + 6.5f;
            activeMiniBoss = Instantiate(miniBossPrefab, new Vector3(spawnX, 1.8f, 0), Quaternion.identity);
            activeMiniBoss.tag = "Enemy";

            // Ép xoay mặt sang trái nhìn về phía Player
            Vector3 s = activeMiniBoss.transform.localScale;
            activeMiniBoss.transform.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);

            SpriteRenderer sr = activeMiniBoss.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.flipX = true;

            BossAnnouncementUI.ShowAnnouncement("⚠️ CẢNH BÁO: MINI BOSS DARK WOLF XUẤT HIỆN! ⚠️", new Color(1f, 0.45f, 0f));
            Debug.Log("🐺 Mini Boss Dark Wolf xuất hiện!");
        }
        else
        {
            miniBossDefeated = true;
            UnlockArena();
        }
    }

    void UnlockArena()
    {
        if (camFollow != null)
        {
            camFollow.UnlockCamera();
        }
    }

    void SpawnMainBoss()
    {
        mainBossSpawned = true;

        if (mainBossPrefab != null)
        {
            float bossX = totalRoadLength - 4f;
            GameObject bossObj = Instantiate(mainBossPrefab, new Vector3(bossX, 2.2f, 0), Quaternion.identity);
            bossObj.tag = "Enemy";

            // 🔄 Tắt FlipX và ép Scale X âm (-5) để quay mặt sang trái
            SpriteRenderer[] allRenderers = bossObj.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer r in allRenderers)
            {
                r.flipX = false;
            }

            Vector3 s = bossObj.transform.localScale;
            bossObj.transform.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);

            BossStageTrigger trigger = bossObj.GetComponent<BossStageTrigger>();
            if (trigger == null) trigger = bossObj.AddComponent<BossStageTrigger>();
            trigger.fightStageSceneName = "Fight_Stage1";

            CreateInvisibleWall("Wall_End", totalRoadLength + 3f);

            BossAnnouncementUI.ShowAnnouncement("THỦ LĨNH CUỐI CÙNG ĐÃ XUẤT HIỆN!", Color.red, 3.0f);
            Debug.Log("👑 Boss chính đã xuất hiện!");
        }
    }

    GameObject CreateInvisibleWall(string wallName, float posX)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.position = new Vector3(posX, 5f, 0f);
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 20f);
        return wall;
    }
}