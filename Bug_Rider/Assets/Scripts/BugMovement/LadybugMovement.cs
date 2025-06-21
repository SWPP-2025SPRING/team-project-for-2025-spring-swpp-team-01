using UnityEngine;
using TMPro;
using System.Collections;

public class LadybugMovement : RideableBugBase
{
    public float flyMaxHeight = 50f;
    public float flyTimeLimit = 20f;
    public float skillCooldown = 7f;
    public LayerMask obstacleMask;
    private bool canFly = true;


    private WalkMovementStrategy walkStrategy;
    private FlyMovementStrategy flyStrategy;

    [Header("Fly UI Sprites")]
    public Sprite flyReadySprite;
    public Sprite flyActiveSprite;
    public Sprite flyCooldownSprite;

    protected override void Awake()
    {
        base.Awake();

        walkStrategy = new WalkMovementStrategy(
            rb, animator, obstacleMask,
            acceleration, maxSpeed,
            angularAcceleration, maxAngularSpeed,
            obstacleCheckDist,
            "Ladybug"
        );

        flyStrategy = new FlyMovementStrategy(rb, animator, maxSpeed, maxAngularSpeed, flyMaxHeight, "Ladybug");
    }

    void Update()
    {
        if (!isMounted) return;
        if (canFly && Input.GetKeyDown(KeyCode.Space))
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
        else
            walkStrategy.HandleMovement(h, v);
    }

    IEnumerator StartFlight()
    {
        if (!CanUseSkill())
        {
            Debug.Log("Skill is not available (still active or cooling down).");
            yield break;
        }
        flyStrategy.SetFlying(true);

        yield return SkillWithCooldown(
            flyTimeLimit,
            skillCooldown,
            () => UIManager.Instance.ShowSkillActive("fly"),
            () =>
            {
                flyStrategy.SetFlying(false);
                UIManager.Instance.ShowSkillCooldown("fly");
            },
            "fly"
        );

        // UIManager.Instance.HideAllSkillUI();
    }


    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);

        if (mounted)
            UIManager.Instance.OnMountSkillUI("fly");


        else
        {
            flyStrategy.SetFlying(false);
            animator?.SetBool("is_walking", false);
            UIManager.Instance.HideAllSkillUI();
        }
    }
}
