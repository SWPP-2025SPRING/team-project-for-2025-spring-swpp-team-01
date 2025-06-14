using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [System.Serializable]
    public class AudioEntry
    {
        public string key;
        public AudioConfig config;
    }


    public List<AudioEntry> audioEntries;

    private Dictionary<string, AudioConfig> audioDict;
    public AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // audioSource = gameObject.AddComponent<AudioSource>();
            InitDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitDictionary()
    {
        audioDict = new Dictionary<string, AudioConfig>();
        foreach (var entry in audioEntries)
        {
            if (!audioDict.ContainsKey(entry.key))
                audioDict.Add(entry.key, entry.config);
        }
    }

    public void Play(string key, bool loop = false)
    {
        if (!audioDict.ContainsKey(key)) return;
        AudioConfig config = audioDict[key];

        StopAllCoroutines();
        StartCoroutine(PlayWithConfig(config, loop));
    }

    private IEnumerator PlayWithConfig(AudioConfig config, bool loop)
    {
        if (config == null || config.clip == null) yield break;
        Debug.Log($"PlayWithConfig: clip={config.clip?.name}, volume={config.volume}, pitch={config.pitch}");

        audioSource.loop = false;
        audioSource.clip = config.clip;
        audioSource.volume = config.volume;
        audioSource.pitch = config.pitch;
        audioSource.time = config.startTime;
        audioSource.Play();

        float duration = (config.endTime > 0)
            ? Mathf.Clamp(config.endTime - config.startTime, 0f, config.clip.length - config.startTime)
            : config.clip.length - config.startTime;

        if (loop)
        {
            while (true)
            {
                yield return new WaitForSeconds(duration);
                audioSource.time = config.startTime;
                audioSource.Play();
            }
        }
        else
        {
            yield return new WaitForSeconds(duration);
            audioSource.Stop();
        }
    }

    public void Stop()
    {
        Debug.Log("StopAllCoroutines");
        StopAllCoroutines();
        audioSource.Stop();
    }
}
