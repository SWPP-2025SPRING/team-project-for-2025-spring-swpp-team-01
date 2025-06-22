using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("컷신 이미지 관련")]
    public Sprite[] cutsceneSprites;
    public Image cutsceneImage;
    public GameObject cutscenePanel;
    public Button skipButton;
    public Button nextButton;

    [Header("컷신 이후 Stage 이미지 패널")]
    public GameObject stageImagePanel;
    public Button playButton;

    [Header("플레이 시 이동할 다음 씬 이름")]
    public string nextSceneName = "Stage1Scene";

    private int index = 0;

    void Start()
    {
        if (cutsceneSprites.Length == 0 || cutsceneImage == null)
        {
            Debug.LogError("컷신 이미지 설정이 빠졌습니다.");
            return;
        }

        skipButton.onClick.AddListener(SkipToStageImage);
        nextButton.onClick.AddListener(NextCutscene);
        playButton.onClick.AddListener(LoadNextScene);

        stageImagePanel.SetActive(false);
        cutscenePanel.SetActive(true);

        index = 0;
        UpdateCutscene();
    }

    void UpdateCutscene()
    {
        cutsceneImage.sprite = cutsceneSprites[index];
        skipButton.gameObject.SetActive(index == 0); // 첫 컷신에만 스킵 버튼
    }

    void NextCutscene()
    {
        index++;
        if (index >= cutsceneSprites.Length)
        {
            ShowStageImage();
        }
        else
        {
            UpdateCutscene();
        }
    }

    void SkipToStageImage()
    {
        ShowStageImage();
    }

    void ShowStageImage()
    {
        cutscenePanel.SetActive(false);
        stageImagePanel.SetActive(true);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
