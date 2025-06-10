using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class AntMovement : MonoBehaviour, IRideableBug
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.4f;
    public float dashCooldown = 1f;
    public float obstacleCheckDist = 0.8f;

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

    private IBugMovementStrategy walkStrategy;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        antAnimator = GetComponent<Animator>();
        walkStrategy = new WalkMovementStrategy();
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
        if (!isMounted || isApproaching || isDashing) return;

        walkStrategy.HandleMovement(rb, antAnimator, obstacleMask, moveSpeed, rotationSpeed, obstacleCheckDist);
    }

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
