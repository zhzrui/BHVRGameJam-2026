using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// Chains cooking minigames in order. Add any minigame (including duplicates) to the list.
public class Day1CookingSequence : MonoBehaviour
{
    [SerializeField] private List<CookingMinigameBase> sequence;

    [Header("Failure")]
    [SerializeField] private GameObject foodDisplayContainer;
    [SerializeField] private Image foodImage;
    [SerializeField] private Sprite ruinedFoodSprite;
    [SerializeField] private float restartDelay = 2f;

    private int currentIndex = 0;

    private void Start()
    {
        if (foodDisplayContainer != null)
            foodDisplayContainer.SetActive(false);

        foreach (var minigame in sequence)
            minigame.HidePanel();

        RunCurrent();
    }

    private void RunCurrent()
    {
        if (currentIndex >= sequence.Count)
        {
            Debug.Log("Day cooking complete!");
            LevelManager.Instance?.LoadNextLevel();
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
        StartCoroutine(ShowRuinedAndRestart());
    }

    private IEnumerator ShowRuinedAndRestart()
    {
        if (foodDisplayContainer != null)
        {
            if (foodImage != null && ruinedFoodSprite != null)
                foodImage.sprite = ruinedFoodSprite;
            foodDisplayContainer.SetActive(true);
        }

        yield return new WaitForSeconds(restartDelay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Unsubscribe(CookingMinigameBase minigame)
    {
        minigame.OnSuccess -= OnCurrentSuccess;
        minigame.OnFail    -= OnCurrentFail;
    }
}
