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

    public void StartFlight()
    {
        if (canFly && !isFlying)
        {
            isFlying = true;
            canFly = false;
            maxHeight = rb.position.y + 20f;
            minHeight = rb.position.y - 20f;
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
        // 비행 중에 상승/하강 조작
        if (isFlying)
        {

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
                Debug.Log("raycasted");
                // 충돌한 오브젝트가 Ground 태그일 경우
                if (hit.collider.CompareTag("Ground") && hit.distance <= 0.5f)
                {
                    Debug.Log("Collided");
                    break;
                }
            }

            // 내려가기
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
