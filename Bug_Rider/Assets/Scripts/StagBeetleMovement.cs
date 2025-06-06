using System.Collections;
using UnityEngine;

public class StagBeetleMovement : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float rotationSpeed = 120f;
    public float flightHeight = 3f;
    public float flightDuration = 4f;

    private Animator animator;
    private Rigidbody rb;

    private Vector3 input;
    private bool isFlying = false;
    private bool isStunned = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (isStunned) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        input = new Vector3(h, 0, v);

        animator.SetBool("is_walking", Mathf.Abs(v) > 0.01f);

        if (Input.GetKeyDown(KeyCode.Space) && !isFlying)
        {
            StartCoroutine(HandleFlight());
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SetStunned(true);
        }
    }

    void FixedUpdate()
    {
        if (isStunned || isFlying) return;

        float h = input.x;
        float v = input.z;

        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 forward = rb.rotation * Vector3.forward;
            Vector3 move = forward * v * walkSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
        }

        if (Mathf.Abs(h) > 0.01f)
        {
            float turn = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turn, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }

    IEnumerator HandleFlight()
    {
        isFlying = true;
        animator.SetBool("is_flying", true);
        rb.useGravity = false;

        float targetY = flightHeight;
        while (rb.position.y < targetY - 0.05f)
        {
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, targetY, 2f * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(flightDuration);

        while (rb.position.y > 0.5f)
        {
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, 0f, 2f * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        rb.useGravity = true;
        isFlying = false;
        animator.SetBool("is_flying", false);
    }

    public void SetStunned(bool value)
    {
        isStunned = value;
        animator.SetBool("is_stunned", value);
        if (value)
        {
            input = Vector3.zero;
            animator.SetBool("is_walking", false);
            animator.SetBool("is_flying", false);
        }
    }
}
