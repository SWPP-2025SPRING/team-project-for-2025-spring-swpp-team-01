using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Skill UI Panels")]
    public GameObject skillAvailablePanel;
    public GameObject skillActivePanel;
    public GameObject skillCooldownPanel;

    [Header("Skill UI Images")]
    public Image availableImage;
    public Image activeImage;
    public Image cooldownImage;

    [Header("Skill UI Texts")]
    public TMP_Text activeTimerText;
    public TMP_Text cooldownTimerText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowSkillAvailable(Sprite sprite)
    {
        availableImage.sprite = sprite;
        skillAvailablePanel.SetActive(true);
        skillActivePanel.SetActive(false);
        skillCooldownPanel.SetActive(false);
    }

    public void ShowSkillActive(Sprite sprite)
    {
        activeImage.sprite = sprite;
        skillAvailablePanel.SetActive(false);
        skillActivePanel.SetActive(true);
        skillCooldownPanel.SetActive(false);
    }

    public void UpdateSkillActiveTime(float time)
    {
        activeTimerText.text = $"{time:F1}";
    }

    public void ShowSkillCooldown(Sprite sprite)
    {
        cooldownImage.sprite = sprite;
        skillAvailablePanel.SetActive(false);
        skillActivePanel.SetActive(false);
        skillCooldownPanel.SetActive(true);
    }

    public void UpdateSkillCooldownTime(float time)
    {
        cooldownTimerText.text = $"{time:F1}";
    }

    public void HideAllSkillUI()
    {
        skillAvailablePanel.SetActive(false);
        skillActivePanel.SetActive(false);
        skillCooldownPanel.SetActive(false);
    }
}
