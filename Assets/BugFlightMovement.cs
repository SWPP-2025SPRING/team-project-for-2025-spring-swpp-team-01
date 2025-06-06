using UnityEngine;
using System.Collections;

// 비행 가능한 벌레의 이동 처리 스크립트
// Movement logic for a flying rideable bug with fixed flight height
public class BugFlightMovement : MonoBehaviour
{
    [Header("Speed / Turn")]
    // 이동 속도 / Forward movement speed
    public float moveSpeed = 4f;

    // 회전 부드러움 / Smoothing time for rotation interpolation
    public float rotationSmoothTime = 0.1f;

    // 비행 고도 / Fixed Y-axis height during flight
    public float flightHeight = 2f;

    [Header("Collision Check")]
    // 장애물 감지 거리 / Distance to check for obstacles ahead
    public float obstacleCheckDist = 0.8f;

    // 감지할 레이어 / Layer mask for obstacle detection
    public LayerMask obstacleMask;

    // 탑승 상태 여부 / Whether player is mounted
    bool isMounted;

    // 접근 중인지 여부 / Whether the bug is auto-approaching the player
    bool isApproaching;

    Rigidbody rb;
    float turnSmoothVelocity;
    Vector3 cachedInput = Vector3.zero;
    Coroutine approachRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // 비행은 중력이 필요 없기 때문에 off
        // Disable gravity for flying
        rb.useGravity = false;

        // 비행 중 Y축 위치 고정, 회전 고정
        // Freeze rotation and Y-position to maintain stable flight
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        if (!isMounted) return;

        // 입력값 저장 → FixedUpdate에서 사용
        // Cache input for use in physics update
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        cachedInput = new Vector3(h, 0, v).normalized;
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        // 입력 없으면 이동 X
        if (cachedInput.sqrMagnitude < 0.01f) return;

        // 입력 방향으로 회전
        float targetAngle = Mathf.Atan2(cachedInput.x, cachedInput.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
        rb.MoveRotation(Quaternion.Euler(0, angle, 0));

        // 장애물 감지 (앞 방향)
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        if (Physics.SphereCast(rayOrigin, 0.4f, transform.forward, out _, obstacleCheckDist, obstacleMask))
        {
            Debug.DrawRay(rayOrigin, transform.forward * obstacleCheckDist, Color.red, 0.1f);
            return;
        }

        // 앞으로 이동, y는 고정
        Vector3 next = rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime;
        next.y = flightHeight;  // Y 고도 고정
        rb.MovePosition(next);
    }

    // 탑승 상태 갱신
    // Set mounted state and stop motion if unmounted
    public void SetMounted(bool mounted)
    {
        isMounted = mounted;

        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // 플레이어 위치로 접근 시작
    // Start auto-approach toward player
    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }

    // 지정된 위치까지 접근하는 코루틴
    // Coroutine that moves the bug toward the target position at flight height
    IEnumerator MoveToTarget(Vector3 target)
    {
        isMounted = false;
        isApproaching = true;

        while (Vector3.Distance(transform.position, target) > 1.5f)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 next = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
            next.y = flightHeight;
            rb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = rb.angularVelocity = Vector3.zero;
        isApproaching = false;
    }

    // 장애물 충돌 시 플레이어 낙하 처리
    // Drop player if bug collides with an obstacle
    void OnCollisionEnter(Collision col)
    {
        Debug.Log($"[BugFlight] hit {col.gameObject.name}");

        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;

        var player = GetComponentInChildren<PlayerMovement>();
        player?.ForceFallFromBug();
        SetMounted(false);
    }
}
