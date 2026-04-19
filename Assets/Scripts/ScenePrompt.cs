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

    private Coroutine animationRoutine;
    private int currentFrame = 0;

    private void OnEnable()
    {
        if (sprites.Count > 0 && promptImage != null)
        {
            animationRoutine = StartCoroutine(AnimatePrompt());
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
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
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