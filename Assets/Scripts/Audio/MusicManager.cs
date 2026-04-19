using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [Serializable]
    public struct SceneTrack
    {
        public string sceneName;     // must match Build Settings scene name
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    public static MusicManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private List<AudioSource> musicSources;
    private int playingMusicSourceIndex;

    [Header("Per-scene tracks")]
    [SerializeField] private SceneTrack[] tracks;

    [Header("Behavior")]
    [SerializeField] private bool playOnStart = true;

    private Dictionary<string, SceneTrack> map;

    public enum UnmappedSceneBehavior { KeepPlaying, Stop, PlayDefault }
    [SerializeField] float fadeTime = 2f;
    [SerializeField] AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Fallback")]
    [SerializeField] private UnmappedSceneBehavior unmappedBehavior = UnmappedSceneBehavior.Stop;
    [SerializeField] private AudioClip defaultClip;
    [Range(0f, 1f)][SerializeField] private float defaultVolume = 1f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);

        // Ensure the AudioSource is on THIS persistent object
        // If inspector reference points to a scene object, it will become null after scene unload.
        if (musicSources == null)
        {
            musicSources = new();
        }

        if (musicSources.Count <= 0)
        {
            musicSources.Add(gameObject.AddComponent<AudioSource>());
            musicSources.Add(gameObject.AddComponent<AudioSource>());
        }

        foreach (AudioSource musicSource in musicSources)
        {
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        // ...the rest of your setup (map + scene event)
        map = new Dictionary<string, SceneTrack>(StringComparer.Ordinal);
        foreach (var t in tracks)
        {
            if (!string.IsNullOrWhiteSpace(t.sceneName) && t.clip != null)
                map[t.sceneName] = t;
        }

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }




    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void Start()
    {
        if (!playOnStart) return;
        TryPlayForScene(SceneManager.GetActiveScene().name);
    }
    
    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"MusicManager scene change: {oldScene.name} -> {newScene.name}");
        TryPlayForScene(newScene.name);
    }


    private void TryPlayForScene(string sceneName)
    {
        if (map.TryGetValue(sceneName, out var t))
        {
            Play(t.clip, t.volume);
            return;
        }

        // Scene not mapped:
        AudioSource musicSource = GetActiveSource();
        switch (unmappedBehavior)
        {
            case UnmappedSceneBehavior.Stop:
                musicSource.Stop();
                musicSource.clip = null;
                break;

            case UnmappedSceneBehavior.PlayDefault:
                if (defaultClip != null) Play(defaultClip, defaultVolume);
                else { musicSource.Stop(); musicSource.clip = null; }
                break;

            case UnmappedSceneBehavior.KeepPlaying:
            default:
                break;
        }
    }

    private AudioSource GetActiveSource()
    {
        return musicSources[playingMusicSourceIndex];
    }

    private void NextSource()
    {
        playingMusicSourceIndex++;
        if (playingMusicSourceIndex >= musicSources.Count) playingMusicSourceIndex = 0;
    }


    public void Play(AudioClip clip, float volume = 1f)
    {
        AudioSource musicSource = GetActiveSource();
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            musicSource.DOFade(volume, fadeTime).SetEase(fadeCurve).SetUpdate(true);
        } else
        {
            // fade current music source
            musicSource.DOFade(0f, fadeTime).SetEase(fadeCurve).SetUpdate(true).OnComplete(() => { musicSource.Stop(); });
            NextSource();
            AudioSource newMusicSource = GetActiveSource();
            newMusicSource.clip = clip;
            newMusicSource.Play();
            newMusicSource.time = UnityEngine.Random.Range(0f, clip.length);
            newMusicSource.volume = 0f;
            newMusicSource.DOFade(volume, fadeTime).SetEase(fadeCurve).SetUpdate(true);
        }

    }
}