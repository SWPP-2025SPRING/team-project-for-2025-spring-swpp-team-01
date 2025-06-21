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
    private string bugName;  // :흰색_확인_표시: 추가됨
    private bool isWalkingSoundPlaying = false;
    public WalkMovementStrategy(
        Rigidbody rb,
        Animator animator,
        LayerMask obstacleMask,
        float acceleration,
        float maxSpeed,
        float angularAcceleration,
        float maxAngularSpeed,
        float obstacleCheckDist,
        string bugName, // :흰색_확인_표시: 추가됨
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
        this.bugName = bugName;
        this.useAcceleration = useAcceleration;
        this.useAngularAcceleration = useAngularAcceleration;
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationY
                        | RigidbodyConstraints.FreezeRotationZ;
        rb.centerOfMass = new Vector3(0, -1.0f, 0);
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
        // 최대 속도 제한
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        // animator?.SetBool("is_walking", flatVel.magnitude > 0.1f);
        // if (flatVel.magnitude > 0.1f && bugName != "Player")
        //     AudioManager.Instance?.PlayBug($"{bugName}_Walk", true);
        // else if(bugName != "Player") AudioManager.Instance?.StopBug();
        bool isWalking = rb.velocity.magnitude > 0.1f;
        animator?.SetBool("is_walking", isWalking);
        if (isWalking)
            {
                if (!isWalkingSoundPlaying)
                {
                    AudioManager.Instance?.PlayBug($"{bugName}_Walk", true);
                    isWalkingSoundPlaying = true;
                }
            }
        // if (bugName != "Player")
        // {
        //     if (isWalking)
        //     {
        //         if (!isWalkingSoundPlaying)
        //         {
        //             AudioManager.Instance?.PlayBug($"{bugName}_Walk", true);
        //             isWalkingSoundPlaying = true;
        //         }
        //     }
        //     else
        //     {
        //         if (isWalkingSoundPlaying)
        //         {
        //             AudioManager.Instance?.StopBug();
        //             isWalkingSoundPlaying = false;
        //         }
        //     }
        // }
    }
}