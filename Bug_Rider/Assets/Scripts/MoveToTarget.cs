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
            animator.SetBool("is_walking", true);  // �̵� ���� �� �ȱ� �ִϸ��̼�
        }
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            if (animator != null)
            {
                animator.SetBool("is_walking", false);  // ����
            }

            Destroy(gameObject);  // ���� �� ����
        }
    }
}
