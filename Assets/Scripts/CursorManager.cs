using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] private int cursorSize = 333; // pixel size to display all cursors at

    private Texture2D pointFinger;
    private Texture2D closeHand;
    private Texture2D knife;
    private Texture2D spoon;

    private bool minigameCursorActive;
    private Texture2D currentCursor;

    public Texture2D KnifeCursor => knife;
    public Texture2D SpoonCursor => spoon;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        pointFinger = Scale(Resources.Load<Texture2D>("kitchen/pointfinger"));
        closeHand = Scale(Resources.Load<Texture2D>("kitchen/closedhand"));
        knife = Scale(Resources.Load<Texture2D>("kitchen/knife"));
        spoon = Scale(Resources.Load<Texture2D>("kitchen/spoon"));

        ApplyCursor(pointFinger);
    }

    private Texture2D Scale(Texture2D source)
    {
        if (source == null) return null;
        RenderTexture rt = RenderTexture.GetTemporary(cursorSize, cursorSize);
        Graphics.Blit(source, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D result = new Texture2D(cursorSize, cursorSize, TextureFormat.ARGB32, false);
        result.ReadPixels(new Rect(0, 0, cursorSize, cursorSize), 0, 0);
        result.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    private void Update()
    {
        if (minigameCursorActive) return;

        Texture2D desired = Mouse.current.leftButton.isPressed ? closeHand : pointFinger;
        if (desired != currentCursor)
            ApplyCursor(desired);
    }

    // Call from a minigame's StartMinigame to lock the cursor appearance
    public void SetMinigameCursor(Texture2D cursor)
    {
        minigameCursorActive = true;
        ApplyCursor(cursor);
    }

    // Call from a minigame's Complete to return to normal cursor
    public void ClearMinigameCursor()
    {
        minigameCursorActive = false;
        ApplyCursor(pointFinger);
    }

    private void ApplyCursor(Texture2D tex)
    {
        currentCursor = tex;
        Cursor.SetCursor(tex, Vector2.zero, CursorMode.ForceSoftware);
    }
}
