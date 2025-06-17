using UnityEngine;
public class WalkMovementStrategy
{
    private Rigidbody rb;
    private Animator animator;
    private LayerMask obstacleMask;
    private float acceleration;
    private float maxSpeed;
    private float angularAcceleration;
    private float maxAngularSpeed;
    private float obstacleCheckDist;
    private bool useAcceleration;
    private bool useAngularAcceleration;
    private float currentAngularSpeed = 0f;
    public WalkMovementStrategy(
        Rigidbody rb,
        Animator animator,
        LayerMask obstacleMask,
        float acceleration,
        float maxSpeed,
        float angularAcceleration,
        float maxAngularSpeed,
        float obstacleCheckDist,
        bool useAcceleration = true,
        bool useAngularAcceleration = false
    )
    {
        this.rb = rb;
        this.animator = animator;
        this.obstacleMask = obstacleMask;
        this.acceleration = acceleration;
        this.maxSpeed = maxSpeed;
        this.angularAcceleration = angularAcceleration;
        this.maxAngularSpeed = maxAngularSpeed;
        this.obstacleCheckDist = obstacleCheckDist;
        this.useAcceleration = useAcceleration;
        this.useAngularAcceleration = useAngularAcceleration;
    }
    public void HandleMovement(float h, float v)
    {
        // 각가속도/즉시회전
        if (Mathf.Abs(h) > 0.01f)
        {
            if (useAngularAcceleration)
            {
                float targetAngular = h * maxAngularSpeed;
                currentAngularSpeed = Mathf.MoveTowards(currentAngularSpeed, targetAngular, angularAcceleration * Time.fixedDeltaTime);
                Quaternion deltaRotation = Quaternion.Euler(0, currentAngularSpeed * Time.fixedDeltaTime, 0);
                rb.MoveRotation(rb.rotation * deltaRotation);
            }
            else
            {
                float turnAmount = h * maxAngularSpeed * Time.fixedDeltaTime;
                Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
                rb.MoveRotation(rb.rotation * deltaRotation);
            }
        }
        else
        {
            currentAngularSpeed = Mathf.MoveTowards(currentAngularSpeed, 0f, angularAcceleration * Time.fixedDeltaTime);
        }
        // 가속도 적용 (이전과 동일)
        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 forward = rb.rotation * Vector3.forward;
            Vector3 rayOrigin = rb.position + Vector3.up * 0.3f;
            if (!Physics.SphereCast(rayOrigin, 0.4f, forward, out _, obstacleCheckDist, obstacleMask))
            {
                if (useAcceleration)
                {
                    rb.AddForce(forward * v * acceleration, ForceMode.Acceleration);
                }
                else
                {
                    Vector3 targetVelocity = forward * v * maxSpeed;
                    Vector3 velocityChange = targetVelocity - new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    rb.AddForce(velocityChange, ForceMode.VelocityChange);
                }
            }
        }
        // 최대 속도 제한(수평만)
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limited = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limited.x, rb.velocity.y, limited.z);
        }
        animator?.SetBool("is_walking", flatVel.magnitude > 0.1f);
    }
}