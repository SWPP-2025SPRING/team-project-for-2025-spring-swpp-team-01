using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAlcoholPenalty : MonoBehaviour
{
    public float stunRadius = 0.5f;           // 감지 반경
    public float stunDuration = 5f;           // 스턴 시간
    private bool isStunned = false;

    private PlayerMovement movementScript;

    void Start()
    {
        movementScript = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!isStunned)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, stunRadius);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Flame")) // 태그로 판별
                {
                    StartCoroutine(Stun());
                    Debug.Log("불꽃 충돌!");
                    break;
                }
            }
        }
    }

    private IEnumerator Stun()
    {
        isStunned = true;


        if (movementScript.isMounted)
        {
            Transform bug = movementScript.mountedBug;
            movementScript.Unmount();
            Destroy(bug.gameObject);
        }

        movementScript.enabled = false;

        yield return new WaitForSeconds(stunDuration);

        if (movementScript != null)
            movementScript.enabled = true;

        isStunned = false;
    }
}
