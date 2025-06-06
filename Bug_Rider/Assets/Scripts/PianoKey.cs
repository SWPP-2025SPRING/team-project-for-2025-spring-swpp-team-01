using UnityEngine;

public class PianoKey : MonoBehaviour
{
    public string noteName;         // 예: "C", "D", "E", ...
    public AudioClip noteSound;     // 도레미파 소리 클립

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void PlayNote()
    {
        if (noteSound != null)
        {
            audioSource.PlayOneShot(noteSound);
        }
        else
        {
            Debug.LogWarning($"{noteName} sound not set.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayNote();
        }
    }
}
