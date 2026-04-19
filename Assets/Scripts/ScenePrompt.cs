using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ScenePrompt : MonoBehaviour
{
    [Header("Animated Prompt Image")]
    [SerializeField] private Image promptImage;
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();

    [Header("Animation")]
    [SerializeField] private float frameTime = 0.15f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine animationRoutine;
    private int currentFrame = 0;
    private bool isTransitioning = false;

    private void OnEnable()
    {
        if (sprites.Count > 0 && promptImage != null)
        {
            animationRoutine = StartCoroutine(AnimatePrompt());
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }

    private void Update()
    {
        if (isTransitioning)
            return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(FadeAndLoadNextScene());
        }
    }

    private IEnumerator FadeAndLoadNextScene()
    {
        isTransitioning = true;

        if (fadeCanvasGroup == null)
        {
            LoadNextScene();
            yield break;
        }

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.transform.SetAsLastSibling();
        fadeCanvasGroup.alpha = 0f;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        yield return null;

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("No next scene in build settings.");
        }
    }

    private IEnumerator AnimatePrompt()
    {
        while (true)
        {
            promptImage.sprite = sprites[currentFrame];

            currentFrame++;
            if (currentFrame >= sprites.Count)
            {
                currentFrame = 0;
            }

            yield return new WaitForSeconds(frameTime);
        }
    }
}