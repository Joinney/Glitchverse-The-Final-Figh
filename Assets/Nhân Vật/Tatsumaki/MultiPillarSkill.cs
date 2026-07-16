using UnityEngine;
using System.Collections;

public class MultiPillarSkill : MonoBehaviour
{
    [Header("Cài đặt Làn Sóng")]
    public GameObject singlePillarPrefab; // Ném Prefab 1 Cột của bạn vào đây
    public int pillarCount = 5;           // Số lượng cột muốn mọc ra
    public float spacing = 1.5f;          // Khoảng cách giữa các cột (giãn cách x)
    public float delayTime = 0.15f;       // Độ trễ (Giây) để tạo hiệu ứng làn sóng lan dần

    void Start()
    {
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        // 1. Tìm xem chủ nhân đang quay mặt về bên nào để mọc cột cho đúng hướng
        GameObject[] friends = GameObject.FindGameObjectsWithTag(gameObject.tag);
        Transform master = null;
        float minDist = Mathf.Infinity;
        
        foreach (GameObject obj in friends)
        {
            if (obj == gameObject) continue; // Bỏ qua bản thân cái trạm
            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                master = obj.transform;
            }
        }

        // Nếu chủ nhân quay trái, hướng sẽ là -1
        float direction = 1f;
        if (master != null && master.localScale.x < 0) direction = -1f;

        // 2. Vòng lặp đẻ từng cột một
        for (int i = 0; i < pillarCount; i++)
        {
            // Tính tọa độ cột tiếp theo dịch chuyển dần về phía trước
            Vector3 spawnPos = transform.position + new Vector3(direction * i * spacing, 0, 0);
            
            // Sinh cột ra
            GameObject newPillar = Instantiate(singlePillarPrefab, spawnPos, Quaternion.identity);
            newPillar.tag = gameObject.tag; // Truyền phe cho cột để không đâm trúng quân mình

            // Đảo chiều hình ảnh cột (nếu cần)
            if (direction < 0)
            {
                Vector3 scale = newPillar.transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                newPillar.transform.localScale = scale;
            }

            // Tạm dừng 1 khoảng thời gian nhỏ trước khi đẻ cột tiếp theo
            yield return new WaitForSeconds(delayTime);
        }

        // Sinh đủ số cột rồi thì tự hủy cái trạm này đi cho nhẹ máy
        Destroy(gameObject);
    }
}