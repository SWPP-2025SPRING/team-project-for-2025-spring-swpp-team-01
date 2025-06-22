using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingSceneController : MonoBehaviour
{
    public Image cutImage;        // 보여줄 컷 이미지
    public Button nextButton;

    private Sprite[] endingCuts;  // 선택된 컷 이미지들
    private int currentIndex = 0;

    void Start()
    {
        int stage = Mathf.Clamp(PlayerPrefs.GetInt("EndingStep", 1), 1, 4);
        endingCuts = LoadCutImages(stage);

        if (endingCuts.Length > 0)
            cutImage.sprite = endingCuts[0];

        nextButton.onClick.AddListener(ShowNextCut);
    }

    void ShowNextCut()
    {
        currentIndex++;
        if (currentIndex >= endingCuts.Length)
        {
            SceneManager.LoadScene("StartScene"); // 종료 후 메인화면으로
        }
        else
        {
            cutImage.sprite = endingCuts[currentIndex];
        }
    }

    Sprite[] LoadCutImages(int stage)
    {
        Sprite cut1 = Resources.Load<Sprite>($"EndingCut/ending{stage}_1");
        Sprite cut2 = Resources.Load<Sprite>($"EndingCut/ending{stage}_2");
        return new Sprite[] { cut1, cut2 };
    }
}
