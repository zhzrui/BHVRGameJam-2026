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
    [SerializeField] private float fadeDuration = 1.5f;

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
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
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
            Debug.Log("ScenePrompt E pressed on object: " + gameObject.name);
            StartCoroutine(FadeAndLoadNextScene());
        }
    }

    private IEnumerator FadeAndLoadNextScene()
    {
        Debug.Log("Fade coroutine START on: " + gameObject.name);

        if (fadeCanvasGroup == null)
        {
            Debug.LogError("fadeCanvasGroup is NULL");
            yield break;
        }

        isTransitioning = true;

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.transform.SetAsLastSibling();
        fadeCanvasGroup.alpha = 0f;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            Debug.Log("Fade alpha: " + fadeCanvasGroup.alpha);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
        Debug.Log("Fade coroutine END, loading next scene");

        yield return new WaitForSeconds(0.2f);

        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
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