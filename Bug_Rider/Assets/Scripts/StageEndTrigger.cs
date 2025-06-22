using UnityEngine;

public class StageEndTrigger : MonoBehaviour
{
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("⚠️ Trigger Entered by: " + other.name);

        if (triggered)
        {
            Debug.Log("⚠️ Already triggered. Skipping.");
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("✅ Player detected. Triggering Stage Clear!");

            triggered = true;
            float playTime = PartSwitcher.Instance.TotalPlayTime;

            // 🔧 비활성화된 오브젝트도 찾을 수 있도록 수정
            StageClearHandler handler = FindObjectOfType<StageClearHandler>(true); // true 추가
            if (handler != null)
            {
                Debug.Log("✅ StageClearHandler found. Showing UI.");
                handler.ShowClearUI(playTime);
            }
            else
            {
                Debug.LogError("❌ StageClearHandler NOT found!");
            }
        }
        else
        {
            Debug.Log("⚠️ Entered object is NOT Player. Tag = " + other.tag);
        }
    }
}
