using UnityEngine;

[System.Serializable]
public class AudioConfig
{
    public AudioClip clip;
    [Range(0f, 2f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public float startTime = 0f;
    public float endTime = -1f; // -1이면 끝까지 재생
}
