using System.Collections;
using UnityEngine;
using TMPro;
[RequireComponent(typeof(Rigidbody))]
public class KatydidMovement : MonoBehaviour, IRideableBug
{
    public float moveSpeed = 4f;
    public float jumpForce = 2f;
    public float superJumpForce = 3f; // 슈퍼점프 힘
    public float obstacleCheckDist = 0.8f;
    public float rotationSpeed = 180f;
    public float jumpCooldown = 3f;
    public LayerMask obstacleMask;
    public GameObject shiftJumpUI;
    public TMP_Text countdownText;
    private bool isMounted = false;
    private bool isGrounded = true;
    private bool isApproaching = false;
    private bool cansuperjump = true;
    private bool isSuperJump = false; // 슈퍼점프 상태
    private float lastJumpTime = -10f;
    private Rigidbody rb;
    private Animator katydidAnimator;
    private Coroutine approachRoutine;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        katydidAnimator = GetComponent<Animator>();
    }
    void Update()
    {
        if (cansuperjump && Input.GetKeyDown(KeyCode.LeftShift))
            isSuperJump = true;
    }
    void FixedUpdate()
    {
        if (!isMounted) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (Mathf.Abs(h) > 0.01f)
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
        }
        if (isGrounded)
        {
            katydidAnimator.SetTrigger("is_jumping");
            if (isSuperJump)
            {
                rb.AddForce(Vector3.up * superJumpForce, ForceMode.Impulse);
                cansuperjump = false;
                isSuperJump = false;
                shiftJumpUI?.SetActive(false);
                StartCoroutine(JumpBoostCooldown());
            }
            else
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            lastJumpTime = Time.time;
            isGrounded = false;
        }
    }
    IEnumerator JumpBoostCooldown()
    {
        float countdown = jumpCooldown;
        while (countdown > 0)
        {
            countdownText.text = $"You can jump after {Mathf.Ceil(countdown)}s";
            countdown -= Time.deltaTime;
            yield return null;
        }
        countdownText.text = "";
        cansuperjump = true;
        shiftJumpUI?.SetActive(true);
    }
    void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (col.gameObject.CompareTag("Obstacle"))
        {
            katydidAnimator?.SetTrigger("is_dropping");
            Debug.Log("is_dropping triggered");
            var player = GetComponentInChildren<PlayerMovement>();
            player?.ForceFallFromBug();
            SetMounted(false);
            Destroy(gameObject, 2f);
        }
        else if ((col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Bug")) && (Time.time - lastJumpTime > 0.1f))
        {
            isGrounded = true;
        }
    }
    public void SetMounted(bool mounted)
    {
        isMounted = mounted;
        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            shiftJumpUI?.SetActive(false);
            countdownText.text = "";
        }
        else
        {
            isGrounded = true; // 탑승할 때 초기화
            shiftJumpUI?.SetActive(true);
            Destroy(GetComponent<MoveToTarget>());
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
    public void SetUI(GameObject JumpUI, TMP_Text countdown)
    {
        shiftJumpUI = JumpUI;
        countdownText = countdown;
    }
}
