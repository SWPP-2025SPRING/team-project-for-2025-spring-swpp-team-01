using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class StageClearHandler : MonoBehaviour
{
    [Header("UI References")]
    public GameObject clearPanel;
    public TMP_Text clearTimeText;
    public Transform bugIconContainer; // 탑승한 벌레 아이콘들을 놓을 위치
    public GameObject bugIconPrefab;   // 단순 Image 오브젝트 프리팹
    public Image stageImage;           // 벌레화 단계에 따른 이미지
    public Button retryButton;
    public Button nextStageButton;

    [Header("Stage Info")]
    public string nextSceneName = "MiddleScene";
    public string retrySceneName = "Stage1";

    void Start()
    {
        clearPanel.SetActive(false); // 처음에는 안 보이게
        retryButton.onClick.AddListener(OnClickRetry);
        nextStageButton.onClick.AddListener(OnClickNextStage);
    }

    public void ShowClearUI(float playTime)
{
    Debug.Log("🎯 ShowClearUI called with playTime: " + playTime);
    
    clearPanel.SetActive(true);
    Debug.Log("🎯 ClearPanel activated");
    
    int minutes = Mathf.FloorToInt(playTime / 60f);
    int seconds = Mathf.FloorToInt(playTime % 60f);
    clearTimeText.text = $"{minutes:D2}:{seconds:D2}";
    Debug.Log("🎯 Time text set: " + clearTimeText.text);
    
    // 🔧 강제 UI 업데이트
    clearTimeText.ForceMeshUpdate();
    Canvas.ForceUpdateCanvases();
    
    int stage = PartSwitcher.Instance.currentStep;
    PlayerPrefs.SetInt("BugStep", stage); 
    Debug.Log("🎯 Current bug stage: " + stage);
    
    Sprite stageSprite = Resources.Load<Sprite>($"BugStage/stage{stage}");
    Debug.Log("🎯 Stage sprite loaded: " + (stageSprite != null ? "Success" : "Failed"));
    if (stageSprite != null) 
    {
        stageImage.sprite = stageSprite;
        Debug.Log("🎯 Stage image sprite set");
    }
    
    foreach (Transform child in bugIconContainer) Destroy(child.gameObject);
    
    List<string> bugs = BugRideTracker.Instance.GetRiddenBugNames();
    Debug.Log("🎯 Ridden bugs count: " + bugs.Count);
    foreach (string bugName in bugs)
    {
        Debug.Log("🎯 Processing bug: " + bugName);
        Sprite bugSprite = Resources.Load<Sprite>($"BugIcons/{bugName}");
        Debug.Log("🎯 Bug sprite loaded: " + (bugSprite != null ? "Success" : "Failed"));
        if (bugSprite != null)
        {
            GameObject icon = Instantiate(bugIconPrefab, bugIconContainer);
            icon.GetComponent<Image>().sprite = bugSprite;
            Debug.Log("🎯 Bug icon created for: " + bugName);
        }
    }
}
    public void OnClickRetry()
    {
        PlayerPrefs.SetInt("BugStep", 0);
        SceneManager.LoadScene(retrySceneName);
    }

    public void OnClickNextStage()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        int step = PartSwitcher.Instance.currentStep;
        PlayerPrefs.SetInt("EndingStep", step);

        if (currentScene == "Stage2_MusicRoom") 
        {
            SceneManager.LoadScene("EndingScene");
        }
        else  
        {
            SceneManager.LoadScene(nextSceneName); 
        }
    }

}
