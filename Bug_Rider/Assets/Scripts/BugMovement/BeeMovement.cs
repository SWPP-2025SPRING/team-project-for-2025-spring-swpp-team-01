using UnityEngine;
using TMPro;
using System.Collections;

public class BeeMovement : RideableBugBase
{
    public float flyMaxHeight = 50f;
    public float flyTimeLimit = 5f;
    public float skillCooldown = 7f;
    public LayerMask obstacleMask;

    private FlyMovementStrategy flyStrategy;

    protected override void Awake()
    {
        base.Awake();

        flyStrategy = new FlyMovementStrategy(
            rb, animator,
            maxSpeed, maxAngularSpeed,
            flyMaxHeight
        );
    }

    void Update()
    {
        if (!isMounted) return;

        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(StartFlight());
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool s = Input.GetKey(KeyCode.Space);

        if (flyStrategy.IsFlying)
            flyStrategy.HandleMovement(h, v, s);
    }

    IEnumerator StartFlight()
    {
        flyStrategy.SetFlying(true);

        yield return SkillWithCooldown(
            flyTimeLimit,
            skillCooldown,
            null,
            () => flyStrategy.SetFlying(false)
        );
    }

    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);

        if (!mounted)
        {
            flyStrategy.SetFlying(false);
            animator?.SetBool("is_walking", false);
        }
    }

    protected override void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;

        if (col.gameObject.CompareTag("Obstacle"))
        {
            Destroy(col.gameObject);
        }
    }
}
