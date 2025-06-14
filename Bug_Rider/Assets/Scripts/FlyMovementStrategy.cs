using UnityEngine;
using System.Collections;
using TMPro;
public class FlyMovementStrategy
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
    public float ascendSpeed = 2f;
    public float descendSpeed = 3f;
    private float maxHeight = 100f;
    private float minHeight = 0f;
    // 외부에서 사용할 수 있는 프로퍼티
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
        if (isFlying)
        {
            float y = rb.position.y;
            float ascendLimit = maxHeight;
            float descendLimit = minHeight;
            // 시간 경과에 따른 누적상승값 감쇠(등비수렴)
            if (Input.GetKey(KeyCode.Space))
            {
                // 상승 누적치를 누적변수로 관리
                // t: 상승 누른 누적 시간(초) — 이 값은 별도 변수로 기록 필요(아래 예시)
                flightAscendTime += Time.fixedDeltaTime;
                // 등비수열 감쇠: deltaY = maxDeltaY * (1 - exp(-감쇠계수 * t))
                // 감쇠계수(decayRate)는 값이 클수록 빨리 수렴
                float decayRate = 1.2f;
                float maxDeltaY = 7f; // 최대로 더할 수 있는 높이
                float deltaY = maxDeltaY * (1f - Mathf.Exp(-decayRate * flightAscendTime));
                y = Mathf.Min(initialFlightY + deltaY, ascendLimit);
            }
            else
            {
                // Space 떼면 바로 감쇠시간 리셋, 하강
                flightAscendTime = 0f;
                y -= descendSpeed * Time.fixedDeltaTime;
                y = Mathf.Max(y, descendLimit);
            }
            Vector3 pos = rb.position;
            pos.y = y;
            rb.MovePosition(pos);
        }
    }
    // public void HandleMovement(Rigidbody rb, Animator animator, LayerMask obstacleMask, float moveSpeed, float rotationSpeed, float obstacleCheckDist)
    // {
    //     if (isFlying)
    //     {
    //         float y = rb.position.y;
    //         float ascendLimit = maxHeight;
    //         // 등비수열 감쇠 상승(입력 없이 자동 적용)
    //         flightAscendTime += Time.fixedDeltaTime;
    //         float decayRate = 1.2f;
    //         float maxDeltaY = 7f; // 상승 한계
    //         float deltaY = maxDeltaY * (1f - Mathf.Exp(-decayRate * flightAscendTime));
    //         y = Mathf.Min(initialFlightY + deltaY, ascendLimit);
    //         Vector3 pos = rb.position;
    //         pos.y = y;
    //         rb.MovePosition(pos);
    //     }
    // }
    // 클래스 내부에 추가
    private float flightAscendTime = 0f;
    private float initialFlightY = 0f;
    // 비행 시작 시점에 초기화 필요
    public void StartFlight()
    {
        if (canFly && !isFlying)
        {
            isFlying = true;
            canFly = false;
            maxHeight = rb.position.y + 20f;
            minHeight = rb.position.y - 20f;
            initialFlightY = rb.position.y;
            flightAscendTime = 0f; // 여기서 초기화
            flightCoroutine = owner.StartCoroutine(FlightRoutine());
        }
    }
    private IEnumerator FlightRoutine()
    {
        float descendSpeedFall = 8f;
        rb.useGravity = false;
        isFlying = true;
        canFly = false;
        flyUI?.SetActive(false);
        animator?.SetTrigger("is_ascend");
        float timer = 0f;
        countdownText.color = new Color(1f, 0.23f, 0.23f);
        while (timer < flightDuration)
        {
            countdownText.text = $"You can fly for {Mathf.Ceil(flightDuration - timer)}s";
            timer += Time.deltaTime;
            yield return null;
        }
        isFlying = false;
        // 자동 착륙
        animator?.SetTrigger("is_descend");
        while (true)
        {
            // 바닥 감지 Raycast (모든 오브젝트와 충돌)
            if (Physics.Raycast(rb.position, Vector3.down, out RaycastHit hit, 10f))
            {
                if (hit.collider.CompareTag("Ground") && hit.distance <= 0.5f)
                {
                    break;
                }
            }
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, pos.y - 1f, descendSpeedFall * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }
        rb.useGravity = true;
        animator?.SetTrigger("descend_to_walk");
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
}