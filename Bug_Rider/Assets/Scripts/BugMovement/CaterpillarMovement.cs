using UnityEngine;
using TMPro;
using System.Collections;

public class CaterpillarMovement : RideableBugBase
{
    public GameObject mothPrefab;
    public GameObject butterflyPrefab;
    public GameObject beePrefab;

    public LayerMask obstacleMask;
    public float transformTime = 5f;

    private WalkMovementStrategy walkStrategy;
    private Coroutine transformRoutine;
    private PlayerMovement mountedPlayer;

    protected override void Awake()
    {
        base.Awake();

        meshRot = Quaternion.Euler(74f, -81f, -86f);
        mesh = transform.Find("Armature");

        walkStrategy = new WalkMovementStrategy(
            rb, animator, obstacleMask,
            acceleration, maxSpeed,
            angularAcceleration, maxAngularSpeed,
            obstacleCheckDist,
            "Caterpillar",
            false,  // useAcceleration
            false   // useAngularAcceleration
        );

        Random.InitState(System.DateTime.Now.Millisecond);
    }

    void FixedUpdate()
    {
        base.RotateMesh();
        if (!isMounted) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        walkStrategy.HandleMovement(h, v);
    }

    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);

        if (!mounted)
        {
            // 사운드 정리 추가
            StopCaterpillarSounds();
            
            animator?.SetBool("is_walking", false);
            if (transformRoutine != null)
                StopCoroutine(transformRoutine);
        }
        else
        {
            mountedPlayer = GetComponentInChildren<PlayerMovement>();
            transformRoutine = StartCoroutine(DelayedTransformation());
        }
    }

    private IEnumerator DelayedTransformation()
    {
        float timer = transformTime;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        TransformIntoRandomBug();
    }

    private void TransformIntoRandomBug()
    {
        // 변신 전에 캐터필러 사운드 정리
        StopCaterpillarSounds();
        
        GameObject[] candidates = { mothPrefab, butterflyPrefab, beePrefab };
        int idx = Random.Range(0, candidates.Length);
        GameObject chosen = candidates[idx];

        // 프리팹에 따라 사운드 매핑
        string enterSoundKey = "";
        if (chosen == mothPrefab)
            AudioManager.Instance?.PlayBugBGM("Moth_Enter");
        else if (chosen == butterflyPrefab)
            AudioManager.Instance?.PlayBugBGM("Butterfly_Enter");
        else if (chosen == beePrefab)
            AudioManager.Instance?.PlayBugBGM("Bee_Enter", true);

        GameObject newBug = Instantiate(chosen, transform.position, transform.rotation);
        if (mountedPlayer != null)
        {
            mountedPlayer.Mount(newBug.transform);
        }

        Destroy(gameObject);
    }
    
    // 캐터필러 사운드 정리 함수 추가
    private void StopCaterpillarSounds()
    {
        // WalkStrategy 사운드 상태 초기화
        if (walkStrategy != null)
        {
            walkStrategy.isWalkingSoundPlaying = false;
        }
        
        // AudioManager에서 버그 사운드 정지
        AudioManager.Instance?.StopBug();
        
        Debug.Log("[CaterpillarMovement] Stopped all caterpillar sounds");
    }
    
    // 오브젝트 파괴 시에도 사운드 정리
    void OnDestroy()
    {
        StopCaterpillarSounds();
    }
}