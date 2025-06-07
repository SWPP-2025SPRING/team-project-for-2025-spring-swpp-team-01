using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public GameObject[] objectsToSpawn;  // ������ �迭
    public Transform spawnPoint;
    public Transform targetPoint;
    public float speed = 2f;
    public float spawnInterval = 3f;     // �� �ʸ��� ��������

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOne()
    {
        int index = Random.Range(0, objectsToSpawn.Length);
        GameObject selectedPrefab = objectsToSpawn[index];

        // ���� ���
        Vector3 direction = (targetPoint.position - spawnPoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject obj = Instantiate(selectedPrefab, spawnPoint.position, rotation);

        // �̵� ����
        MoveToTarget mover = obj.AddComponent<MoveToTarget>();
        mover.Initialize(targetPoint.position, speed);
    }
}


