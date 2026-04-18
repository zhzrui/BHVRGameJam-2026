using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // force singleton behaviour
    public static MusicManager instance;
    [SerializeField] List<AudioSource> musicPlayers;
    [SerializeField] float fadeTime = 5.0f;
    int currentMusicPlayer = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Destroy(this.gameObject);
        }
    }

    void PlayNew(AudioClip audioClip)
    {
        // fade
        AudioSource prev = GetCurrentSource();
        AudioSource next = GetNextSource();

        next.clip = audioClip;
        next.Play();
        prev.DOFade(0f, fadeTime);
        next.DOFade(1f, fadeTime);
    }

    void Stop()
    {
        GetCurrentSource().DOFade(0f, fadeTime).OnComplete(() => { GetCurrentSource().Pause(); });
    }

    void Play()
    {
        GetCurrentSource().Play();
        GetCurrentSource().DOFade(1f, fadeTime);
    }

    AudioSource GetCurrentSource()
    {
        return musicPlayers[currentMusicPlayer];
    }

    AudioSource GetNextSource()
    {
        currentMusicPlayer += 1;
        if (currentMusicPlayer >= musicPlayers.Count) currentMusicPlayer = 0;
        return GetCurrentSource();
    }
}
