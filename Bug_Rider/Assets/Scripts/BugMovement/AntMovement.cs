using UnityEngine;
using System.Collections;
using TMPro;
public class AntMovement : RideableBugBase
{
    private WalkMovementStrategy walkStrategy;
    public float dashSpeed, dashDuration, dashCooldown;
    private Vector3 dashVel = Vector3.zero;
    private bool isDashing = false;
    public LayerMask obstacleMask;
    private Vector3 dashDir;
    private float dashStartTime;
    public Sprite dashReadySprite;
    public Sprite dashActiveSprite;
    public Sprite dashCooldownSprite;
    protected override void Awake()
    {
        base.Awake();
        meshRot = Quaternion.Euler(-90f, 180f, 0f);
        mesh = transform.Find("Armature");
        walkStrategy = new WalkMovementStrategy(
            rb,
            animator,
            obstacleMask,
            acceleration,
            maxSpeed,
            angularAcceleration,
            maxAngularSpeed,
            obstacleCheckDist,
            "Ant"
        );
    }
    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);
        Debug.Log("[AntMovement] SetMounted " + mounted);
        if (mounted)
            UIManager.Instance.OnMountSkillUI("dash");
        else
            UIManager.Instance.HideAllSkillUI();
    }
    void Update()
    {
        if (!isMounted) return;
        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(Dash());
    }
    void FixedUpdate()
    {
        base.RotateMesh();
        if (!isMounted) return;
        // if (isDashing)
        //     rb.MovePosition(rb.position + dashDir * dashSpeed * Time.fixedDeltaTime);
        if (isDashing)
        {
            float t = (Time.time - dashStartTime) / dashDuration;
            float speed = Mathf.Lerp(dashSpeed, maxSpeed, t);
            rb.velocity = dashDir.normalized * speed;
            return;
        }
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        walkStrategy.HandleMovement(h, v);
    }
    IEnumerator Dash()
    {
        if (!CanUseSkill())
        {
            Debug.Log("Skill is not available (still active or cooling down).");
            yield break;
        }
        dashDir = rb.rotation * Vector3.forward;
        dashStartTime = Time.time;
        isDashing = true;
        AudioManager.Instance.PlayBugSkill("Ant_Dash");
        yield return SkillWithCooldown(
            dashDuration,
            dashCooldown,
            () => {
                animator?.SetTrigger("is_dashing");
            },
            () => {
                isDashing = false;
                UIManager.Instance.ShowSkillCooldown("dash");
            },
            "dash"
        );
    }
}