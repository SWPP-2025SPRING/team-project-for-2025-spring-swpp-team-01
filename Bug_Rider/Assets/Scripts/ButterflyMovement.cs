using System.Collections;
using UnityEngine;

public class ButterflyMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 150f;
    public float flightHeight = 2.5f;
    public float flightDuration = 4f;

    private Rigidbody rb;
    private Animator animator;

    private bool isMounted = false;
    private bool isFlying = false;
    private bool canFly = true;

    private Vector3 cachedInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (!isMounted) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        cachedInput = new Vector3(h, 0, v);

        animator.SetBool("is_walking", Mathf.Abs(v) > 0.01f);

        if (Input.GetKeyDown(KeyCode.Space) && canFly)
        {
            StartCoroutine(HandleFlight());
        }
    }

    void FixedUpdate()
    {
        if (!isMounted || isFlying) return;

        float h = cachedInput.x;
        float v = cachedInput.z;

        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 forward = rb.rotation * Vector3.forward;
            Vector3 next = rb.position + forward * v * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(next);
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
        canFly = false;

        animator.SetTrigger("is_ascend");
        rb.useGravity = false;

        float groundY = rb.position.y;
        while (rb.position.y < flightHeight - 0.05f)
        {
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, flightHeight, 2.5f * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        animator.SetBool("is_flying", true);
        yield return new WaitForSeconds(flightDuration);

        animator.SetTrigger("is_drop");
        animator.SetBool("is_flying", false);

        while (rb.position.y > groundY)
        {
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, groundY, 2f * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        rb.useGravity = true;
        isFlying = false;

        yield return new WaitForSeconds(5f);
        canFly = true;
    }

    public void SetMounted(bool mounted)
    {
        isMounted = mounted;
        if (!mounted)
        {
            animator.SetBool("is_walking", false);
            animator.SetBool("is_flying", false);
            rb.velocity = Vector3.zero;
        }
    }
}
