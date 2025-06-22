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
    
    [Header("Distance Control")]
    public float activationDistance = 25f;   // 플레이어가 이 거리 안에 오면 활성화
    public float deactivationDistance = 30f; // 플레이어가 이 거리 밖으로 가면 비활성화
    public float checkInterval = 1f;         // 몇 초마다 플레이어 거리 체크할지
    
    public GameObject antDashUI;
    public GameObject laybugDashUI;
    public GameObject katydidDashUI;
    public TMP_Text antCountdownText;
    public TMP_Text ladybugCountdownText;
    public TMP_Text katydidCountdownText;
    
    private Transform player;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private Coroutine distanceCheckCoroutine;
    
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
        
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Spawner: Player not found!");
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
        // 거리 체크 시작
        if (player != null)
        {
            distanceCheckCoroutine = StartCoroutine(DistanceCheckLoop());
        }
    }
    
    void OnDestroy()
    {
        // 코루틴 정리
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        if (distanceCheckCoroutine != null)
            StopCoroutine(distanceCheckCoroutine);
    }
    
    IEnumerator DistanceCheckLoop()
    {
        while (true)
        {
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                
                // 거리에 따른 스포너 상태 제어
                if (!isSpawning && distance <= activationDistance)
                {
                    StartSpawning();
                }
                else if (isSpawning && distance > deactivationDistance)
                {
                    StopSpawning();
                }
                
                // 디버그 정보 (필요하면 주석 해제)
                // Debug.Log($"[Spawner {name}] Distance to player: {distance:F1}m, Spawning: {isSpawning}");
            }
            
            yield return new WaitForSeconds(checkInterval);
        }
    }
    
    void StartSpawning()
    {
        if (isSpawning) return;
        
        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnLoop());
        Debug.Log($"[Spawner {name}] Started spawning - Player in range");
    }
    
    void StopSpawning()
    {
        if (!isSpawning) return;
        
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        Debug.Log($"[Spawner {name}] Stopped spawning - Player out of range");
    }
    
    IEnumerator SpawnLoop()
    {
        while (isSpawning)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    void SpawnOne()
    {
        if (!isSpawning) return; // 추가 안전 체크
        
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
        
        // UI 설정 (필요하면 주석 해제)
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
    
    // 디버그용 기즈모 (Scene 뷰에서 거리 확인 가능)
    void OnDrawGizmosSelected()
    {
        // 활성화 거리 (초록색)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
        
        // 비활성화 거리 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, deactivationDistance);
        
        // 스포너 상태 표시
        Gizmos.color = isSpawning ? Color.green : Color.gray;
        Gizmos.DrawCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
    }
}