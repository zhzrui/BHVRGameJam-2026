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
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(true);
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
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning("Fade Canvas Group is not assigned.");

            int nextSceneNoFade = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneNoFade < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(nextSceneNoFade);
            else
                Debug.Log("No next scene in build settings.");

            yield break;
        }

        isTransitioning = true;
        fadeCanvasGroup.gameObject.SetActive(true);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

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