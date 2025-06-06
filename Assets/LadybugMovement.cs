using UnityEngine;
using TMPro;

// 무당벌레 이동 및 비행을 제어하는 스크립트
// Handles movement and flight logic for the ladybug rideable bug
[RequireComponent(typeof(Rigidbody))]
public class LadybugMovement : MonoBehaviour, IRideableBug
{
    // 걷기 속도 / Walking speed
    public float moveSpeed = 4f;

    // 회전 속도 / Rotation speed
    public float rotationSpeed = 180f;

    // 장애물 감지 거리 / Distance to detect obstacles in front
    public float obstacleCheckDist = 0.8f;

    // 비행 고도 / Fixed Y-position during flight (for hovering effect)
    public float flightHeight = 0.01f;

    // 장애물 감지용 레이어 / Obstacle detection layer mask
    public LayerMask obstacleMask;

    // 비행 UI (버튼 등) / UI element related to flight (e.g., indicator or button)
    public GameObject FlyUI;

    // 쿨타임 카운트다운 텍스트 / Text to show cooldown time for flight
    public TMP_Text countdownText;

    // 현재 탑승 상태 / Whether player is currently mounted
    private bool isMounted = false;

    // 접근 중인지 여부 / Whether currently approaching player
    private bool isApproaching = false;

    private Rigidbody rb;
    private Animator animator;

    private Coroutine approachRoutine;

    // 걷기 전략 / Strategy object for ground movement
    private IBugMovementStrategy walkStrategy;

    // 비행 전략 / Strategy object for handling flight behavior
    private FlyMovementStrategy flyStrategy;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponent<Animator>();

        // 걷기/비행 전략 초기화
        // Initialize movement and flight strategies
        walkStrategy = new WalkMovementStrategy();
        flyStrategy = new FlyMovementStrategy(this, countdownText, FlyUI, rb, animator);

        // 시작 시 UI는 꺼둔다
        // Disable fly UI at start
        FlyUI?.SetActive(false);
    }

    void Update()
    {
        // 탑승 중일 때만 입력 받음
        // Only respond to input when mounted
        if (!isMounted) return;

        // 스페이스 키로 비행 시작
        // Start flying when pressing space, if flight is available
        if (Input.GetKeyDown(KeyCode.Space) && flyStrategy.CanFly)
        {
            flyStrategy.StartFlight();
        }
    }

    void FixedUpdate()
    {
        if (!isMounted || isApproaching) return;

        // 걷기 처리 / Handle ground movement
        walkStrategy.HandleMovement(rb, animator, obstacleMask, moveSpeed, rotationSpeed, obstacleCheckDist);

        // 비행 처리 / Handle flight movement
        flyStrategy.HandleMovement(rb, animator, obstacleMask, moveSpeed, rotationSpeed, obstacleCheckDist);
    }

    // 탑승 상태 설정
    // Sets the mounted state and updates visual/UI accordingly
    public void SetMounted(bool mounted)
    {
        isMounted = mounted;

        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            animator?.SetBool("is_walking", false);
            FlyUI?.SetActive(false);
        }
        else
        {
            FlyUI?.SetActive(true);
        }
    }

    // 플레이어 위치로 접근 시작
    // Starts approach movement towards the player
    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }

    // 플레이어 위치로 이동하는 코루틴
    // Coroutine for approaching the player until close enough
    private System.Collections.IEnumerator MoveToTarget(Vector3 target)
    {
        SetMounted(false);
        isApproaching = true;

        while (Vector3.Distance(transform.position, target) > 1.5f)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 next = rb.position + dir * moveSpeed * Time.fixedDeltaTime;

            // 비행 중일 경우 고도 유지
            // Maintain flight height while moving
            next.y = flightHeight;

            rb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isApproaching = false;
    }

    // 장애물과 충돌 시 낙하 처리
    // Drop player when colliding with obstacles during mount
    void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;

        if (col.gameObject.CompareTag("Obstacle"))
        {
            animator?.SetTrigger("is_drop");

            flyStrategy.StopFlight();
            FlyUI?.SetActive(false);

            var player = GetComponentInChildren<PlayerMovement>();
            player?.ForceFallFromBug();
            SetMounted(false);
        }
    }
}
