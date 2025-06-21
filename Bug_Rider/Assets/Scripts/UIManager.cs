using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [Header("Skill UI Panels")]

    public GameObject activePanel;
    public GameObject cooldownBasePanel;
    public GameObject cooldownActivePanel;


    [Header("Skill UI Images")]
    public Image activeImage;
    public Image cooldownBaseImage;
    public Image cooldownActiveImage;


    [Header("Skill Sprites - Active")]
    public Sprite dashActiveSprite;
    public Sprite jumpActiveSprite;
    public Sprite flyActiveSprite;

    [Header("Skill Cooldown Sprites - Base (Always On)")]
    public Sprite dashCooldownBaseSprite;
    public Sprite jumpCooldownBaseSprite;
    public Sprite flyCooldownBaseSprite;

    [Header("Skill Cooldown Sprites - Active (During Cooldown)")]
    public Sprite dashCooldownActiveSprite;
    public Sprite jumpCooldownActiveSprite;
    public Sprite flyCooldownActiveSprite;


    [Header("Skill UI Text")]
    public TMP_Text activeTimerText;

    private Dictionary<string, Sprite> activeSprites;

    private Dictionary<string, Sprite> cooldownBaseSprites;
    private Dictionary<string, Sprite> cooldownActiveSprites;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitSkillSprites();
        InitCooldownSprite();
    }

    private void InitSkillSprites()
    {
        activeSprites = new Dictionary<string, Sprite>
        {
            { "fly",  flyActiveSprite },
        };
    }


    private void InitCooldownSprite()
    {
        cooldownBaseSprites = new Dictionary<string, Sprite>
    {
        { "dash", dashCooldownBaseSprite },
        { "jump", jumpCooldownBaseSprite },
        { "fly",  flyCooldownBaseSprite },
    };

        cooldownActiveSprites = new Dictionary<string, Sprite>
    {
        { "dash", dashCooldownActiveSprite },
        { "jump", jumpCooldownActiveSprite },
        { "fly",  flyCooldownActiveSprite },
    };

        // 초기 상태
        activePanel?.SetActive(false);
        cooldownBasePanel?.SetActive(false);
        cooldownActivePanel?.SetActive(false);
    }

    public void ShowSkillActive(string skillType)
    {
        Debug.Log("[UIManager] ShowSkillActive");

        if (activeSprites.TryGetValue(skillType.ToLower(), out var sprite))
        {
            activePanel?.SetActive(true);
            activeImage.sprite = sprite;
            activePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[UIManager] No active sprite for: {skillType}");
        }
    }

    public void ShowSkillCooldown(string skillType)
    {
        HideAllSkillUI();
        if (cooldownBaseSprites.TryGetValue(skillType.ToLower(), out var sprite))
        {
            cooldownBasePanel?.SetActive(true);
            cooldownBaseImage.sprite = sprite;
            cooldownBasePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[UIManager] No cooldown base sprite for: {skillType}");
        }
    }

    public void UpdateSkillActiveTime(float time)
    {
        activeTimerText.text = $"{time:F0}";
    }

    public void UpdateSkillCooldownTime(string skillType, float time, float totalCooldownTime)
    {
        Debug.Log("[UIManager] UpdateSkillCooldownTime");

        skillType = skillType.ToLower();

        if (!cooldownActiveSprites.TryGetValue(skillType, out var activeSprite) ||
            !cooldownBaseSprites.TryGetValue(skillType, out var baseSprite))
            return;

        cooldownBasePanel?.SetActive(true);
        cooldownActivePanel?.SetActive(true);

        // Base sprite는 항상 보여줌
        cooldownBaseImage.sprite = baseSprite;

        float fill = Mathf.Clamp01(1f - time / totalCooldownTime);

        if (fill < 1f)
        {
            cooldownActiveImage.sprite = activeSprite;
            cooldownActiveImage.fillAmount = fill;
            cooldownActivePanel.SetActive(true);
        }
        else
        {
            cooldownActiveImage.fillAmount = 0f;
        }
    }

    public void HideAllSkillUI()
    {
        activePanel.SetActive(false);
        cooldownBasePanel.SetActive(false);
        cooldownActivePanel.SetActive(false);
    }

    public void OnMountSkillUI(string skillType)
    {
        Debug.Log($"[UIManager] OnMountSKillUI: {skillType}");

        skillType = skillType.ToLower();

        if (!cooldownBaseSprites.TryGetValue(skillType, out var baseSprite) ||
            !cooldownActiveSprites.TryGetValue(skillType, out var activeSprite))
        {
            Debug.LogWarning($"[UIManager] Mount UI Init failed: invalid skillType '{skillType}'");
            return;
        }

        // 다른 UI는 꺼줌
        activePanel?.SetActive(false);

        // 쿨다운 UI는 켜줌
        cooldownBaseImage.sprite = baseSprite;
        cooldownBasePanel.SetActive(true);

        cooldownActiveImage.sprite = activeSprite;
        cooldownActiveImage.fillAmount = 1f;
        cooldownActivePanel.SetActive(true);
    }

}
