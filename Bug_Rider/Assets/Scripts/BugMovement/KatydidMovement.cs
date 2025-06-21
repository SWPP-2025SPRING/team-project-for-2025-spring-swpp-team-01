using UnityEngine;
using TMPro;
using System.Collections;

public class KatydidMovement : RideableBugBase
{
    public float moveSpeed = 4f;
    public float jumpForce = 2f;
    public float superJumpForce = 3f;
    public float rotationSpeed = 180f;
    public float jumpCooldown = 3f;
    public LayerMask obstacleMask;

    private bool isGrounded = true;
    private bool canSuperJump = true;
    private bool isSuperJump = false;
    private float lastJumpTime = -10f;

    private Coroutine cooldownRoutine;

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        if (!isMounted) return;

        if (canSuperJump && Input.GetKeyDown(KeyCode.LeftShift))
            isSuperJump = true;
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 회전
        if (Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // 전후 이동
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

        // 점프
        if (isGrounded)
        {
            animator?.SetTrigger("is_jumping");

            if (isSuperJump)
            {
                AudioManager.Instance?.PlayBug("Ketydid_Superjump");
                rb.AddForce(Vector3.up * superJumpForce, ForceMode.Impulse);
                canSuperJump = false;
                isSuperJump = false;
                cooldownRoutine = StartCoroutine(SuperJumpCooldown());
            }
            else
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                AudioManager.Instance?.PlayBug("Ketydid_Jump");
            }

            lastJumpTime = Time.time;
            isGrounded = false;
        }
    }

    IEnumerator SuperJumpCooldown()
    {
        if (!CanUseSkill())
        {
            Debug.Log("Skill is not available (still active or cooling down).");
            yield break;
        }

        yield return SkillWithCooldown(
            0f,
            jumpCooldown,
            null,
            null
        );
        canSuperJump = true;
    }



    protected override void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;

        if (col.gameObject.CompareTag("Obstacle"))
        {
            AudioManager.Instance?.PlayBug("Stun");
            animator?.SetTrigger("is_dropping");
            GetComponentInChildren<PlayerMovement>()?.ForceFallFromBug();
            SetMounted(false);
            Destroy(gameObject, 2f);
        }
        else if ((col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Bug")) && (Time.time - lastJumpTime > 0.1f))
        {
            isGrounded = true;
        }
    }

    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);

        if (!mounted)
        {
            isGrounded = false;
            if (cooldownRoutine != null) StopCoroutine(cooldownRoutine);
        }
        else
        {
            isGrounded = true;
        }
    }
}
