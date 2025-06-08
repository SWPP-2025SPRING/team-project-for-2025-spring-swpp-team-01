using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class KatydidMovement : MonoBehaviour, IRideableBug
{
    public float moveSpeed = 3f;
    public float jumpForce = 5f;
    public float obstacleCheckDist = 0.8f;
    public float rotationSpeed = 180f;
    public float jumpCooldown = 3f;

    public LayerMask obstacleMask;
    public GameObject shiftJumpUI;
    public TMP_Text countdownText;

    private bool isMounted = false;
    private readonly bool isJumping = false;
    private bool isGrounded = true;
    private bool isApproaching = false;
    private bool canJump = true;

    private Rigidbody rb;
    private Animator katydidAnimator;
    private Coroutine jumpRoutine;
    private Coroutine approachRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        katydidAnimator = GetComponent<Animator>();

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

            if (jumpRoutine == null)
                jumpRoutine = StartCoroutine(JumpWhenGrounded());
        }
        else
        {
            // 이동 중이 아니면 점프 중지
            if (jumpRoutine != null)
            {
                StopCoroutine(jumpRoutine);
                jumpRoutine = null;
            }
        }
    }

    IEnumerator JumpWhenGrounded()
    {
        while (true)
        {
            if (isGrounded)
            {
                katydidAnimator.SetTrigger("is_jumping");

                if (Input.GetKey(KeyCode.LeftShift) && canJump)
                {
                    rb.AddForce(Vector3.up * jumpForce * 2, ForceMode.Impulse);
                    canJump = false;
                    shiftJumpUI?.SetActive(false);
                    StartCoroutine(JumpBoostCooldown());
                }
                else
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                }
                isGrounded = false;
            }
            yield return null;
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
        canJump = true;
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
        else if (col.contacts.Length > 0 && col.contacts[0].normal.y > 0.5f)
        {
            // 착지 판정
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

            if (jumpRoutine != null)
            {
                StopCoroutine(jumpRoutine);
                jumpRoutine = null;
            }
        }
        else
        {
            shiftJumpUI?.SetActive(true);
            Destroy(GetComponent<MoveToTarget>());
        }

        isGrounded = true; // 탑승할 때 초기화
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
