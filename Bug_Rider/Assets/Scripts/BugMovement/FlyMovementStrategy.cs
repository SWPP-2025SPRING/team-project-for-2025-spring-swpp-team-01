using UnityEngine;
public class FlyMovementStrategy
{
    readonly Rigidbody rb;
    readonly Animator animator;
    readonly float baseFlySpeed;
    readonly float flyRotationSpeed;
    readonly float maxHeight;
    readonly float ascendForce;
    bool isFlying;
    float flightTimer;
    float initialY;
    public bool IsFlying => isFlying;
    public float FlightTime => flightTimer;
    private string bugName;  // ✅ 추가됨

    public FlyMovementStrategy(
        Rigidbody rb,
        Animator animator,
        float flySpeed,
        float flyRotationSpeed,
        float maxHeight,
        string bugName, // ✅ 추가됨
        float ascendForce = 18f)
    {
        this.rb = rb;
        this.animator = animator;
        this.baseFlySpeed = flySpeed;
        this.flyRotationSpeed = flyRotationSpeed;
        this.maxHeight = maxHeight;
        this.ascendForce = ascendForce;
        this.bugName = bugName;
    }
    public void SetFlying(bool value)
    {
        if (value)
        {
            isFlying = true;
            flightTimer = 0f;
            initialY = rb.position.y;
            animator?.ResetTrigger("is_drop");           // 추가
            animator?.SetBool("is_flying", true);
            AudioManager.Instance?.PlayBug($"{bugName}_Fly");
        }
        else
        {
            isFlying = false;
            animator?.SetBool("is_flying", false);
            animator?.SetTrigger("is_drop");
            AudioManager.Instance?.StopBug();
        }
    }
    public void HandleMovement(float h, float v, bool isSpace)
    {
        if (!isFlying) return;
        flightTimer += Time.fixedDeltaTime;
        float speedFactor = flightTimer < 1f ? 0.5f : 1f;
        float forceFactor = flightTimer < 1f ? 1f : 1f;
        /* 회전만 수행 */
        if (Mathf.Abs(h) > 0.01f)
        {
            float turn = h * flyRotationSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turn, 0));
        }
        animator.speed = baseFlySpeed * speedFactor;

        /* 입력이 있을 때만 이동 */
        Vector3 flatVel = Vector3.zero;
        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 forward = rb.rotation * Vector3.forward * v;
            flatVel = forward.normalized * baseFlySpeed * speedFactor;
        }
        rb.velocity = new Vector3(flatVel.x, rb.velocity.y, flatVel.z);
        /* 상승 */
        if (isSpace)
        {
            float remain = Mathf.Max(0f, maxHeight - (rb.position.y - initialY));
            if (remain > 0f)
            {
                float t = remain / maxHeight;
                rb.AddForce(Vector3.up * ascendForce * forceFactor * t * t,
                            ForceMode.Acceleration);
            }
        }
    }
}