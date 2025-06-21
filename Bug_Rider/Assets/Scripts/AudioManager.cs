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
    public AudioSource bgmSource;
    public AudioSource bugSource;
    public AudioSource obstacleSource;

    public float bgmFadeDuration = 1.5f;
    public float fadeInTime = 0.3f;
    public float fadeOutTime = 0.3f;

    private Coroutine bgmFadeCoroutine;
    private Coroutine bugCoroutine;
    private Coroutine obstacleCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitDictionary();

            if (bgmSource == null)
                bgmSource = gameObject.AddComponent<AudioSource>();
            if (bugSource == null)
                bugSource = gameObject.AddComponent<AudioSource>();
            if (obstacleSource == null)
                obstacleSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("Starting BGM");
        PlayBGM("Stage1");
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

    // ---------------- BGM ----------------

    public void PlayBGM(string key, bool loop = true)
    {
        if (!audioDict.ContainsKey(key)) return;
        AudioConfig config = audioDict[key];
        if (config == null || config.clip == null) return;

        if (bgmFadeCoroutine != null)
            StopCoroutine(bgmFadeCoroutine);

        bgmSource.clip = config.clip;
        bgmSource.loop = loop;
        bgmSource.pitch = config.pitch;
        bgmSource.time = config.startTime;
        bgmSource.volume = 0f;
        bgmSource.Play();
        Debug.Log("PlayedBGM");
        bgmFadeCoroutine = StartCoroutine(FadeIn(bgmSource, config.volume, bgmFadeDuration));
    }

    public void StopBGM()
    {
        if (bgmFadeCoroutine != null)
            StopCoroutine(bgmFadeCoroutine);

        bgmFadeCoroutine = StartCoroutine(FadeOutAndStop(bgmSource, bgmFadeDuration));
    }

    // ---------------- Bug Sound ----------------

    public void PlayBug(string key, bool loop = false)
    {
        if (!audioDict.ContainsKey(key)) return;
        AudioConfig config = audioDict[key];

        if (bugCoroutine != null)
            StopCoroutine(bugCoroutine);

        Debug.Log("[AudioManager] PlayBug: " + key);
        bugCoroutine = StartCoroutine(PlaySoundCoroutine(bugSource, config, loop));
    }

    public void StopBug()
    {
        Debug.Log("[AudioManager] StopBug");

        if (bugCoroutine != null)
            StopCoroutine(bugCoroutine);
        StartCoroutine(FadeOutAndStop(bugSource, fadeOutTime));
    }

    // ---------------- Obstacle Sound ----------------

    public void PlayObstacle(string key, bool loop = false)
    {
        if (!audioDict.ContainsKey(key)) return;
        AudioConfig config = audioDict[key];

        if (obstacleCoroutine != null)
            StopCoroutine(obstacleCoroutine);

        obstacleCoroutine = StartCoroutine(PlaySoundCoroutine(obstacleSource, config, loop));
    }

    public void StopObstacle()
    {
        if (obstacleCoroutine != null)
            StopCoroutine(obstacleCoroutine);
        StartCoroutine(FadeOutAndStop(obstacleSource, fadeOutTime));
    }

    // ---------------- Common Coroutine ----------------

    private IEnumerator PlaySoundCoroutine(AudioSource source, AudioConfig config, bool loop)
    {
        if (config == null || config.clip == null) yield break;

        source.clip = config.clip;
        source.loop = loop;
        source.pitch = config.pitch;
        source.time = config.startTime;
        source.volume = 0f;
        source.Play();

        yield return StartCoroutine(FadeIn(source, config.volume, fadeInTime));

        if (loop)
        {
            // 루프는 여기서 끝 (계속 재생됨)
            yield break;
        }

        float duration = (config.endTime > 0)
            ? Mathf.Clamp(config.endTime - config.startTime, 0f, config.clip.length - config.startTime)
            : config.clip.length - config.startTime;

        float playTime = duration - fadeOutTime;
        if (playTime > 0)
            yield return new WaitForSeconds(playTime);

        yield return StartCoroutine(FadeOutAndStop(source, fadeOutTime));
    }

    private IEnumerator FadeIn(AudioSource source, float targetVolume, float duration)
    {
        float t = 0f;
        source.volume = 0.1f;  

        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, t / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }
        source.volume = 0f;
        source.Stop();
    }
}
