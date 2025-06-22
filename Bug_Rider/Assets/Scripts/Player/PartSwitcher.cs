using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; 

public class PartSwitcher : MonoBehaviour
{
    public static PartSwitcher Instance {get; private set;} // 6.22추가 

    [Header("설정")]
    public float interval = 30f;  // 30초 간격, Inspector에서 조정 가능
    public TMP_Text timerText;
    [Header("Human 파츠")]
    public GameObject[] humanArms;
    public GameObject[] humanLegs;
    public GameObject[] humanChest;
    public GameObject[] humanHead;
    [Header("Ant 파츠")]
    public GameObject[] antArms;
    public GameObject[] antLegs;
    public GameObject[] antChest;
    public GameObject[] antHead;
    [Header("기타 파츠")]
    public GameObject[] humanPants;
    public GameObject[] humanShoes;
    public GameObject[] humanHeap;
    public GameObject[] antHeap;
    private float startTime;
    public int currentStep = 0;

    void Awake() //추가 
    { // 추가 
        if(Instance == null) Instance = this; // 추가
        else{Destroy(gameObject); return;} // 추가
    } // 추가 


    void Start()
    {
        startTime = Time.time;
        UpdateTimerUI(0f);

        string sceneName = SceneManager.GetActiveScene().name; //추가
        if (sceneName == "Stage1_ScienceRoom")  //추가
        {
            PlayerPrefs.SetInt("BugStep", 0);//추가
            currentStep = 0;//추가
        }
        else  //추가
        {
            currentStep = PlayerPrefs.GetInt("BugStep", 0);//추가
            ApplyCurrentStep();//추가
        }
    }

    void ApplyCurrentStep()// 추가 
    {
        if (currentStep >= 1) Step1_Head();// 추가 
        if (currentStep >= 2) Step2_Arms();// 추가 
        if (currentStep >= 3) Step3_Chest();// 추가 
        if (currentStep >= 4) Step4_Legs();// 추가 
    }


    void Update()
    {
        float elapsed = Time.time - startTime;
        UpdateTimerUI(elapsed);
        if (currentStep == 0 && elapsed >= interval * 1) { Step1_Head(); currentStep++; }
        else if (currentStep == 1 && elapsed >= interval * 2) { Step2_Arms(); currentStep++; }
        else if (currentStep == 2 && elapsed >= interval * 3) { Step3_Chest(); currentStep++; }
        else if (currentStep == 3 && elapsed >= interval * 4) { Step4_Legs(); currentStep++; }
    }

    public float TotalPlayTime => Time.time - startTime; // 추가 

    public void ResetTimer()   // Retry 눌렀을 때 호출
    { // 추가 
        startTime = Time.time; // 추가 
    } // 추가 
    

    void UpdateTimerUI(float elapsed)
    {
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
    void Step2_Arms()
    {
        SetActiveGroup(humanArms, false);
        SetActiveGroup(antArms, true);
    }
    void Step4_Legs()
    {
        SetActiveGroup(humanLegs, false);
        SetActiveGroup(humanPants, false);
        SetActiveGroup(humanShoes, false);
        SetActiveGroup(humanHeap, false);
        SetActiveGroup(antLegs, true);
        SetActiveGroup(antHeap, true);
    }
    void Step3_Chest()
    {
        SetActiveGroup(humanChest, false);
        SetActiveGroup(antChest, true);
    }
    void Step1_Head()
    {
        SetActiveGroup(humanHead, false);
        SetActiveGroup(antHead, true);
    }
    void SetActiveGroup(GameObject[] group, bool active)
    {
        foreach (var go in group)
        {
            if (go != null)
                go.SetActive(active);
        }
    }
}