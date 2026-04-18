using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLineData
{
    [TextArea(3, 6)]
    public string text;

    [Header("Optional flag")]
    public string flagToSet;

    [Header("Objective control")]
    public bool pauseForObjective;
    public string objectiveId;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dialogueText;
    public GameObject continueIndicator;

    [Header("Dialogue")]
    public DialogueLineData[] lines;

    [Header("Voice (per line)")]
    [Tooltip("Optional: one voice clip per line. Index must match lines[].")]
    public AudioClip[] voiceClips;
    [SerializeField] private AudioSource voiceSource;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    public bool stopVoiceOnSkip = false;

    [Header("Cut rule")]
    [Tooltip("Any clip in this list will be cut immediately when the line is fully shown.")]
    public AudioClip[] cutOnLineEndClips;

    [Header("Behavior")]
    public bool autoLoadNextLevel = true;
    public UnityEvent onDialogueComplete = new UnityEvent();

    [Header("Indicator Pulse")]
    public float pulseSpeed = 2.5f;
    [Range(0f, 1f)] public float minPulseAlpha = 0.3f;

    private int currentLine = 0;
    private Coroutine pulseCoroutine;
    private CanvasGroup indicatorCanvasGroup;

    private bool waitingForObjective = false;
    private string waitingObjectiveId = "";

    void Awake()
    {
        if (continueIndicator != null)
        {
            indicatorCanvasGroup = continueIndicator.GetComponent<CanvasGroup>();
            if (indicatorCanvasGroup == null)
                indicatorCanvasGroup = continueIndicator.AddComponent<CanvasGroup>();
        }

        if (voiceSource == null)
            voiceSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        HideIndicator();

        if (lines == null || lines.Length == 0)
        {
            dialogueText.text = "";
            return;
        }

        ShowLine(currentLine);
    }

    void Update()
    {
        if (waitingForObjective)
            return;

        if (WasAdvancePressedThisFrame())
        {
            OnAdvance();
        }
    }

    private bool WasAdvancePressedThisFrame()
    {
        bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return space || click;
    }

    private void OnAdvance()
    {
        currentLine++;

        if (currentLine < lines.Length)
        {
            ShowLine(currentLine);
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowLine(int index)
    {
        if (index < 0 || index >= lines.Length)
            return;

        HideIndicator();

        DialogueLineData lineData = lines[index];

        // Show text instantly
        dialogueText.text = lineData.text;

        // Optional flag
        if (!string.IsNullOrEmpty(lineData.flagToSet))
        {
            DialogueFlags.SetFlag(lineData.flagToSet);
        }

        PlayVoiceForLine(index);

        // Since the line is fully displayed immediately,
        // apply the "cut on line end" rule right away.
        CutIfCurrentClipMatchesRule();

        if (lineData.pauseForObjective)
        {
            waitingForObjective = true;
            waitingObjectiveId = lineData.objectiveId;

            HideIndicator();

            // Start objective in gameplay
            ObjectiveManager.Instance?.StartObjective(waitingObjectiveId);
        }
        else
        {
            ShowIndicator();
        }
    }

    public void ResumeAfterObjective(string objectiveId)
    {
        if (!waitingForObjective)
            return;

        if (objectiveId != waitingObjectiveId)
            return;

        waitingForObjective = false;
        waitingObjectiveId = "";

        currentLine++;

        if (currentLine < lines.Length)
        {
            ShowLine(currentLine);
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        HideIndicator();
        onDialogueComplete?.Invoke();

        if (autoLoadNextLevel)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }

    private void PlayVoiceForLine(int index)
    {
        if (voiceSource == null) return;

        AudioClip clip = null;
        if (voiceClips != null && index >= 0 && index < voiceClips.Length)
            clip = voiceClips[index];

        if (clip == null)
            return;

        voiceSource.volume = voiceVolume;
        voiceSource.loop = false;
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    private void StopVoice()
    {
        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();
    }

    private void CutIfCurrentClipMatchesRule()
    {
        if (voiceSource == null) return;
        if (!voiceSource.isPlaying) return;
        if (cutOnLineEndClips == null || cutOnLineEndClips.Length == 0) return;

        AudioClip current = voiceSource.clip;
        if (current == null) return;

        for (int i = 0; i < cutOnLineEndClips.Length; i++)
        {
            if (current == cutOnLineEndClips[i])
            {
                voiceSource.Stop();
                return;
            }
        }
    }

    private void ShowIndicator()
    {
        if (continueIndicator == null || indicatorCanvasGroup == null)
            return;

        continueIndicator.SetActive(true);

        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);

        pulseCoroutine = StartCoroutine(PulseIndicator());
    }

    private void HideIndicator()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        if (indicatorCanvasGroup != null)
            indicatorCanvasGroup.alpha = 0f;

        if (continueIndicator != null)
            continueIndicator.SetActive(false);
    }

    private IEnumerator PulseIndicator()
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * pulseSpeed;
            float alpha = Mathf.Lerp(minPulseAlpha, 1f, (Mathf.Sin(t) + 1f) / 2f);
            indicatorCanvasGroup.alpha = alpha;
            yield return null;
        }
    }
}