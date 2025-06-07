using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeMovement : MonoBehaviour
{
    public Transform target;         // 쫓을 대상 (플레이어)
    public float moveSpeed = 2f;     // 이동 속도
    public float heightOffset = 1.24f;

    void Update()
    {
        if (target == null) return;

        // 목표 위치 (높이 포함)
        Vector3 targetPosition = target.position + new Vector3(0, heightOffset, 6);

        // 현재 위치 기준으로 목표와의 방향 벡터 계산 (Y축만 사용)
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;  // Y축 무시 → XZ 평면 방향만 사용

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어가 죽는 처리
            Debug.Log("연기에 닿음!");  // 또는 Destroy(other.gameObject);
        }
    }
}
