using UnityEngine;

public class WalkMovementStrategy : IBugMovementStrategy
{
    public void HandleMovement(Rigidbody rb, Animator animator, LayerMask obstacleMask, float moveSpeed, float rotationSpeed, float obstacleCheckDist)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Mathf.Abs(v) > 0.01f && Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 forward = rb.rotation * Vector3.forward;
            Vector3 rayOrigin = rb.position + Vector3.up * 0.3f;

            if (!Physics.SphereCast(rayOrigin, 0.4f, forward, out _, obstacleCheckDist, obstacleMask))
            {
                Vector3 next = rb.position + forward * v * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(next);
            }

            animator?.SetBool("is_walking", true);
        }
        else
        {
            animator?.SetBool("is_walking", false);
        }
    }
}
