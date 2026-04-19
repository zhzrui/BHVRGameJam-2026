using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DialogueLineData
{
    [TextArea(3, 6)]
    public string text;

    [Header("Optional flag")]
    public string flagToSet;
    [Header("Sprite Update")]
    public string[] spriteUpdates;

    [Header("Objective control")]
    public bool pauseForObjective;
    public string objectiveId;

    [Header("Food Display")]
    public bool showFoodDisplay;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dialogueText;
    public GameObject continueIndicator;
    public ScreenSprite screenSprite;

    [Header("Dialogue")]
    public DialogueLineData[] lines;

    [Header("Voice (per line)")]
    [Tooltip("Optional: one voice clip per line. Index must match lines[].")]
    public AudioClip[] voiceClips;
    [SerializeField] private AudioSource voiceSource;
    [Range(0f, 1f)] public float voiceVolume = 1f;

    [Header("Cut rule")]
    [Tooltip("Any clip in this list will be cut immediately when the line is fully shown.")]
    public AudioClip[] cutOnLineEndClips;

    [Header("Behavior")]
    public bool autoLoadNextLevel = false;
    public UnityEvent onDialogueComplete = new UnityEvent();
    public UnityEvent onPausedForObjective = new UnityEvent();

    [Header("Scene Transition Prompt")]
    [SerializeField] private GameObject scenePrompt;
    [SerializeField] private GameObject eKeyPrompt;
    [SerializeField] private float eKeyDelay = 0.8f;

    [Header("Food Display")]
    [SerializeField] private GameObject foodDisplayContainer;
    [SerializeField] private Image foodImage;
    [SerializeField] private Sprite completedFoodSprite;

    private int currentLine = 0;

    private bool waitingForObjective = false;
    private string waitingObjectiveId = "";

    void Awake()
    {
        if (voiceSource == null)
            voiceSource = GetComponent<AudioSource>();
        if (screenSprite == null) Debug.LogError("screensprite not assigned!", this.gameObject);
    }

    void Start()
    {
        HideIndicator();

        if (eKeyPrompt != null)
            eKeyPrompt.SetActive(false);

        if (foodDisplayContainer != null)
            foodDisplayContainer.SetActive(false);


        if (lines == null || lines.Length == 0)
        {
            if (dialogueText != null)
                dialogueText.text = "";
            return;
        }

        currentLine = 0;
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
        return (space || click) && Time.timeScale > 0;
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
        if (lines == null || index < 0 || index >= lines.Length)
            return;

        HideIndicator();

        DialogueLineData lineData = lines[index];

        if (dialogueText != null)
            dialogueText.text = lineData.text;

        if (!string.IsNullOrEmpty(lineData.flagToSet))
        {
            DialogueFlags.SetFlag(lineData.flagToSet);
            Debug.Log("Dialogue flag set: " + lineData.flagToSet);
        }

        if (lineData.spriteUpdates != null)
        {
            screenSprite.UpdateSprites(lineData.spriteUpdates); // error handling in SpriteUpdates :thumbs_up:
        }

        if (lineData.showFoodDisplay && foodDisplayContainer != null)
        {
            if (foodImage != null && completedFoodSprite != null)
                foodImage.sprite = completedFoodSprite;
            foodDisplayContainer.SetActive(true);
        }

        PlayVoiceForLine(index);
        CutIfCurrentClipMatchesRule();

        if (lineData.pauseForObjective && !string.IsNullOrEmpty(lineData.objectiveId))
        {
            waitingForObjective = true;
            waitingObjectiveId = lineData.objectiveId;

            Debug.Log("Dialogue paused, waiting for objective: " + waitingObjectiveId);
            HideIndicator();
            onPausedForObjective?.Invoke();
        }
        else
        {
            waitingForObjective = false;
            waitingObjectiveId = "";
            ShowIndicator();
        }
    }

    public void MarkObjectiveComplete(string objectiveId)
    {
        Debug.Log("MarkObjectiveComplete called with: " + objectiveId);
        Debug.Log("Currently waiting for: " + waitingObjectiveId);

        if (!waitingForObjective)
            return;

        if (string.IsNullOrEmpty(objectiveId))
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

    public bool IsWaitingForObjective()
    {
        return waitingForObjective;
    }

    public string GetWaitingObjectiveId()
    {
        return waitingObjectiveId;
    }

    private void EndDialogue()
    {
        HideIndicator();
        onDialogueComplete?.Invoke();

        if (foodDisplayContainer != null)
        {
            if (foodImage != null && completedFoodSprite != null)
                foodImage.sprite = completedFoodSprite;
            foodDisplayContainer.SetActive(true);
        }

        if (eKeyPrompt != null)
        {
            StartCoroutine(ShowEKeyAfterDelay());
            return;
        }

        else if (autoLoadNextLevel && LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }

    private IEnumerator ShowEKeyAfterDelay()
    {
        yield return new WaitForSeconds(eKeyDelay);
        eKeyPrompt.SetActive(true);
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
        if (continueIndicator != null)
            continueIndicator.SetActive(true);
    }

    private void HideIndicator()
    {
        if (continueIndicator != null)
            continueIndicator.SetActive(false);
    }
}
