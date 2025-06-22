using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PillbugMovement : RideableBugBase
{
    public float baseSpeed = 2f;
    public float accelerationRate = 2f;
    public float rotationSpeed = 180f;
    public LayerMask obstacleMask;
    private float currentSpeed;
    protected override void Awake()
    {
        base.Awake();
        currentSpeed = baseSpeed;
    }
    void FixedUpdate()
    {
        base.RotateMesh();
        if (!isMounted) return;
        // 회전
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
        // 전진
        Vector3 forward = rb.rotation * Vector3.forward;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        if (isGrounded)
        {
            currentSpeed += accelerationRate * Time.fixedDeltaTime;
            rb.AddForce(forward * currentSpeed, ForceMode.Acceleration);
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }
        animator.SetBool("is_rolling", true);
    }
    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);
        currentSpeed = baseSpeed;
        if (!mounted)
        {
            SphereCollider col = GetComponent<SphereCollider>();
            if (col != null)
            {
                Vector3 center = col.center;
                center.y += col.radius;
                col.center = center;
            }
            animator.SetBool("is_rolling", false);
            AudioManager.Instance?.StopBug();  // 롤 소리 정지
        }
        else
        {
            SphereCollider col = GetComponent<SphereCollider>();
            if (col != null)
            {
                Vector3 center = col.center;
                center.y -= col.radius;
                col.center = center;
            }
            animator.SetBool("is_rolling", true);
            AudioManager.Instance?.PlayBug("Pillbug_Roll", true);  // 롤 소리 재생
        }
    }
    protected override void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Wall")) return;
        AudioManager.Instance?.PlayBug("Pillbug_Stun");  // Pillbug 전용 사운드
    }
}