using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public Transform target;                             // 따라갈 대상 (벌레 or 플레이어)
    public Vector3 offset = new Vector3(0f, 2f, -6f);     // 로컬 뒤 + 위
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        // 등 뒤 위치 계산: target의 회전에 따라 offset 적용
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // 부드럽게 따라가기
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 항상 target이 바라보는 방향을 보도록
        Vector3 lookAtPoint = target.position + target.forward * 5f;
        transform.LookAt(lookAtPoint);
    }
}
