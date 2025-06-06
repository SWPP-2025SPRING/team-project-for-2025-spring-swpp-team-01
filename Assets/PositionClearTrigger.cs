using UnityEngine;
using UnityEngine.SceneManagement;

public class PositionClearTrigger : MonoBehaviour
{
    public Transform player;
    public Transform goalPoint;
    public float range = 1f;
    


    public TimerManager timerManager; // TimerManager 연결

    void Update()
    {
        if (Vector3.Distance(player.position, goalPoint.position) < range)
        {
            float time = timerManager.GetElapsedTime();
            int score = Mathf.Max(0, Mathf.FloorToInt(300 - time)); // 예: 빠를수록 높은 점수

            float clearTime = timerManager.GetElapsedTime();
            ScoreManager.SaveTime(clearTime);  // 새 함수로 교체

            PlayerPrefs.SetString("CurrentStage", "Stage1");
            SceneManager.LoadScene("StageClear");
        }
    }
}
