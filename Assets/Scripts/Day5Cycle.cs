using System.Collections;
using System.Diagnostics;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Day5Cycle : MonoBehaviour
{
    [SerializeField] private Image displayImage;
    [SerializeField] private Sprite[] staticImages;

    [Header("Prompt")]
    [SerializeField] private GameObject ePrompt;
    [SerializeField] private GameObject finalPrompt;
    [SerializeField] private Transform promptParent;

    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float waitBeforeQuit = 1f;
    [SerializeField] private float spriteFadeDuration = 0.4f;

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        if (displayImage != null && staticImages.Length > 0)
        {
            displayImage.sprite = staticImages[currentIndex];
        }

        UpdatePrompt();
    }

    void Update()
    {
        if (isTransitioning)
            return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentIndex < staticImages.Length - 1)
            {
                StartCoroutine(FadeToNextImage());
            }
            else
            {
                OnFinalImageInteract();
            }
        }
    }

    private void NextImageImmediate()
    {
        if (staticImages == null || staticImages.Length == 0)
            return;

        if (currentIndex >= staticImages.Length - 1)
            return;

        currentIndex++;
        displayImage.sprite = staticImages[currentIndex];

        UpdatePrompt();
    }

    private IEnumerator FadeToNextImage()
    {
        if (fadeCanvasGroup == null)
        {
            NextImageImmediate();
            yield break;
        }

        isTransitioning = true;

        fadeCanvasGroup.gameObject.SetActive(true);

        float timer = 0f;

        // Fade to black
        while (timer < spriteFadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / spriteFadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        // Change image while fully black
        NextImageImmediate();

        yield return null;

        timer = 0f;

        // Fade back out
        while (timer < spriteFadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / spriteFadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        isTransitioning = false;
    }



    private void UpdatePrompt()
    {
        bool isFinalImage = currentIndex == staticImages.Length - 1;

        if (ePrompt != null)
            ePrompt.SetActive(!isFinalImage);

        if (finalPrompt != null)
            finalPrompt.SetActive(isFinalImage);
    }

    private IEnumerator FadeToBlackAndEnd()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning("Fade Canvas Group is not assigned.");
            yield break;
        }

        fadeCanvasGroup.gameObject.SetActive(true);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(waitBeforeQuit);

        Debug.Log("Game ended");

        yield return new WaitForSeconds(waitBeforeQuit);

        Debug.Log("Returning to main menu");

        SceneManager.LoadScene(0);
    }

    private void OnFinalImageInteract()
    {
        Debug.Log("Final image action here");
        StartCoroutine(FadeToBlackAndEnd());
    }
}