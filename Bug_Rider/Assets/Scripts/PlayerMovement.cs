using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isMounted = false;
    private Transform mountedBug;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isMounted)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Unmount();
            }
            return;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryCallBug();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryMount();
        }

        
    }
    
    void TryCallBug()
    {
        float searchRadius = 20f;
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);
        Transform closestBug = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Bug"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestBug = hit.transform;
                }
            }
        }

        if (closestBug != null)
        {
            if (closestBug.TryGetComponent<BugMovement>(out var bugMover))
            {
                bugMover.ApproachTo(transform.position);
            }
            else if (closestBug.TryGetComponent<BugFlightMovement>(out var bugFlier))
            {
                bugFlier.ApproachTo(transform.position);
            }
        }

    }

    void TryMount()
    {
        float checkRadius = 2f;
        Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Bug"))
            {
                Mount(hit.transform);
                break;
            }
        }
    }

    void Mount(Transform bug)
    {
        isMounted = true;
        mountedBug = bug;

        controller.enabled = false;
        transform.SetParent(bug);
        transform.localPosition = new Vector3(0, 1.2f, 0); 

        // BugMovement or BugFlightMovement 모두 지원
        bug.GetComponent<BugMovement>()?.SetMounted(true);
        bug.GetComponent<BugFlightMovement>()?.SetMounted(true);
    }

    void Unmount()
    {
        if (!isMounted || mountedBug == null) return;

        isMounted = false;

        Vector3 dismountPos = mountedBug.position + mountedBug.right * 1.5f;
        transform.SetParent(null);
        transform.position = dismountPos;

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        controller.enabled = true;

        mountedBug.GetComponent<BugMovement>()?.SetMounted(false);
        mountedBug.GetComponent<BugFlightMovement>()?.SetMounted(false);
        mountedBug = null;
    }
}
