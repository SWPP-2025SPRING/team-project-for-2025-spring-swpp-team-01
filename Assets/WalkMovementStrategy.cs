using UnityEngine;

public class WalkMovementStrategy : IBugMovementStrategy
{
    public void HandleMovement(Rigidbody rb, Animator animator, LayerMask obstacleMask, float moveSpeed, float rotationSpeed, float obstacleCheckDist)
    {
        // 입력 받기 (WASD → Horizontal, Vertical)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 회전 처리 (좌우 입력이 있는 경우)
        // Rotate left/right if horizontal input is non-zero
        if (Mathf.Abs(v) > 0.01f && Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // 전진 입력 처리
        // Move forward if vertical input exists
        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 forward = rb.rotation * Vector3.forward;

            // 장애물 감지
            // Ray/Sphere check to avoid walking into obstacles
            Vector3 rayOrigin = rb.position + Vector3.up * 0.3f;
            if (!Physics.SphereCast(rayOrigin, 0.04f, forward, out _, obstacleCheckDist, obstacleMask))
            {
                // 장애물이 없으면 이동
                // Move forward only if no obstacle ahead
                Vector3 next = rb.position + forward * v * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(next);
            }

            // 걷는 애니메이션 활성화
            animator?.SetBool("is_walking", true);
        }
        else
        {
            // 입력 없을 땐 걷기 애니메이션 비활성화
            animator?.SetBool("is_walking", false);
        }
    }
}
