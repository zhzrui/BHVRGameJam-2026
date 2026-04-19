using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// Chains cooking minigames in order. Add any minigame (including duplicates) to the list.
public class Day1CookingSequence : MonoBehaviour
{
    [SerializeField] private List<CookingMinigameBase> sequence;

    [Header("Failure")]
    [SerializeField] private GameObject foodDisplayContainer;
    [SerializeField] private Image foodImage;
    [SerializeField] private Sprite ruinedFoodSprite;
    [SerializeField] private float restartDelay = 1f;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private TMP_Text failureText;
    [SerializeField] private string failureLine;
    [SerializeField] private float blackScreenHoldTime = 2f;

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

        if (failureText != null && !string.IsNullOrEmpty(failureLine))
        {
            failureText.gameObject.SetActive(true);
            failureText.text = failureLine;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(blackScreenHoldTime);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Unsubscribe(CookingMinigameBase minigame)
    {
        minigame.OnSuccess -= OnCurrentSuccess;
        minigame.OnFail    -= OnCurrentFail;
    }
}
