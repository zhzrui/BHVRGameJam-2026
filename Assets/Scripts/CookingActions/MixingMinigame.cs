using UnityEngine;
using UnityEngine.UI;

public class MixingMinigame : CookingMinigameBase
{
    //private void Start() => StartMinigame();

    [Header("Meter Settings")]
    [SerializeField] private float maxMeter = 100f;
    [SerializeField] private float clickFillAmount = 8f;
    [SerializeField] private float decayRate = 10f;
    [SerializeField] private float successThreshold = 95f;

    [SerializeField] private string objectiveKey = "cooking";

    [Header("UI")]
    [SerializeField] private Slider meterSlider;

    private float currentMeter;
    private bool isActive;

    public override void StartMinigame()
    {
        currentMeter = 0f;
        isActive = true;
        minigamePanel.SetActive(true);
        UpdateUI();
    }

    public void OnClick()
    {
        if (!isActive) return;

        currentMeter = Mathf.Min(maxMeter, currentMeter + clickFillAmount);
        UpdateUI();

        if (currentMeter >= successThreshold)
            Complete(true);
    }

    private void Update()
    {
        if (!isActive) return;
        currentMeter = Mathf.Max(0f, currentMeter - decayRate * Time.deltaTime);
        UpdateUI();
    }

    private void Complete(bool success)
    {
        isActive = false;
        minigamePanel.SetActive(false);

        if (success)
        {
            //ObjectiveManager.Instance?.CompleteObjective(objectiveKey);

            DialogueManager dm = FindFirstObjectByType<DialogueManager>();
            if (dm != null)
            {
                dm.MarkObjectiveComplete(objectiveKey);
            }

            RaiseSuccess();
        }
        else RaiseFail();
    }

    private void UpdateUI()
    {
        if (meterSlider != null)
            meterSlider.value = currentMeter / maxMeter;
    }
}
