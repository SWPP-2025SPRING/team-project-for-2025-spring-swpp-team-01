using UnityEngine;
using System.Collections;

public class MothMovement : RideableBugBase
{
    public float retreatSpeed = 3f;
    public float retreatDuration = 5f;
    public LayerMask obstacleMask;

    private Coroutine retreatCoroutine;
    private bool isRetreating = false;

    protected override void Awake()
    {
        base.Awake();
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        if (isRetreating)
        {
            Vector3 retreatDirection = (-transform.forward + Vector3.up * 0.3f).normalized;
            rb.velocity = retreatDirection * retreatSpeed;
        }
    }

    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);

        if (!mounted)
        {
            isRetreating = false;
            animator?.SetBool("is_flying", false);
            AudioManager.Instance?.StopBug();  // ✅ 떨어지고 나면 소리 끔
            AudioManager.Instance?.StopObstacle(); // Turn off _Enter sound

            if (retreatCoroutine != null)
                StopCoroutine(retreatCoroutine);
        }
        else
        {
            animator?.SetBool("is_flying", true);
            AudioManager.Instance?.PlayBug("Moth_Fly");
            retreatCoroutine = StartCoroutine(RetreatAndAutoUnmount());
        }
    }

    private IEnumerator RetreatAndAutoUnmount()
    {
        isRetreating = true;
        float timer = 0f;

        while (timer < retreatDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("자동 하차 완료");
        isRetreating = false;
        GetComponentInChildren<PlayerMovement>()?.Unmount();
        AudioManager.Instance?.StopBug();  // ✅ 자동 하차 후에도 소리 끔
    }

    protected override void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;

        if (col.gameObject.CompareTag("Obstacle"))
        {
            if (isRetreating)
            {
                Debug.Log("Retreat 중 충돌 → 즉시 하차");
                isRetreating = false;
                if (retreatCoroutine != null)
                    StopCoroutine(retreatCoroutine);

                GetComponentInChildren<PlayerMovement>()?.ForceFallFromBug();
                SetMounted(false);
                Destroy(transform.root.gameObject, 2f);
            }
            else
            {
                base.OnCollisionEnter(col);
            }
        }
    }
}