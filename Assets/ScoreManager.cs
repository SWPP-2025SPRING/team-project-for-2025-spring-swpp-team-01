using UnityEngine;
using System.Collections.Generic;
using System;

public static class ScoreManager
{
    private const string SaveKey = "LeaderboardData";

    public static void SaveTime(float timeTaken)
    {
        List<ScoreEntry> scores = LoadScores();

        scores.Add(new ScoreEntry
        {
            timeTaken = timeTaken,
            date = DateTime.Now.ToString("yyyy.MM.dd HH:mm")
        });

        scores.Sort((a, b) => a.timeTaken.CompareTo(b.timeTaken)); // 빠른 시간 순

        string json = JsonUtility.ToJson(new ScoreListWrapper { list = scores });
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static List<ScoreEntry> LoadScores()
    {
        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json))
            return new List<ScoreEntry>();

        return JsonUtility.FromJson<ScoreListWrapper>(json).list;
    }

    [Serializable]
    private class ScoreListWrapper
    {
        public List<ScoreEntry> list;
    }
}
