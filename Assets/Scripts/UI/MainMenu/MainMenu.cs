using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] CanvasGroup background;
    [SerializeField] CanvasGroup content;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Sequence loadIn = DOTween.Sequence();
        loadIn.Append(content.DOFade(0f, 0f));
        loadIn.Append(background.DOFade(0f, 0f));
        loadIn.Append(background.DOFade(1f, 1f));
        loadIn.Insert(0f, content.DOFade(1f, 2f));
        loadIn.Play();
    }

    public void StartGame()
    {
        content.DOFade(0f, 1f);
        background.DOFade(0f, 1f).OnComplete(() => { SceneManager.LoadScene("Day1"); });
        // throw new System.NotImplementedException("mraow");
    }
}
