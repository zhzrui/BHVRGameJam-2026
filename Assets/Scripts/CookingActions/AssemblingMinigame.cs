using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class AssemblingMinigame : CookingMinigameBase, IDropHandler
{
    //private void Start() => StartMinigame();

    [Header("Setup")]
    [SerializeField] private List<DraggableIngredient> ingredientsInOrder;
    [SerializeField] private Transform bowlSlotParent;

    [SerializeField] private string objectiveKey = "cooking";

    public event System.Action OnWrongOrder;

    private int currentStep = 0;

    public override void StartMinigame()
    {
        currentStep = 0;
        minigamePanel.SetActive(true);

        foreach (var ingredient in ingredientsInOrder)
            ingredient.gameObject.SetActive(true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableIngredient dropped = eventData.pointerDrag?.GetComponent<DraggableIngredient>();
        if (dropped == null) { Debug.Log("[Assembling] Drop rejected: no DraggableIngredient found"); return; }

        Debug.Log($"[Assembling] Dropped orderIndex={dropped.orderIndex}, expecting step={currentStep}, total={ingredientsInOrder.Count}");

        if (dropped.orderIndex == currentStep)
        {
            dropped.gameObject.SetActive(false);
            SpawnInBowl(dropped);
            currentStep++;

            if (currentStep >= ingredientsInOrder.Count)
                Complete();
        }
        else
        {
            Debug.Log($"[Assembling] Wrong order — snapping back");
            OnWrongOrder?.Invoke();
        }
    }

    private void SpawnInBowl(DraggableIngredient dropped)
    {
        if (bowlSlotParent == null || dropped.bowlSprite == null) return;

        GameObject icon = new GameObject(dropped.bowlSprite.name);
        icon.transform.SetParent(bowlSlotParent, false);
        Image img = icon.AddComponent<Image>();
        img.sprite = dropped.bowlSprite;
        img.SetNativeSize();
    }

    private void Complete()
    {
        Debug.Log("[Assembling] Complete! Hiding panel and raising success.");
        if (minigamePanel != null) minigamePanel.SetActive(false);
        else Debug.LogWarning("[Assembling] minigamePanel is not assigned!");
        //ObjectiveManager.Instance?.CompleteObjective(objectiveKey);

        DialogueManager dm = FindFirstObjectByType<DialogueManager>();
        if (dm != null)
        {
            dm.MarkObjectiveComplete(objectiveKey);
        }

        RaiseSuccess();
    }
}
