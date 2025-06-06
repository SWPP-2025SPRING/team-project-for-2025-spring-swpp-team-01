using UnityEngine;

public class KillZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var rm = other.GetComponent<RespawnManager>();
            if (rm != null)
            {
                Debug.Log("▶ KillZone hit – Respawn!");
                rm.RespawnToNearest();
            }
        }
    }
}
