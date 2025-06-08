using UnityEngine;
using TMPro;

public class PartSwitcher : MonoBehaviour
{
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
    private int currentStep = 0;

    void Start()
    {
        startTime = Time.time;
        UpdateTimerUI(0f);
    }

    void Update()
    {
        float elapsed = Time.time - startTime;
        UpdateTimerUI(elapsed);

        if (currentStep == 0 && elapsed >= interval * 1) { Step1_Arms(); currentStep++; }
        else if (currentStep == 1 && elapsed >= interval * 2) { Step2_Legs(); currentStep++; }
        else if (currentStep == 2 && elapsed >= interval * 3) { Step3_Chest(); currentStep++; }
        else if (currentStep == 3 && elapsed >= interval * 4) { Step4_Head(); currentStep++; }
    }

    void UpdateTimerUI(float elapsed)
    {
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void Step1_Arms()
    {
        SetActiveGroup(humanArms, false);
        SetActiveGroup(antArms, true);
    }

    void Step2_Legs()
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

    void Step4_Head()
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
