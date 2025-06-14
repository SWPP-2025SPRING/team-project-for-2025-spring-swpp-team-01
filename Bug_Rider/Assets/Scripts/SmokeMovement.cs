using UnityEngine;
public class SmokeMovement : MonoBehaviour
{
    [Header("추적 대상")]
    public Transform target;           // 플레이어
    [Header("자동 추적 설정")]
    public bool autoFollow = true;     // true면 Update()로 계속 추적
    [Header("이동 파라미터")]
    public float moveSpeed = 2f;
    public float heightOffset = 1.24f;
    public float behind = 5f;
    /* ---------- 외부에서 호출할 스냅 함수 ---------- */
    public void SnapBehindTarget()
    {
        if (target == null) return;
        // 정확히 플레이어 뒤 + 높이 오프셋
        Vector3 snapPos = target.position
                        + Vector3.up * heightOffset
                        - target.forward * behind;
        transform.position = snapPos;
        // 플레이어를 바라보도록 회전
        Vector3 dir = (target.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
    /* ---------- 자동 추적 ---------- */
    void Update()
    {
        if (!autoFollow || target == null) return;
        Vector3 targetPos = target.position
                          + Vector3.up * heightOffset
                          - target.forward * behind;
        // 회전
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  targetRot, 10f * Time.deltaTime);
        }
        // 이동
        transform.position = Vector3.MoveTowards(transform.position,
                                                 targetPos,
                                                 moveSpeed * Time.deltaTime);
    }
}