using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class AntMovement : MonoBehaviour, IRideableBug
{
    public float acceleration = 5f;      // 가속도
    public float maxSpeed = 5f;           // 최대 속도
    public float rotationSpeed = 45f;    // 회전 속도
    public float dashSpeed = 10f;
    public float dashDuration = 1f;
    public float dashCooldown = 1f;
    public float obstacleCheckDist = 0.8f;
    public float moveSpeed = 10f;

    public LayerMask obstacleMask;
    public GameObject DashUI;
    public TMP_Text countdownText;

    private bool isMounted = false;
    private bool isApproaching = false;
    private bool isDashing = false;
    private bool canDash = true;

    private Rigidbody rb;
    private Animator antAnimator;
    private Coroutine approachRoutine;
    private Vector3 currentVel = Vector3.zero;   // 가속도 적용 속도 벡터

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        antAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isMounted || isDashing || !canDash) return;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        if (!isMounted || isApproaching) return;

        if (isDashing)
        {
            rb.MovePosition(rb.position + currentVel * Time.fixedDeltaTime);
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        Vector3 forward = rb.rotation * Vector3.forward;
        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 targetVel = forward * v * maxSpeed;
            currentVel = Vector3.MoveTowards(currentVel, targetVel, acceleration * Time.fixedDeltaTime);

            // Clamp speed
            if (currentVel.magnitude > maxSpeed)
                currentVel = currentVel.normalized * maxSpeed;

            Vector3 rayOrigin = rb.position + Vector3.up * 0.3f;
            if (!Physics.SphereCast(rayOrigin, 0.4f, forward, out _, obstacleCheckDist, obstacleMask))
            {
                rb.MovePosition(rb.position + currentVel * Time.fixedDeltaTime);
            }
        }
        else
        {
            currentVel = Vector3.MoveTowards(currentVel, Vector3.zero, acceleration * Time.fixedDeltaTime);

            // Clamp speed
            if (currentVel.magnitude > maxSpeed)
                currentVel = currentVel.normalized * maxSpeed;

            rb.MovePosition(rb.position + currentVel * Time.fixedDeltaTime);
        }

        antAnimator?.SetBool("is_walking", currentVel.magnitude > 0.1f);
    }


    public IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        DashUI?.SetActive(false);

        antAnimator?.SetTrigger("is_dashing");

        Vector3 dashDir = rb.rotation * Vector3.forward;
        currentVel = dashDir * dashSpeed;

        float dashElapsed = 0f;
        while (dashElapsed < dashDuration)
        {
            // dash 동안 속도 유지
            currentVel = dashDir * dashSpeed;
            dashElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;

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
            Destroy(GetComponent<MoveToTarget>());
        }
    }

    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }

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

    public void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;

        var player = GetComponentInChildren<PlayerMovement>();
        antAnimator?.SetTrigger("is_dropping");
        player?.ForceFallFromBug();
        SetMounted(false);
        Destroy(gameObject,2f);
    }

    public void SetUI(GameObject dashUI, TMP_Text countdown)
    {
        DashUI = dashUI;
        countdownText = countdown;
    }

}