using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }
    [Min(1)] [SerializeField] int poolSize = 5;
    [Min(1)] [SerializeField] int loopPoolSize = 3;
    int currentAudioSource = 0;
    List<AudioSource> audioPool = new();
    [SerializeField] List<SoundEffect> inspectorSoundEffects = new(); // only used to initialize. internally uses dictionary.
    Dictionary<string, AudioClip> soundEffects = new();
    List<AudioSource> loopPool = new();
    Dictionary<AudioClip, AudioSource> loopUsed = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < poolSize; ++i)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            audioPool.Add(source);
        }

        for (int i = 0; i < loopPoolSize; ++i)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            loopPool.Add(source);
        }

        DontDestroyOnLoad(gameObject);

        // initialize dictionary
        foreach (SoundEffect soundEffect in inspectorSoundEffects)
        {
            soundEffects.Add(soundEffect.name, soundEffect.audioClip);
        }
    }

    public void Play(string sound)
    {
        AudioClip audioClip;
        soundEffects.TryGetValue(sound, out audioClip);
        if (audioClip == null)
        {
            Debug.LogError("could not find sound "+sound);
        } else
        {
            Play(audioClip);
        }
    }

    public void Play(AudioClip clip)
    {
        currentAudioSource++;
        if (currentAudioSource >= audioPool.Count)
        {
            currentAudioSource = 0;
        }

        audioPool[currentAudioSource].pitch = Random.Range(0.9f, 1.1f);
        audioPool[currentAudioSource].PlayOneShot(clip);
    }

    public void PlayLoop(string sound)
    {
        AudioClip audioClip;
        soundEffects.TryGetValue(sound, out audioClip);
        if (audioClip == null)
        {
            Debug.LogError("could not find sound "+sound);
        } else
        {
            PlayLoop(audioClip);
        }
    }

    public void PlayLoop(AudioClip clip)
    {
        if (loopPool.Count > 0)
        {
            loopPool.Last().clip = clip;
            loopPool.Last().Play();
            loopUsed.Add(clip, loopPool.Last());
            loopPool.RemoveAt(loopPool.Count - 1);
        } else
        {
            Debug.LogWarning("not enough audiosources in loop pool");
        }
    }

    public void StopLoop(string sound)
    {
        AudioClip audioClip;
        soundEffects.TryGetValue(sound, out audioClip);
        if (audioClip == null)
        {
            Debug.LogError("could not find sound "+sound);
        } else
        {
            StopLoop(audioClip);
        }
    }

    public void StopLoop(AudioClip clip)
    {
        AudioSource loopSource;
        loopUsed.TryGetValue(clip, out loopSource);
        if (loopSource != null)
        {
            loopUsed.Remove(clip);
            loopSource.Stop();
            loopPool.Add(loopSource);
        }
    }
}