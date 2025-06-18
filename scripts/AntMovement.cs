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

    public Sprite dashReadySprite;
    public Sprite dashActiveSprite;
    public Sprite dashCooldownSprite;

    protected override void Awake()
    {
        base.Awake();

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

        if (mounted)
            UIManager.Instance.ShowSkillAvailable(dashReadySprite);
        else
            UIManager.Instance.HideAllSkillUI();
    }

    void Update()
    {
        if (!isMounted) return;
        if (Input.GetKeyDown(KeyCode.LeftShift))
            StartCoroutine(Dash());
    }

    void FixedUpdate()
    {
        if (!isMounted) return;
        if (isDashing)
            rb.MovePosition(rb.position + dashDir * dashSpeed * Time.fixedDeltaTime);

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
        isDashing = true;
        AudioManager.Instance.PlayBug("Ant_Dash");

        yield return SkillWithCooldown(
            dashDuration,
            dashCooldown,
            () => {
                animator?.SetTrigger("is_dashing");
                UIManager.Instance.ShowSkillActive(dashActiveSprite);
            },
            () => {
                isDashing = false;
                UIManager.Instance.ShowSkillCooldown(dashCooldownSprite);
            }
        );

        UIManager.Instance.HideAllSkillUI(); 
    }
}