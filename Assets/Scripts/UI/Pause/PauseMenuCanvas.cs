using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    // [SerializeField] CanvasGroup background;
    // [SerializeField] CanvasGroup content;
    CanvasGroup canvasGroup;

    InputAction cancel;
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
            menuShown = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.DOFade(1f, 0.5f);
            // canvasGroup.DOFade(1f, 0.5f).OnComplete(() => { Time.timeScale = 0f; }); removed cuz it doesn't really work
        }
    }

    public void Hide()
    {
        if (menuShown)
        {
            menuShown = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.DOFade(0f, 0.5f);
            // Time.timeScale = 1f;
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Menu()
    {
        throw new System.NotImplementedException("meow");
    }
    
    void OnDestroy()
    {
        cancel.performed -= Cancel;
    }
}
