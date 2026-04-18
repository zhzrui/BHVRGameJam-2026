using UnityEngine;
using UnityEngine.UI;
using System;

public class MixingMinigame : MonoBehaviour
{
    //private void Start() => StartMinigame();

    [Header("Meter Settings")]
    [SerializeField] private float maxMeter = 100f;
    [SerializeField] private float clickFillAmount = 8f;
    [SerializeField] private float decayRate = 10f;       // units per second lost passively
    [SerializeField] private float successThreshold = 95f; // fill required to complete

    [Header("UI")]
    [SerializeField] private Slider meterSlider;
    [SerializeField] private GameObject minigamePanel;

    public event Action OnSuccess;
    public event Action OnFail;

    private float currentMeter;
    private bool isActive;

    public void StartMinigame()
    {
        currentMeter = 0f;
        isActive = true;
        minigamePanel.SetActive(true);
        UpdateUI();
    }

    private void Update()
    {
        if (!isActive) return;

        // Passive decay
        currentMeter = Mathf.Max(0f, currentMeter - decayRate * Time.deltaTime);
        UpdateUI();
    }

    // Call this from a UI Button's OnClick or an input handler
    public void OnClick()
    {
        if (!isActive) return;

        currentMeter = Mathf.Min(maxMeter, currentMeter + clickFillAmount);
        UpdateUI();

        if (currentMeter >= successThreshold)
            Complete(true);
    }

    private void Complete(bool success)
    {
        isActive = false;
        minigamePanel.SetActive(false);

        if (success) OnSuccess?.Invoke();
        else OnFail?.Invoke();
    }

    private void UpdateUI()
    {
        if (meterSlider != null)
            meterSlider.value = currentMeter / maxMeter;
    }
}
