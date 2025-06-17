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
            obstacleCheckDist
        );
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
        dashDir = rb.rotation * Vector3.forward;
        isDashing = true;

        yield return SkillWithCooldown(
            dashDuration,
            dashCooldown,
            () => animator?.SetTrigger("is_dashing"),
            () => isDashing = false
        );
    }
}