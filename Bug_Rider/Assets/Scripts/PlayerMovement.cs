using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    public bool isMounted = false;
    public Transform mountedBug;
    private bool isFalling = false;

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
        move.y = 0f;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);

            controller.Move(move.normalized * speed * Time.deltaTime);
        }

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

        transform.rotation = bug.rotation;

        // BugMovement or BugFlightMovement 모두 지원
        bug.GetComponent<BugMovement>()?.SetMounted(true);
        bug.GetComponent<BugFlightMovement>()?.SetMounted(true);
    }

    public void Unmount()
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

    void Fall()
    {
        isFalling = true;
        controller.enabled = false;
        StartCoroutine(SimulateFall());
        Debug.Log("장애물과 충돌하여 추락");
    }

    public void ForceFallFromBug()
    {
        if (isMounted)
        {
            // 탑승 해제 먼저 수행
            Unmount();
        }
        Fall(); // 기존 추락 로직 실행
    }

    IEnumerator SimulateFall()
    {
        float fallDuration = 2f;
        float fallSpeed = -10f;
        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            velocity = new Vector3(0, fallSpeed, 0);
            controller.Move(velocity * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        RecoverFromFall();
    }

    void RecoverFromFall()
    {
        isFalling = false;
        velocity = Vector3.zero;
        controller.enabled = true;
        Debug.Log("추락 후 복구됨");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle") && !isFalling)
        {
            Fall();
        }
    }
}

