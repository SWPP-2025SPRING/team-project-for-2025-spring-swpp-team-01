using UnityEngine;
using System.Collections;
using TMPro;

// 개미 전용 이동 스크립트
// Handles movement logic specific to the ant rideable bug
[RequireComponent(typeof(Rigidbody))]
public class AntMovement : MonoBehaviour, IRideableBug
{
    // 이동 속도 / Walking speed
    public float moveSpeed = 3f;

    // 회전 속도 / Turning speed
    public float rotationSpeed = 180f;

    // 대시 속도 / Dash movement speed
    public float dashSpeed = 15f;

    // 대시 지속 시간 / Duration of the dash
    public float dashDuration = 0.4f;

    // 대시 쿨타임 / Cooldown before dash is available again
    public float dashCooldown = 1f;

    // 장애물 감지 거리 / Distance to check for obstacles in front
    public float obstacleCheckDist = 0.8f;

    // 충돌 감지용 레이어 마스크 / Layer mask to detect obstacles
    public LayerMask obstacleMask;

    // 대시 UI (활성화/비활성화) / UI element to show dash availability
    public GameObject DashUI;

    // 남은 대시 쿨타임 텍스트 / Text to show remaining cooldown for dash
    public TMP_Text countdownText;

    // 현재 탑승 상태 / Whether the player is mounted on the ant
    private bool isMounted = false;

    // 플레이어에게 다가가는 중인지 여부 / Whether the ant is currently approaching the player
    private bool isApproaching = false;

    // 현재 대시 중인지 여부 / Whether the ant is currently dashing
    private bool isDashing = false;

    // 대시가 가능한지 여부 / Whether the dash can be triggered
    private bool canDash = true;

    private Rigidbody rb;
    private Animator antAnimator;
    private Coroutine approachRoutine;

    // 이동 전략 (걷기) / Movement strategy for walking (Strategy Pattern)
    private IBugMovementStrategy walkStrategy;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        antAnimator = GetComponent<Animator>();
        walkStrategy = new WalkMovementStrategy();

        // 시작 시 대시 UI는 비활성화
        // Deactivate dash UI at start
        DashUI?.SetActive(false);
    }

    void Update()
    {
        // 탑승 중이면서 대시 가능할 때만 대시 입력 받음
        // Only allow dash input when mounted and dash is ready
        if (!isMounted || isDashing || !canDash) return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        // 대시 중이거나 접근 중일 때는 조작 불가
        // Movement is disabled during dash or auto-approach
        if (!isMounted || isApproaching || isDashing) return;

        walkStrategy.HandleMovement(rb, antAnimator, obstacleMask, moveSpeed, rotationSpeed, obstacleCheckDist);
    }

    // 대시 동작 처리 코루틴
    // Coroutine to handle forward dash movement
    public IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        DashUI?.SetActive(false);
        antAnimator?.SetTrigger("is_dashing");

        Vector3 dashDir = rb.rotation * Vector3.forward;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            Vector3 next = rb.position + dashDir * dashSpeed * Time.fixedDeltaTime;
            rb.MovePosition(next);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;

        // 쿨타임 UI 처리
        // Show dash cooldown UI while waiting
        float countdown = dashCooldown;
        while (countdown > 0 && isMounted)
        {
            countdownText.text = $"You can dash after {Mathf.Ceil(countdown)}s";
            countdown -= Time.deltaTime;
            yield return null;
        }

        countdownText.text = "";
        DashUI?.SetActive(isMounted);
        canDash = true;
    }

    // 탑승 상태를 갱신한다 (UI 처리 포함)
    // Sets mounted state and updates UI accordingly
    public void SetMounted(bool mounted)
    {
        isMounted = mounted;

        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            DashUI?.SetActive(false);
            countdownText.text = "";
        }
        else
        {
            DashUI?.SetActive(true);
        }
    }

    // 플레이어 위치로 자동 이동 시작
    // Starts a coroutine to approach the player
    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }

    // 지정 위치로 접근하는 코루틴
    // Moves toward the target location until close enough
    public IEnumerator MoveToTarget(Vector3 target)
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

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isApproaching = false;
    }

    // 장애물 충돌 시 플레이어 낙하 처리
    // Forces the player to fall when ant hits an obstacle while mounted
    public void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;

        var player = GetComponentInChildren<PlayerMovement>();
        antAnimator?.SetTrigger("is_dropping");
        player?.ForceFallFromBug();
        SetMounted(false);
    }
}
