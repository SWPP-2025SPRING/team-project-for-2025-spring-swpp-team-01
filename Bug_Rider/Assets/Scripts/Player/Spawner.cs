using UnityEngine;
using System.Collections;
using TMPro;
public class Spawner : MonoBehaviour
{
    public GameObject[] objectsToSpawn;  // 프리팹 배열
    public Transform spawnPoint;
    public Transform targetPoint;
    public float speed = 14f;
    public float spawnInterval = 3f;     // 몇 초마다 생성할지
    public float delay = 10f;            // 몇 초 후에 벌레 사라지게 할 지
    public GameObject antDashUI;
    public GameObject laybugDashUI;
    public GameObject katydidDashUI;
    public TMP_Text antCountdownText;
    public TMP_Text ladybugCountdownText;
    public TMP_Text katydidCountdownText;
    void Awake()
    {
        if (targetPoint == null)
        {
            targetPoint = FindChildByName(transform, "TargetPoint");
            if (targetPoint == null)
                Debug.LogWarning("Spawner: TargetPoint not found under " + name);
        }
        if (spawnPoint == null)
        {
            spawnPoint = FindChildByName(transform, "SpawnPoint");
            if (spawnPoint == null)
                Debug.LogWarning("Spawner: SpawnPoint not found under " + name);
        }
    }
    Transform FindChildByName(Transform parent, string targetName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
                return child;
        }
        return null;
    }
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
        Vector3 direction = (targetPoint.position - spawnPoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        GameObject obj = Instantiate(selectedPrefab, spawnPoint.position, rotation);
        MoveToTarget mover = obj.AddComponent<MoveToTarget>();
        mover.Initialize(targetPoint.position, speed, delay);
        AntMovement ant = obj.GetComponent<AntMovement>();
        LadybugMovement ladybug = obj.GetComponent<LadybugMovement>();
        KatydidMovement katydid = obj.GetComponent<KatydidMovement>();
        CaterpillarMovement caterpillar = obj.GetComponent<CaterpillarMovement>();
        // if (ant != null)
        // {
        //     ant.SetUI(antDashUI, antCountdownText);
        // }
        // if (ladybug != null)
        // {
        //     ladybug.SetUI(laybugDashUI, ladybugCountdownText);
        // }
        // if (katydid != null)
        // {
        //     katydid.SetUI(katydidDashUI, katydidCountdownText);
        // }
    }
}