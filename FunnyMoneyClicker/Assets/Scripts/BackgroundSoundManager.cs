using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager instance;

    [Header("Music Settings")]
    public List<AudioClip> musicClips = new List<AudioClip>();
    public bool shuffle = true;
    public bool loopAll = true;
    [Range(0f, 1f)] public float volume = 0.6f;
    public float fadeDuration = 1f;

    private AudioSource audioSource;
    private List<int> playedIndices = new List<int>();
    private int currentClipIndex = -1;
    private bool isFading = false;

    private void Awake()
    {
        // Singleton pattern to persist between scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        //DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    private void Start()
    {
        PlayNextTrack();
    }

    private void Update()
    {
        // Check if the song finished
        if (!audioSource.isPlaying && !isFading)
        {
            PlayNextTrack();
        }
    }

    private void PlayNextTrack()
    {
        if (musicClips.Count == 0) return;

        int nextIndex;

        // Shuffle mode: no repeats until all have played
        if (shuffle)
        {
            if (playedIndices.Count >= musicClips.Count)
                playedIndices.Clear(); // reset when all played

            do
            {
                nextIndex = Random.Range(0, musicClips.Count);
            } while (playedIndices.Contains(nextIndex));

            playedIndices.Add(nextIndex);
        }
        else
        {
            nextIndex = (currentClipIndex + 1) % musicClips.Count;
        }

        currentClipIndex = nextIndex;
        StartCoroutine(FadeToTrack(musicClips[nextIndex]));
    }

    private System.Collections.IEnumerator FadeToTrack(AudioClip newClip)
    {
        isFading = true;

        // Fade out
        float startVolume = audioSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, volume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = volume;
        isFading = false;
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
}
