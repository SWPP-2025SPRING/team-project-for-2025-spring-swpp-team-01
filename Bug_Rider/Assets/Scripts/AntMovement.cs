using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class AntMovement : MonoBehaviour, IRideableBug
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;
    public float dashSpeed = 15f;           // 대시 속도
    public float dashDuration = 0.4f;       // 대시 지속 시간
    public float dashCooldown = 1f;         // 대시 쿨타임
    public float obstacleCheckDist = 0.8f;
    public LayerMask obstacleMask;
    public GameObject DashUI;
    public TMP_Text countdownText;


    private bool isMounted = false;
    private bool isApproaching = false;
    private bool isDashing = false;
    private bool canDash = true;

    private Rigidbody rb;
    private Coroutine approachRoutine;
    private Animator antAnimator;



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        antAnimator = GetComponent<Animator>();
        DashUI?.SetActive(false);
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
        if (!isMounted) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");


        if (Mathf.Abs(v) > 0.01f && Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }


        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 forward = rb.rotation * Vector3.forward;

            Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
            if (!Physics.SphereCast(rayOrigin, 0.4f, forward, out _, obstacleCheckDist, obstacleMask))
            {
                Vector3 next = rb.position + forward * v * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(next);
            }
            antAnimator.SetBool("is_walking", true);
        }
        else
        {
            antAnimator.SetBool("is_walking", false);
        }
    }

    IEnumerator Dash()
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
        Debug.Log("Ant SetMounted called: " + mounted);
        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            DashUI?.SetActive(false);
        }
        else
        {
            DashUI?.SetActive(true);
        }
    }

    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }

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

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isApproaching = false;
    }

    void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;

        var player = GetComponentInChildren<PlayerMovement>();
        antAnimator?.SetTrigger("is_dropping");
        Debug.Log("is_dropping performed");
        player?.ForceFallFromBug();
        SetMounted(false);
    }
}
