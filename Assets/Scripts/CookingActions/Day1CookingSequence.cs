using UnityEngine;
using System.Collections.Generic;

// Chains cooking minigames in order. Add any minigame (including duplicates) to the list.
public class Day1CookingSequence : MonoBehaviour
{
    [SerializeField] private List<CookingMinigameBase> sequence;

    private int currentIndex = 0;

    private void Start()
    {
        foreach (var minigame in sequence)
            minigame.HidePanel();

        RunCurrent();
    }

    private void RunCurrent()
    {
        if (currentIndex >= sequence.Count)
        {
            Debug.Log("Day 1 cooking complete!");
            return;
        }

        CookingMinigameBase current = sequence[currentIndex];
        current.OnSuccess += OnCurrentSuccess;
        current.OnFail    += OnCurrentFail;
        current.StartMinigame();
    }

    private void OnCurrentSuccess()
    {
        Unsubscribe(sequence[currentIndex]);
        currentIndex++;
        RunCurrent();
    }

    private void OnCurrentFail()
    {
        Unsubscribe(sequence[currentIndex]);
        Debug.Log("Cooking failed!");
        // Hook up retry / game-over logic here
    }

    private void Unsubscribe(CookingMinigameBase minigame)
    {
        minigame.OnSuccess -= OnCurrentSuccess;
        minigame.OnFail    -= OnCurrentFail;
    }
}
