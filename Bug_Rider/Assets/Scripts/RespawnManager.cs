using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public Transform[] respawnPoints;
    private int currentRespawnIndex = 0;
    private bool reachedLastPoint = false;

    private GameObject player;

    private void Awake()
    {
        // 자식 오브젝트들을 리스폰 포인트로 등록
        int count = transform.childCount;
        respawnPoints = new Transform[count];

        for (int i = 0; i < count; i++)
        {
            Transform point = transform.GetChild(i);
            respawnPoints[i] = point;

            var trigger = point.gameObject.AddComponent<RespawnTriggerPoint>();
            trigger.manager = this;
            trigger.index = i;
        }

        player = GameObject.FindGameObjectWithTag("Player");

    }

    // 트리거 방식으로 벌레 or 플레이어 감지
    private void OnTriggerEnter(Collider other)
    {
        if (reachedLastPoint) return;

        // 만약 플레이어 본체가 직접 떨어졌다면 그대로 리스폰
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit the respawn trigger.");
            RespawnPlayer();
            return;
        }

        // 그렇지 않다면, 벌레일 가능성이 있음. 벌레에 탑승한 플레이어를 찾음
        Transform playerTransform = other.transform.Find("Player");

        if (playerTransform == null)
        {
            // 자식들 중에서 Player 태그를 가진 오브젝트 탐색
            foreach (Transform child in other.transform)
            {
                if (child.CompareTag("Player"))
                {
                    playerTransform = child;
                    break;
                }
            }
        }

        if (playerTransform != null)
        {
            Debug.Log("Bug with player hit the respawn trigger.");

            // 플레이어만 분리해서 리스폰
            RespawnPlayerFromObject(playerTransform.gameObject);

            other.gameObject.SetActive(false);

            // 벌레 오브젝트 파괴
            Destroy(other.gameObject);
        }
    }

    public void SetRespawnIndex(int index)
    {
        if (index > currentRespawnIndex)
        {
            currentRespawnIndex = index;

            if (index >= respawnPoints.Length - 1)
            {
                reachedLastPoint = true;
                Debug.Log("Reached last respawn point.");
            }
        }
    }

    // 일반 리스폰 (혼자 떨어졌을 때)
    public void RespawnPlayer()
    {
        if (player == null) return;

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        player.transform.position = respawnPoints[currentRespawnIndex].position + Vector3.up * 0.1f;

        if (controller != null) controller.enabled = true;

        Debug.Log($"Respawned to point {currentRespawnIndex}");
    }

    // 벌레에서 떨어졌을 때 외부에서 받은 플레이어 리스폰
    public void RespawnPlayerFromObject(GameObject targetPlayer)
    {
        if (targetPlayer == null) return;

        CharacterController controller = targetPlayer.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        PlayerMovement movement = targetPlayer.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.ForceFallFromBug();
        }
        targetPlayer.transform.SetParent(null); // 벌레로부터 분리
        targetPlayer.transform.position = respawnPoints[currentRespawnIndex].position + Vector3.up * 0.5f;

        if (controller != null) controller.enabled = true;

        Debug.Log($"[Mounted] Respawned player to point {currentRespawnIndex}");
    }
}
