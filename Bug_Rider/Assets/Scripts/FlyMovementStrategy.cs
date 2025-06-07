using UnityEngine;
using System.Collections;
using TMPro;

public class FlyMovementStrategy : IBugMovementStrategy
{
    private readonly MonoBehaviour owner;
    private readonly TMP_Text countdownText;
    private readonly GameObject flyUI;

    private readonly Rigidbody rb;
    private readonly Animator animator;

    private Coroutine flightCoroutine;

    private float flightDuration = 5f;
    private bool isFlying = false;
    private bool canFly = true;
    private bool shouldAutoFall = false;
    private bool isAutoDescending = false;


    // 외부에서 사용할 수 있는 프로퍼티
    public bool CanFly => canFly;
    public float FlightDuration => flightDuration;

    public FlyMovementStrategy(MonoBehaviour owner, TMP_Text countdownText, GameObject flyUI, Rigidbody rb, Animator animator, bool shouldAutoFall = false)
    {
        this.owner = owner;
        this.countdownText = countdownText;
        this.flyUI = flyUI;
        this.rb = rb;
        this.animator = animator;
        this.shouldAutoFall = shouldAutoFall;
    }

    public void StartFlight()
    {
        if (canFly && !isFlying)
        {
            isFlying = true;
            canFly = false;
            flightCoroutine = owner.StartCoroutine(FlightRoutine());
        }
    }

    public void StopFlight()
    {
        if (flightCoroutine != null)
        {
            owner.StopCoroutine(flightCoroutine);
            flightCoroutine = null;
        }

        countdownText.text = "";
    }

    public void HandleMovement(Rigidbody rb, Animator animator, LayerMask obstacleMask, float moveSpeed, float rotationSpeed, float obstacleCheckDist)
    {
        // 항상 회전 가능하도록 하기 (비행 여부와 무관)
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.1f)
        {
            Quaternion deltaRotation = Quaternion.Euler(0f, h * rotationSpeed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // 비행 중에 상승/하강 조작
        if (isFlying)
        {
            // float ascendSpeed = 3f;
            // float descendSpeed = 2f;
            // float maxHeight = rb.position.y + 4f;
            // float minHeight = rb.position.y - 0.5f;

            // float targetY = rb.position.y;

            // if (Input.GetKey(KeyCode.Space))
            //     targetY += ascendSpeed * Time.fixedDeltaTime;
            // else
            //     targetY -= descendSpeed * Time.fixedDeltaTime;

            // targetY = Mathf.Clamp(targetY, minHeight, maxHeight);

            // Vector3 pos = rb.position;
            // pos.y = Mathf.MoveTowards(pos.y, targetY, (ascendSpeed + descendSpeed) * 0.5f * Time.fixedDeltaTime);
            // rb.MovePosition(pos);

            float ascendSpeed = 3f;
            float descendSpeed = 2f;
            float maxHeight = rb.position.y + 4f;
            float minHeight = rb.position.y - 0.5f;

            float targetY = rb.position.y;

            if (Input.GetKey(KeyCode.Space))
                targetY += ascendSpeed * Time.fixedDeltaTime;
            else
                targetY -= descendSpeed * Time.fixedDeltaTime;

            targetY = Mathf.Clamp(targetY, minHeight, maxHeight);

            // 이동 입력
             h = Input.GetAxis("Horizontal"); // ← → : 좌/우 평행이동
            float v = Input.GetAxis("Vertical");   // ↑ ↓ : 전/후 이동

            // 이동 방향 (벌의 로컬 축 기준)
            Vector3 right = rb.transform.right * h;
            Vector3 forward = rb.transform.forward * v;
            Vector3 moveDir = (right + forward).normalized;

            // 목표 위치 계산 (X/Z 평면 이동 + y 높이 반영)
            Vector3 targetPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
            targetPos.y = Mathf.MoveTowards(rb.position.y, targetY, (ascendSpeed + descendSpeed) * 0.5f * Time.fixedDeltaTime);

            rb.MovePosition(targetPos);

            // 회전 (좌/우 입력 기준으로 제자리 회전)
            if (Mathf.Abs(h) > 0.1f)
            {
                Quaternion deltaRotation = Quaternion.Euler(0f, h * rotationSpeed * Time.fixedDeltaTime, 0f);
                rb.MoveRotation(rb.rotation * deltaRotation);
            }
            animator?.SetFloat("fly_speed", moveDir.magnitude);
        }



        else if (isAutoDescending)
        {
            // 자동 하강 모드에서는 사용자의 Space 키 무시
            float descendSpeed = 2f;
            Vector3 pos = rb.position;
            pos.y -= descendSpeed * Time.fixedDeltaTime;
            rb.MovePosition(pos);
        }
    }


    private IEnumerator FlightRoutine()
    {
        float groundY = rb.position.y;
        rb.useGravity = false;

        isFlying = true;
        canFly = false;

        flyUI?.SetActive(false);
        // 1. ascend 상태가 Animator에 존재하면 먼저 실행
        bool hasAscendState = animator.HasState(0, Animator.StringToHash("Base Layer.ascend"));
        if (hasAscendState)
        {
            animator?.SetTrigger("is_ascend");

            float ascendLength = GetClipLength("ascend");
            yield return new WaitForSeconds(ascendLength);
        }

        // 2. 비행 상태 진입
        animator?.SetBool("is_flying", true);


        float timer = 0f;
        float ascendSpeed = 3f;
        float descendSpeed = 2f;

        countdownText.color = new Color(1f, 0.23f, 0.23f);

        while (timer < flightDuration)
        {
            countdownText.text = $"You can fly for {Mathf.Ceil(flightDuration - timer)}s";
            timer += Time.deltaTime;
            yield return null;
        }

        // 비행 종료 → 자동 착륙 시작
        isFlying = false;
        isAutoDescending = true;

        // 자동 착륙
        animator?.SetTrigger("is_descend");
        animator?.SetBool("is_flying", false); // is_flying 중단

        while (rb.position.y > groundY + 0.05f)
        {
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, groundY, descendSpeed * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        // 착륙 완료
        isAutoDescending = false;
        rb.useGravity = true;
        animator?.SetTrigger("descend_to_walk");
        isFlying = false;

        // 자동 낙하가 필요한 경우
        if (shouldAutoFall)
        {
            var player = rb.GetComponentInChildren<PlayerMovement>();
            player?.ForceFallFromBug();

            // ✅ UI 숨김 처리
            flyUI?.SetActive(false);
            countdownText?.gameObject.SetActive(false);
            animator?.SetTrigger("is_drop");
        }


        // 쿨타임
        float cooldown = 7f;
        countdownText.color = new Color(0.23f, 0.55f, 1f);
        while (cooldown > 0)
        {
            countdownText.text = $"You can fly after {Mathf.Ceil(cooldown)}s";
            cooldown -= Time.deltaTime;
            yield return null;
        }

        countdownText.text = "";
        canFly = true;
        flyUI?.SetActive(true);

    }
    private float GetClipLength(string clipName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 0f;
    }

}
