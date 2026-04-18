using UnityEngine;
using System;

public abstract class CookingMinigameBase : MonoBehaviour
{
    [SerializeField] protected GameObject minigamePanel;

    public event Action OnSuccess;
    public event Action OnFail;

    public abstract void StartMinigame();
    public void HidePanel() { if (minigamePanel != null) minigamePanel.SetActive(false); }

    protected void RaiseSuccess() => OnSuccess?.Invoke();
    protected void RaiseFail()    => OnFail?.Invoke();
}
