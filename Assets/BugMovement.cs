using UnityEngine;
using System.Collections;

// 기본 탑승 벌레의 이동 로직을 담당하는 스크립트
// Basic movement script for a rideable bug (WASD + 회전)
// Could serve as a base for other bug types
[RequireComponent(typeof(Rigidbody))]
public class BugMovement : MonoBehaviour
{
    // 이동 속도 / Movement speed when mounted
    public float moveSpeed = 3f;

    // 회전 부드러움 / Smooth time for rotation damping
    public float rotationSmoothTime = 0.1f;

    // 장애물 감지 거리 / Distance to detect obstacles ahead
    public float obstacleCheckDist = 0.8f;

    // 장애물 레이어 마스크 / Obstacle detection layer
    public LayerMask obstacleMask;

    // 탑승 여부 / Is the bug currently mounted by a player
    bool isMounted;

    // 플레이어에게 접근 중인지 여부 / Whether bug is auto-moving toward the player
    bool isApproaching;

    Rigidbody rb;
    float turnSmoothVelocity;
    Vector3 cachedInput = Vector3.zero;
    Coroutine approachRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (!isMounted) return;

        // 입력값 캐싱 (FixedUpdate에서 사용)
        // Cache input values to apply in FixedUpdate
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        cachedInput = new Vector3(h, 0, v).normalized;
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        // 입력이 거의 없으면 이동하지 않음
        // Don't move if there's no significant input
        if (cachedInput.sqrMagnitude < 0.01f) return;

        // 입력 방향으로 회전
        // Smoothly rotate toward input direction
        float targetAngle = Mathf.Atan2(cachedInput.x, cachedInput.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
        rb.MoveRotation(Quaternion.Euler(0, angle, 0));

        // 장애물 감지 (전방)
        // Check for obstacles using a sphere cast
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        if (Physics.SphereCast(rayOrigin, 0.4f, transform.forward, out _, obstacleCheckDist, obstacleMask))
        {
            return;
        }

        // 이동 처리
        // Move forward
        Vector3 next = rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }

    // 탑승 상태 설정
    // Set the mounted state and stop movement if unmounted
    public void SetMounted(bool mounted)
    {
        isMounted = mounted;

        if (!mounted)
        {
            // 속도를 0으로 초기화하여 바로 멈춤
            // Stop immediately when unmounted
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // 지정된 위치로 접근 시작
    // Starts automatic movement toward target position
    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }

    // 자동 이동 처리 코루틴
    // Coroutine to move toward a position (used when called by player)
    IEnumerator MoveToTarget(Vector3 target)
    {
        SetMounted(false);
        isApproaching = true;

        while (Vector3.Distance(transform.position, target) > 1.5f)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 next = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = rb.angularVelocity = Vector3.zero;
        isApproaching = false;
    }

    // 장애물 충돌 시 플레이어 낙하 처리
    // Drop player on collision with obstacles while mounted
    void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;

        var player = GetComponentInChildren<PlayerMovement>();
        player?.ForceFallFromBug();
        SetMounted(false);
    }
}
