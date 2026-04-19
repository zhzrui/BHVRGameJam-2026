using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[System.Serializable]
public class CuttingIngredientData
{
    public Texture2D texture;
    public float[] cutPositions;
}

public class CuttingMinigame : CookingMinigameBase, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    //private void Start() => StartMinigame();

    [Header("Ingredients")]
    [SerializeField] private List<CuttingIngredientData> ingredients; // cut these in order

    [Header("Cut Settings")]
    [SerializeField] private RectTransform ingredientContainer;
    [SerializeField] private float cutLineTolerance = 20f;

    [Header("Visuals")]
    [SerializeField] private RectTransform cutLineIndicator;
    [SerializeField] private float sliceSeparation = 8f;

    [SerializeField] private string objectiveKey = "cooking";

    private int currentIngredientIndex;
    private int currentCutIndex;
    private bool isCutting;
    private float currentRightStart;
    private RawImage mainDisplay;
    private List<RawImage> completedSlices = new();

    // Shorthand for the active ingredient's data
    private CuttingIngredientData CurrentIngredient => ingredients[currentIngredientIndex];

    public override void StartMinigame()
    {
        currentIngredientIndex = 0;
        minigamePanel.SetActive(true);
        LoadIngredient();
        SFXManager.Instance?.Play("knife");
    }

    private void LoadIngredient()
    {
        currentCutIndex = 0;
        currentRightStart = 0f;
        isCutting = false;

        foreach (var slice in completedSlices)
            if (slice != null) Destroy(slice.gameObject);
        completedSlices.Clear();

        if (mainDisplay == null)
        {
            mainDisplay = new GameObject("IngredientMain").AddComponent<RawImage>();
            mainDisplay.transform.SetParent(ingredientContainer, false);
        }

        mainDisplay.gameObject.SetActive(true);
        mainDisplay.texture = CurrentIngredient.texture;
        CursorManager.Instance?.SetMinigameCursor(CursorManager.Instance.KnifeCursor);
        SetAnchoredSlice(mainDisplay.rectTransform, 0f, 1f, 0f);
        mainDisplay.uvRect = new Rect(0, 0, 1, 1);
        mainDisplay.transform.SetAsFirstSibling();

        RefreshCutLine();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentCutIndex >= CurrentIngredient.cutPositions.Length) return;

        float normX = PointerToNormalizedX(eventData.position, eventData.pressEventCamera);
        float tolerance = cutLineTolerance / ingredientContainer.rect.width;
        float cutX = CurrentIngredient.cutPositions[currentCutIndex];

        Debug.Log($"[Cutting] Click normX={normX:F2} cutX={cutX:F2} hit={Mathf.Abs(normX - cutX) <= tolerance}");

        if (Mathf.Abs(normX - cutX) <= tolerance)
            isCutting = true;
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isCutting)
            CompleteCut();
        isCutting = false;
    }

    private void CompleteCut()
    {
        SFXManager.Instance?.Play("cut");
        isCutting = false;

        float cutX = CurrentIngredient.cutPositions[currentCutIndex];
        int sliceNumber = completedSlices.Count + 1;

        RawImage slice = new GameObject($"Slice_{sliceNumber}").AddComponent<RawImage>();
        slice.transform.SetParent(ingredientContainer, false);
        slice.texture = CurrentIngredient.texture;
        slice.uvRect = new Rect(currentRightStart, 0, cutX - currentRightStart, 1);
        SetAnchoredSlice(slice.rectTransform, currentRightStart, cutX, 0f);
        completedSlices.Add(slice);
        cutLineIndicator?.SetAsLastSibling();

        for (int i = 0; i < completedSlices.Count; i++)
            SetAnchoredSlice(completedSlices[i].rectTransform,
                completedSlices[i].uvRect.x,
                completedSlices[i].uvRect.x + completedSlices[i].uvRect.width,
                -sliceSeparation * (completedSlices.Count - i));

        currentRightStart = cutX;
        mainDisplay.uvRect = new Rect(cutX, 0, 1f - cutX, 1);
        SetAnchoredSlice(mainDisplay.rectTransform, cutX, 1f, 0f);

        currentCutIndex++;
        Debug.Log($"[Cutting] Ingredient {currentIngredientIndex + 1}, cut {currentCutIndex}/{CurrentIngredient.cutPositions.Length}");

        if (currentCutIndex >= CurrentIngredient.cutPositions.Length)
            FinishIngredient();
        else
            RefreshCutLine();
    }

    private void FinishIngredient()
    {
        currentIngredientIndex++;
        Debug.Log($"[Cutting] Ingredient done. Next: {currentIngredientIndex}/{ingredients.Count}");

        if (currentIngredientIndex >= ingredients.Count)
        {
            Complete();
        }
        else
        {
            // Brief pause could be added here via coroutine if desired
            LoadIngredient();
        }
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

        if (!cutLineIndicator.TryGetComponent<Canvas>(out var lineCanvas))
        {
            lineCanvas = cutLineIndicator.gameObject.AddComponent<Canvas>();
            cutLineIndicator.gameObject.AddComponent<GraphicRaycaster>();
        }
        lineCanvas.overrideSorting = true;
        lineCanvas.sortingOrder = 10;

        float cutX = CurrentIngredient.cutPositions[currentCutIndex];
        cutLineIndicator.anchorMin = new Vector2(cutX, 0f);
        cutLineIndicator.anchorMax = new Vector2(cutX, 1f);
        cutLineIndicator.sizeDelta = new Vector2(4f, 0f);
        cutLineIndicator.anchoredPosition = Vector2.zero;
        cutLineIndicator.gameObject.SetActive(true);
    }

    private void Complete()
    {
        cutLineIndicator?.gameObject.SetActive(false);

        foreach (var slice in completedSlices)
            if (slice != null) Destroy(slice.gameObject);
        completedSlices.Clear();
        mainDisplay?.gameObject.SetActive(false);
        CursorManager.Instance?.ClearMinigameCursor();

        minigamePanel?.SetActive(false);

        DialogueManager dm = FindFirstObjectByType<DialogueManager>();
        dm?.MarkObjectiveComplete(objectiveKey);

        RaiseSuccess();
    }
}
