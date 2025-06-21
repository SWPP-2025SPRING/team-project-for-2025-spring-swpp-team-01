using UnityEngine;
using System.Collections;
public class RespawnManager : MonoBehaviour
{
    public Transform[] respawnPoints;
    public Vector3[] respawnLocations;
    public Vector3[] respawnRotations;
    private int currentRespawnIndex = 0;
    private bool reachedLastPoint = false;
    private GameObject player;
    private Animator animator;
    public GameObject smoke;
    /* ---------- 이동 잠금용 변수 ---------- */
    private float movementLockEndTime = 0f;    // 잠금이 풀릴 절대 시각
    private bool isMovementLocked = false; // 현재 잠금 상태
    private PlayerMovement playerMovement;
    private void Awake()
    {
        // 자식 오브젝트들을 리스폰 포인트로 등록 (트리거/콜라이더 자동 세팅)
        int count = transform.childCount;
        respawnPoints = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            Transform point = transform.GetChild(i);
            // 이미 붙어 있으면 재사용, 없으면 새로 붙임
            var trigger = point.GetComponent<RespawnTriggerPoint>();
            if (trigger == null)
                trigger = point.gameObject.AddComponent<RespawnTriggerPoint>();
            trigger.manager = this;
            trigger.index = i;
            var col = point.GetComponent<BoxCollider>();
            if (col == null) col = point.gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
        }
        // 플레이어와 이동 스크립트 캐싱
        player = GameObject.FindGameObjectWithTag("Player");
        animator = player.GetComponent<Animator>();
        if (player != null)
            playerMovement = player.GetComponent<PlayerMovement>();
    }
    private void Update()
    {
        // 이동 잠금 해제 체크
        if (isMovementLocked && Time.time >= movementLockEndTime)
        {
            if (playerMovement != null)
                playerMovement.enabled = true;
            isMovementLocked = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (reachedLastPoint) return;
        if (other.CompareTag("Player"))
        {
            Debug.Log("바닥");
            SetRespawnIndex(currentRespawnIndex);
            RespawnPlayer();
            return;
        }
        // 벌레 탑승 플레이어 감지
        Transform playerTf = other.transform.Find("Player");
        if (playerTf == null)
        {
            foreach (Transform child in other.transform)
                if (child.CompareTag("Player"))
                {
                    playerTf = child;
                    break;
                }
        }
        if (playerTf != null)
        {
            SetRespawnIndex(currentRespawnIndex);
            RespawnPlayerFromObject(playerTf.gameObject);
            other.gameObject.SetActive(false);
            Destroy(other.gameObject);
        }
    }

    public void RespawnByContext() // Same as OnTriggerEnter(), written again for Restart
    {
        bool isRiding = animator != null && animator.GetBool("is_riding_on_bug");

        if (isRiding)
        {
            RespawnPlayerFromObject(player);
        }
        else
        {
            RespawnPlayer();
        }
    }


    public void SetRespawnIndex(int index)
    {
        if (true)
        {
            currentRespawnIndex = index;
            if (index == respawnPoints.Length - 1)
            {
                reachedLastPoint = true;
                Debug.Log("Reached last respawn point.");
            }
        }
    }
    // 일반 리스폰 (월드 좌표 사용)
    public void RespawnPlayer()
    {
        if (player == null) return;
        // Rigidbody 초기화
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        // 월드 위치·회전 결정
        GetWorldTransform(currentRespawnIndex, out Vector3 worldPos, out Quaternion worldRot);
        // 월드 좌표 텔레포트
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.position = worldPos;
            rb.rotation = worldRot;
            rb.isKinematic = false;
        }
        else
        {
            player.transform.SetPositionAndRotation(worldPos, worldRot);
        }
        // 월드 좌표 로그 찍기
        Debug.Log($"[RespawnPlayer] WorldPos={worldPos:F3}, WorldRot={worldRot.eulerAngles}");
        // 연기 재배치
        UpdateSmoke(player.transform);
        // 2초간 이동 잠금
        LockMovement();
    }
    // 벌레에서 떨어졌을 때 리스폰 (월드 좌표 사용)
    public void RespawnPlayerFromObject(GameObject targetPlayer)
    {
        if (targetPlayer == null) return;
        var mov = targetPlayer.GetComponent<PlayerMovement>();
        animator.SetBool("is_riding_on_bug", false);
        if (mov != null)
            mov.ForceFallFromBug();
        // Rigidbody 초기화
        var rb = targetPlayer.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        // 월드 위치·회전 결정
        GetWorldTransform(currentRespawnIndex, out Vector3 worldPos, out Quaternion worldRot);
        // 월드 좌표 텔레포트
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.position = worldPos;
            rb.rotation = worldRot;
            rb.isKinematic = false;
        }
        else
        {
            targetPlayer.transform.SetPositionAndRotation(worldPos, worldRot);
        }
        // 월드 좌표 로그
        Debug.Log($"[RespawnFromObject] WorldPos={worldPos:F3}, WorldRot={worldRot.eulerAngles}");
        // 연기 재배치
        UpdateSmoke(targetPlayer.transform);
        // 2초간 이동 잠금
        LockMovement();
    }
    // 인덱스별 하드코딩 월드 위치·회전
    private void GetWorldTransform(int idx, out Vector3 pos, out Quaternion rot)
    {
        pos = respawnLocations[idx];
        rot = Quaternion.Euler(respawnRotations[idx]);
    }
    // 연기 위치 갱신 (월드 기준)
    private void UpdateSmoke(Transform playerTf)
    {
        StartCoroutine(SnapSmokeNextFrame(playerTf));
    }
    private IEnumerator SnapSmokeNextFrame(Transform playerTf)
    {
        var smokeCtrl = smoke.GetComponent<SmokeMovement>();
        // SmokeMovement 비활성화 → 강제 위치 이동 중 이동 방지
        if (smokeCtrl != null)
        {
            smokeCtrl.enabled = false;
            smokeCtrl.target = playerTf; // 대상 설정은 미리 해놓음
        }
        yield return null; // :흰색_확인_표시: 1 프레임 대기 (리스폰 반영 후)
        // 최신 forward를 반영한 위치로 이동
        Vector3 newPos = playerTf.position - 5f * playerTf.forward;
        smoke.transform.position = newPos;
        Debug.Log($"[SnapSmoke] Moved smoke to {newPos}");
        if (smokeCtrl != null)
            smokeCtrl.enabled = true;
    }
    // 이동 잠금
    private void LockMovement()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;
        isMovementLocked = true;
        movementLockEndTime = Time.time + 2f;
    }
}