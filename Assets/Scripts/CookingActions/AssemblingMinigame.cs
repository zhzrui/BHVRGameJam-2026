using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class AssemblingMinigame : MonoBehaviour, IDropHandler
{
    private void Start() => StartMinigame();

    [Header("Setup")]
    [SerializeField] private List<DraggableIngredient> ingredientsInOrder; // assign in Inspector, top-to-bottom = correct order
    [SerializeField] private Transform bowlSlotParent; // where placed ingredients visually stack
    [SerializeField] private GameObject minigamePanel;

    public event Action OnSuccess;
    public event Action OnWrongOrder; // optional: hook up a shake/sound effect

    private int currentStep = 0;

    public void StartMinigame()
    {
        currentStep = 0;
        minigamePanel.SetActive(true);

        foreach (var ingredient in ingredientsInOrder)
            ingredient.gameObject.SetActive(true);
    }

    // Called by Unity's event system when something is dropped onto this GameObject
    public void OnDrop(PointerEventData eventData)
    {
        DraggableIngredient dropped = eventData.pointerDrag?.GetComponent<DraggableIngredient>();
        if (dropped == null) return;

        if (dropped.orderIndex == currentStep)
        {
            dropped.gameObject.SetActive(false);
            currentStep++;

            if (currentStep >= ingredientsInOrder.Count)
                Complete();
        }
        else
        {
            // Wrong order — ingredient's OnEndDrag will snap it back
            OnWrongOrder?.Invoke();
        }
    }

    private void Complete()
    {
        minigamePanel.SetActive(false);
        OnSuccess?.Invoke();
    }
}
