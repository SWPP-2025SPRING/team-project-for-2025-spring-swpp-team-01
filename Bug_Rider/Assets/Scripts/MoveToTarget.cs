using UnityEngine;

public class MoveToTarget : MonoBehaviour
{
    private Vector3 target;
    private float speed;
    private Animator animator;

    public void Initialize(Vector3 targetPosition, float moveSpeed)
    {
        target = targetPosition;
        speed = moveSpeed;

        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("is_walking", true);  // 이동 시작 시 걷기 애니메이션
        }
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            if (animator != null)
            {
                animator.SetBool("is_walking", false);  // 정지
            }

            Destroy(gameObject);  // 도착 후 제거
        }
    }
}
