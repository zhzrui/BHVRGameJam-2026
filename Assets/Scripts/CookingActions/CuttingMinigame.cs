using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CuttingMinigame : CookingMinigameBase, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    //private void Start() => StartMinigame();

    [Header("Ingredient")]
    [SerializeField] private Texture2D ingredientTexture;
    [SerializeField] private RectTransform ingredientContainer;

    [Header("Cut Settings")]
    [SerializeField] private float[] cutPositions = { 0.33f, 0.66f };
    [SerializeField] private float cutLineTolerance = 20f;
    [SerializeField] private float requiredCutDistance = 80f;

    [Header("Visuals")]
    [SerializeField] private RectTransform cutLineIndicator;
    [SerializeField] private float sliceSeparation = 8f;

    [SerializeField] private string objectiveKey = "cooking";

    private int currentCutIndex;
    private bool isCutting;
    private float cuttingProgress;
    private float currentRightStart;
    private RawImage mainDisplay;
    private List<RawImage> completedSlices = new();

    public override void StartMinigame()
    {
        currentCutIndex = 0;
        currentRightStart = 0f;
        isCutting = false;
        cuttingProgress = 0f;

        foreach (var slice in completedSlices)
            if (slice != null) Destroy(slice.gameObject);
        completedSlices.Clear();

        if (mainDisplay == null)
        {
            mainDisplay = new GameObject("IngredientMain").AddComponent<RawImage>();
            mainDisplay.transform.SetParent(ingredientContainer, false);
        }

        mainDisplay.texture = ingredientTexture;
        SetAnchoredSlice(mainDisplay.rectTransform, 0f, 1f, 0f);
        mainDisplay.uvRect = new Rect(0, 0, 1, 1);
        mainDisplay.transform.SetAsFirstSibling(); // always render behind slices and cut line

        minigamePanel.SetActive(true);
        RefreshCutLine();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentCutIndex >= cutPositions.Length) return;

        float normX = PointerToNormalizedX(eventData.position, eventData.pressEventCamera);
        float tolerance = cutLineTolerance / ingredientContainer.rect.width;

        if (Mathf.Abs(normX - cutPositions[currentCutIndex]) <= tolerance)
        {
            isCutting = true;
            cuttingProgress = 0f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isCutting) return;

        cuttingProgress += Mathf.Abs(eventData.delta.y);
        if (cuttingProgress >= requiredCutDistance)
            CompleteCut();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isCutting = false;
        cuttingProgress = 0f;
    }

    private void CompleteCut()
    {
        isCutting = false;

        float cutX = cutPositions[currentCutIndex];
        int sliceNumber = completedSlices.Count + 1;

        RawImage slice = new GameObject($"Slice_{sliceNumber}").AddComponent<RawImage>();
        slice.transform.SetParent(ingredientContainer, false);
        slice.texture = ingredientTexture;
        slice.uvRect = new Rect(currentRightStart, 0, cutX - currentRightStart, 1);
        SetAnchoredSlice(slice.rectTransform, currentRightStart, cutX, 0f);
        completedSlices.Add(slice);
        cutLineIndicator?.SetAsLastSibling(); // keep cut line on top after each new slice

        for (int i = 0; i < completedSlices.Count; i++)
            SetAnchoredSlice(completedSlices[i].rectTransform,
                completedSlices[i].uvRect.x,
                completedSlices[i].uvRect.x + completedSlices[i].uvRect.width,
                -sliceSeparation * (completedSlices.Count - i));

        currentRightStart = cutX;
        mainDisplay.uvRect = new Rect(cutX, 0, 1f - cutX, 1);
        SetAnchoredSlice(mainDisplay.rectTransform, cutX, 1f, 0f);

        currentCutIndex++;

        if (currentCutIndex >= cutPositions.Length)
            Complete();
        else
            RefreshCutLine();
    }

    private void SetAnchoredSlice(RectTransform rt, float fromNorm, float toNorm, float xPixels)
    {
        rt.anchorMin = new Vector2(fromNorm, 0f);
        rt.anchorMax = new Vector2(toNorm, 1f);
        rt.offsetMin = new Vector2(xPixels, 0f);
        rt.offsetMax = new Vector2(xPixels, 0f);
    }

    private float PointerToNormalizedX(Vector2 screenPos, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ingredientContainer, screenPos, cam, out Vector2 local);
        float width = ingredientContainer.rect.width;
        return (local.x + width * 0.5f) / width;
    }

    private void RefreshCutLine()
    {
        if (cutLineIndicator == null) return;

        if (currentCutIndex >= cutPositions.Length)
        {
            cutLineIndicator.gameObject.SetActive(false);
            return;
        }

        // Add an overriding Canvas so the cut line always sorts above the ingredient image
        if (!cutLineIndicator.TryGetComponent<Canvas>(out var lineCanvas))
        {
            lineCanvas = cutLineIndicator.gameObject.AddComponent<Canvas>();
            cutLineIndicator.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        lineCanvas.overrideSorting = true;
        lineCanvas.sortingOrder = 10;

        float cutX = cutPositions[currentCutIndex];
        cutLineIndicator.anchorMin = new Vector2(cutX, 0f);
        cutLineIndicator.anchorMax = new Vector2(cutX, 1f);
        cutLineIndicator.sizeDelta = new Vector2(4f, 0f);
        cutLineIndicator.anchoredPosition = Vector2.zero;
        cutLineIndicator.gameObject.SetActive(true);
    }

    private void Complete()
    {
        cutLineIndicator?.gameObject.SetActive(false);

        // Destroy dynamically created slice images so they don't persist if ingredientContainer
        // is outside minigamePanel
        foreach (var slice in completedSlices)
            if (slice != null) Destroy(slice.gameObject);
        completedSlices.Clear();
        mainDisplay?.gameObject.SetActive(false);

        minigamePanel?.SetActive(false);
        ObjectiveManager.Instance?.CompleteObjective(objectiveKey);
        RaiseSuccess();
    }
}
