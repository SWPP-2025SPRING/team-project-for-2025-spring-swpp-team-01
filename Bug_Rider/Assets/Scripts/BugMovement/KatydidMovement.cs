using UnityEngine;
using TMPro;
using System.Collections;
public class KatydidMovement : RideableBugBase
{
    public float moveSpeed = 4f;
    public float jumpForce = 2f;
    public float rotationSpeed = 180f;
    public float jumpCooldown = 3f;
    public LayerMask obstacleMask;
    private bool canSuperJump = true;
    private bool isSuperJump = false;
    private float lastJumpTime = -10f;
    private Coroutine cooldownRoutine;
    [Header("Jump UI Sprites")]
    public Sprite jumpReadySprite;
    public Sprite jumpActiveSprite;
    public Sprite jumpCooldownSprite;
    protected override void Awake()
    {
        base.Awake();
        mesh = transform.Find("Armature");
        meshRot = Quaternion.Euler(-90f, 0f, 0f);
    }
    void Update()
    {
        if (!isMounted) return;
        if (canSuperJump && Input.GetKeyDown(KeyCode.Space))
            isSuperJump = true;
    }
    void FixedUpdate()
    {
        base.RotateMesh();
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
        if (isSuperJump)
        {
            AudioManager.Instance?.PlayBug("Ketydid_Superjump");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canSuperJump = false;
            isSuperJump = false;
            cooldownRoutine = StartCoroutine(SuperJumpCooldown());
        }
    }
    IEnumerator SuperJumpCooldown()
    {
        if (!CanUseSkill())
        {
            yield break;
        }
        // 준비 UI는 이미 떴으므로 생략
        yield return SkillWithCooldown(
            0f,
            jumpCooldown,
            null,
            () => UIManager.Instance.ShowSkillCooldown("jump"),
            "jump"
        );
        // UIManager.Instance.HideAllSkillUI();
        canSuperJump = true;
    }
    protected override void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (col.gameObject.CompareTag("Obstacle"))
        {
            AudioManager.Instance?.StopBugAll();
            AudioManager.Instance?.PlayObstacle("Stun");
            animator?.SetTrigger("is_dropping");
            GetComponentInChildren<PlayerMovement>()?.ForceFallFromBug();
            SetMounted(false);
            Destroy(gameObject, 2f);
        }
        else if ((col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Bug")) && (Time.time - lastJumpTime > 0.5f))
        {
            animator?.SetTrigger("is_jumping");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            AudioManager.Instance?.PlayBug("Ketydid_Jump");
            lastJumpTime = Time.time;
        }
    }
    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);
        if (!mounted)
        {
            if (cooldownRoutine != null) StopCoroutine(cooldownRoutine);
            UIManager.Instance.HideAllSkillUI();
        }
        else
        {
            Jump();
            if (canSuperJump)
                UIManager.Instance?.OnMountSkillUI("jump");
        }
    }
    private void Jump()
    {
            animator?.SetBool("is_walking", false);
            animator?.SetTrigger("is_jumping");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            AudioManager.Instance?.PlayBug("Ketydid_Jump");
            lastJumpTime = Time.time;
    }
}