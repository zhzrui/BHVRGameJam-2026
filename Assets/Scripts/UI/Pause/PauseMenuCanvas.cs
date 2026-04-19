using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Rendering;

public class PauseMenu : MonoBehaviour
{
    // [SerializeField] CanvasGroup background;
    // [SerializeField] CanvasGroup content;
    CanvasGroup canvasGroup;

    InputAction cancel;
    [SerializeField] Volume volume;
    bool menuShown = false;

    void Awake()
    {
        cancel = InputSystem.actions.FindAction("Cancel");
        cancel.performed += Cancel;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Cancel(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (menuShown)
            {
                Hide();
            } else
            {
                Show();
            }
        }
    }

    public void Show()
    {
        if (!menuShown)
        {
            DOTween.Kill(canvasGroup);
            menuShown = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            Time.timeScale = 0f;
            DOTween.To(()=> volume.weight, x=> volume.weight = x, 1.0f, 1f).SetUpdate(true);
            canvasGroup.DOFade(1f, 1f).SetUpdate(true);
            // canvasGroup.DOFade(1f, 0.5f).OnComplete(() => { Time.timeScale = 0f; });
        }
    }

    public void Hide()
    {
        if (menuShown)
        {
            DOTween.Kill(canvasGroup);
            menuShown = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            DOTween.To(()=> volume.weight, x=> volume.weight = x, 0.0f, 1f).SetUpdate(true);
            canvasGroup.DOFade(0f, 1f).SetUpdate(true).OnComplete(() => { Time.timeScale = 1f; });
        }
    }

    public void Restart()
    {
        cancel.performed -= Cancel;
        DOTween.Kill(canvasGroup);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Menu()
    {
        cancel.performed -= Cancel;
        DOTween.Kill(canvasGroup);
        SceneManager.LoadScene("MainMenu");
    }
    
    void OnDestroy()
    {
        DOTween.Kill(canvasGroup);
        cancel.performed -= Cancel;
    }
}
