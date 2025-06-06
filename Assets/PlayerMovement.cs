using UnityEngine;
using System.Collections;

// 플레이어의 이동, 탑승, 추락 등을 모두 제어하는 핵심 스크립트
// Main script for handling player movement, bug mounting, and falling
public class PlayerMovement : MonoBehaviour
{
    // 걷기 속도 / Walking speed
    public float walkSpeed = 3f;

    // 달리기 속도 / Running speed
    public float runSpeed = 6f;

    // 중력 가속도 / Gravity force
    public float gravity = -9.81f;

    // 캐릭터 컨트롤러 컴포넌트 / Unity's CharacterController component
    private CharacterController controller;

    // 현재 속도 벡터 / Current velocity vector
    private Vector3 velocity;

    // 탑승 상태 여부 / Whether the player is currently mounted on a bug
    public bool isMounted = false;

    // 탑승 중인 벌레 오브젝트 / The bug the player is currently mounted on
    public Transform mountedBug;

    // 낙하 중인지 여부 / Whether the player is currently falling
    private bool isFalling = false;

    // 애니메이터 컴포넌트 / Animator component for playing animations
    private Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 벌레를 타고 있을 경우, 플레이어 조작을 중단한다
        // If mounted, ignore normal movement and only handle unmounting
        if (isMounted)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Unmount(); // E 키로 하차
            }
            return;
        }

        // 입력값을 받아 이동 방향 벡터 계산
        // Calculate movement direction based on WASD input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        move.y = 0f;

        // 쉬프트 키로 달리기 전환
        // Use Left Shift to run
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // 입력값이 있는 경우에만 회전 및 이동 처리
        // If there's movement input, rotate and move the character
        if (move.sqrMagnitude > 0.01f)
        {
            animator.SetBool("is_running", true);
            Quaternion targetRotation = Quaternion.LookRotation(move.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);

            controller.Move(move.normalized * speed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("is_running", false); // 멈춤 애니메이션 / Stop animation
        }

        // 중력 적용
        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -0.2f; // 땅에 붙이기 위한 작은 음수값
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 벌레 호출 (F 키)
        // F key to call nearby bugs
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryCallBug();
        }

        // 탑승 시도 (E 키)
        // E key to attempt mounting a bug
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryMount();
        }
    }

    // 주변 벌레 중 가까운 대상에게 다가오라고 지시
    // Call the nearest bug to approach the player
    void TryCallBug()
    {
        float searchRadius = 2f;
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);
        Transform closestBug = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Bug"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestBug = hit.transform;
                }
            }
        }

        // 타입별로 캐스팅하여 해당 벌레에 ApproachTo 명령 전달
        // Try type casting to specific bug types and call ApproachTo()
        if (closestBug != null)
        {
            if (closestBug.TryGetComponent<BugMovement>(out var bugMover))
            {
                bugMover.ApproachTo(transform.position);
            }
            else if (closestBug.TryGetComponent<BugFlightMovement>(out var bugFlier))
            {
                bugFlier.ApproachTo(transform.position);
            }
            else if (closestBug.TryGetComponent<AntMovement>(out var ant))
            {
                ant.ApproachTo(transform.position);
            }
            else if (closestBug.TryGetComponent<LadybugMovement>(out var ladybug))
            {
                ladybug.ApproachTo(transform.position);
            }
        }
    }

    // 가까운 벌레에 탑승 시도
    // Attempt to mount the nearest bug within radius
    void TryMount()
    {
        float checkRadius = 2f;
        Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius);

        Transform closestBug = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Bug")) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);

            // Ladybug 우선
            // Prioritize ladybugs over other bugs
            if (hit.TryGetComponent<LadybugMovement>(out var ladybug))
            {
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestBug = ladybug.transform;
                }
            }
            // 개미가 있을 경우
            else if (hit.TryGetComponent<AntMovement>(out var ant))
            {
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestBug = ant.transform;
                }
            }
        }

        if (closestBug != null)
        {
            Mount(closestBug);
        }
    }

    // 탑승 처리 함수
    // Mount onto the given bug transform
    void Mount(Transform bug)
    {
        isMounted = true;
        mountedBug = bug;

        controller.enabled = false;
        transform.SetParent(bug); // 벌레에 붙임

        // 위치 및 회전 조정 (벌레 타입별)
        // Set local position/rotation based on bug type
        if (bug.TryGetComponent<LadybugMovement>(out _))
        {
            transform.localPosition = new Vector3(0, -0.2f, 0.5f);
            transform.localRotation = Quaternion.identity;
        }
        else if (bug.TryGetComponent<AntMovement>(out _))
        {
            transform.localPosition = new Vector3(0, -0.6f, 1f);
            transform.localRotation = Quaternion.Euler(0f, 90f, 0f); // 개미는 반대 방향
        }
        else
        {
            transform.localPosition = new Vector3(0, -1.2f, 0);
            transform.localRotation = Quaternion.identity;
        }

        // 애니메이션 및 상태 갱신
        // Trigger mount animation and update state
        animator.SetTrigger("is_riding");
        animator.SetBool("is_riding_on_bug", true);

        // 벌레에게도 Mounted 상태 전달
        // Notify bug of mounting
        bug.GetComponent<BugMovement>()?.SetMounted(true);
        bug.GetComponent<BugFlightMovement>()?.SetMounted(true);
        bug.GetComponent<AntMovement>()?.SetMounted(true);
        bug.GetComponent<LadybugMovement>()?.SetMounted(true);
    }

    // 하차 처리 함수
    // Handle unmounting from the bug
    public void Unmount()
    {
        if (!isMounted || mountedBug == null) return;

        isMounted = false;

        Vector3 dismountPos = mountedBug.position + Vector3.up * 0.04f;

        transform.SetParent(null);
        transform.position = dismountPos;

        float bugYRotation = mountedBug.eulerAngles.y;

        if (mountedBug.TryGetComponent<AntMovement>(out _))
        {
            transform.rotation = Quaternion.Euler(0f, bugYRotation + 180f, 0f); // 개미는 반전
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, bugYRotation, 0f);
        }

        controller.enabled = true;

        animator.SetBool("is_riding_on_bug", false);

        mountedBug.GetComponent<BugMovement>()?.SetMounted(false);
        mountedBug.GetComponent<BugFlightMovement>()?.SetMounted(false);
        mountedBug.GetComponent<AntMovement>()?.SetMounted(false);
        mountedBug.GetComponent<LadybugMovement>()?.SetMounted(false);

        mountedBug = null;
    }

    // 장애물에 부딪혔을 때 낙하 처리
    // Called when colliding with obstacles to simulate fall
    void Fall()
    {
        isFalling = true;
        controller.enabled = false;
        animator.SetTrigger("is_falling");
        StartCoroutine(SimulateFall());
        Debug.Log("장애물과 충돌하여 추락"); // Hit obstacle and fell
    }

    // 외부에서 강제로 낙하시키기
    // External call to force fall from bug
    public void ForceFallFromBug()
    {
        if (isMounted)
        {
            Unmount(); // 먼저 하차
        }

        controller.enabled = true;
        Fall();
    }

    // 낙하 코루틴
    // Simulate falling for a set duration
    IEnumerator SimulateFall()
    {
        float fallDuration = 2f;
        float fallSpeed = -1f;
        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            velocity = new Vector3(0, fallSpeed, 0);
            controller.Move(velocity * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        RecoverFromFall();
    }

    // 낙하 후 회복 처리
    // Reset fall state and re-enable control
    void RecoverFromFall()
    {
        isFalling = false;
        velocity = Vector3.zero;
        controller.enabled = true;
        Debug.Log("추락 후 복구됨"); // Recovered after fall
    }

    // 장애물 충돌 감지
    // Trigger fall if collided with obstacle
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle") && !isFalling)
        {
            Fall();
        }
    }
}
