using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class LeaderboardController : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;

    void Start()
    {
        List<ScoreEntry> scores = ScoreManager.LoadScores();
        int displayCount = Mathf.Min(5, scores.Count);

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < displayCount; i++)
        {
            var entry = scores[i];

            int minutes = Mathf.FloorToInt(entry.timeTaken / 60f);
            int seconds = Mathf.FloorToInt(entry.timeTaken % 60f);
            string timeFormatted = $"{minutes:00}:{seconds:00}";

            sb.AppendLine($"{i + 1}. {timeFormatted} - {entry.date}");
        }

        leaderboardText.text = sb.ToString();
    }
}
