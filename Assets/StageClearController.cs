using UnityEngine;
using UnityEngine.SceneManagement;

// 스테이지 클리어 후 다음 스테이지로 넘어가거나, 현재 스테이지를 다시 시작할 때 사용
// Used to proceed to the next stage or retry the current stage after clearing
public class StageClearController : MonoBehaviour
{
    [Tooltip("다음 스테이지의 씬 이름")]
    // 다음으로 넘어갈 스테이지의 씬 이름
    // Name of the next stage to load
    public string nextStageName;

    // 다음 스테이지 씬을 로드한다
    // Loads the next stage scene
    public void LoadNextStage()
    {
        SceneManager.LoadScene(nextStageName);
    }

    // 현재 스테이지를 다시 시작한다
    // Reloads the current stage using PlayerPrefs
    public void RetryStage()
    {
        string currentStage = PlayerPrefs.GetString("CurrentStage", "Stage1");
        SceneManager.LoadScene(currentStage);
    }
}
