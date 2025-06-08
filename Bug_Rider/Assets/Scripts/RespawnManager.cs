using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public Transform[] respawnPoints;
    private int currentRespawnIndex = 0;
    private bool reachedLastPoint = false;

    private GameObject player;

    private void Awake()
    {
        // �ڽ� ������Ʈ���� ������ ����Ʈ�� ���
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

    // Ʈ���� ������� ���� or �÷��̾� ����
    private void OnTriggerEnter(Collider other)
    {
        if (reachedLastPoint) return;

        // ���� �÷��̾� ��ü�� ���� �������ٸ� �״�� ������
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit the respawn trigger.");
            RespawnPlayer();
            return;
        }

        // �׷��� �ʴٸ�, ������ ���ɼ��� ����. ������ ž���� �÷��̾ ã��
        Transform playerTransform = other.transform.Find("Player");

        if (playerTransform == null)
        {
            // �ڽĵ� �߿��� Player �±׸� ���� ������Ʈ Ž��
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

            // �÷��̾ �и��ؼ� ������
            RespawnPlayerFromObject(playerTransform.gameObject);

            other.gameObject.SetActive(false);

            // ���� ������Ʈ �ı�
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

    // �Ϲ� ������ (ȥ�� �������� ��)
    public void RespawnPlayer()
    {
        if (player == null) return;

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        player.transform.position = respawnPoints[currentRespawnIndex].position + Vector3.up * 0.1f;

        if (controller != null) controller.enabled = true;

        Debug.Log($"Respawned to point {currentRespawnIndex}");
    }

    // �������� �������� �� �ܺο��� ���� �÷��̾� ������
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
        targetPlayer.transform.SetParent(null); // �����κ��� �и�
        targetPlayer.transform.position = respawnPoints[currentRespawnIndex].position + Vector3.up * 0.5f;

        if (controller != null) controller.enabled = true;

        Debug.Log($"[Mounted] Respawned player to point {currentRespawnIndex}");
    }
}
