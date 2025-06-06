using UnityEngine;
using System.Collections;
using TMPro;

// 벌레 비행을 전략 패턴으로 처리하는 클래스
// Strategy pattern class that handles flying movement for bugs
public class FlyMovementStrategy : IBugMovementStrategy
{
    private readonly MonoBehaviour owner; // 코루틴 실행용 / Reference to run coroutines
    private readonly TMP_Text countdownText; // 비행 시간 안내 텍스트 / Text UI for flight timer
    private readonly GameObject flyUI; // 비행 관련 UI 오브젝트 / UI element for flight availability

    private readonly Rigidbody rb;
    private readonly Animator animator;

    private Coroutine flightCoroutine;

    private float flightDuration = 5f; // 비행 지속 시간 / Duration of flight
    private bool isFlying = false;
    private bool canFly = true;

    public bool CanFly => canFly;
    public float FlightDuration => flightDuration;

    public FlyMovementStrategy(MonoBehaviour owner, TMP_Text countdownText, GameObject flyUI, Rigidbody rb, Animator animator)
    {
        this.owner = owner;
        this.countdownText = countdownText;
        this.flyUI = flyUI;
        this.rb = rb;
        this.animator = animator;
    }

    // 비행 시작
    // Initiates flight if available
    public void StartFlight()
    {
        if (canFly && !isFlying)
        {
            isFlying = true;
            canFly = false;
            flightCoroutine = owner.StartCoroutine(FlightRoutine());
        }
    }

    // 비행 중단 (즉시 종료)
    // Immediately stop the current flight
    public void StopFlight()
    {
        if (flightCoroutine != null)
        {
            owner.StopCoroutine(flightCoroutine);
            flightCoroutine = null;
        }
        countdownText.text = "";
    }

    // 비행 중 상하 이동 제어
    // Handle vertical movement input during flight (Space to ascend)
    // space bar 누르면 계속 maxheight까지 계속 올라가도록
    public void HandleMovement(Rigidbody rb, Animator animator, LayerMask obstacleMask, float moveSpeed, float rotationSpeed, float obstacleCheckDist)
    {
        if (isFlying)
        {
            float ascendSpeed = 0.3f;
            float descendSpeed = 0.2f;
            float maxHeight = rb.position.y + 0.3f;
            float minHeight = rb.position.y - 0.02f;

            float targetY = rb.position.y;

            if (Input.GetKey(KeyCode.Space))
                targetY += ascendSpeed * Time.fixedDeltaTime;
            else
                targetY -= descendSpeed * Time.fixedDeltaTime;

            targetY = Mathf.Clamp(targetY, minHeight, maxHeight);

            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, targetY, (ascendSpeed + descendSpeed) * 0.5f * Time.fixedDeltaTime);
            rb.MovePosition(pos);
        }
    }

    // 비행 진행, 착륙, 쿨타임을 처리하는 코루틴
    // Coroutine to handle flight time, auto landing, and cooldown
    private IEnumerator FlightRoutine()
    {
        float groundY = rb.position.y;
        rb.useGravity = false;

        isFlying = true;
        canFly = false;

        flyUI?.SetActive(false);
        animator?.SetTrigger("is_ascend");

        float timer = 0f;
        float ascendSpeed = 3f;
        float descendSpeed = 2f;

        countdownText.color = new Color(1f, 0.23f, 0.23f); // 붉은색 (비행 중)

        while (timer < flightDuration)
        {
            countdownText.text = $"You can fly for {Mathf.Ceil(flightDuration - timer)}s";
            timer += Time.deltaTime;
            yield return null;
        }

        // 자동 착륙 처리
        animator?.SetTrigger("is_descend");
        while (rb.position.y > groundY + 0.005f)
        {
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, groundY, descendSpeed * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        rb.useGravity = true;
        animator?.SetTrigger("descend_to_walk");
        isFlying = false;

        // 비행 쿨타임
        // 쿨타임 끝나면 그대로 낙하해야함 
        float cooldown = 7f;
        countdownText.color = new Color(0.23f, 0.55f, 1f); // 파란색 (쿨타임 중)

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
}
