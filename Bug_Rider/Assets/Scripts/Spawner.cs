using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public GameObject[] objectsToSpawn;  // 프리팹 배열
    public Transform spawnPoint;
    public Transform targetPoint;
    public float speed = 2f;
    public float spawnInterval = 3f;     // 몇 초마다 생성할지

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

        // 방향 계산
        Vector3 direction = (targetPoint.position - spawnPoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject obj = Instantiate(selectedPrefab, spawnPoint.position, rotation);

        // 이동 로직
        MoveToTarget mover = obj.AddComponent<MoveToTarget>();
        mover.Initialize(targetPoint.position, speed);
    }
}


