using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoomCameraController : MonoBehaviour
{
    public enum CameraState { Counter, Room }

    [Header("Counter Screen")]
    public Vector3 counterPosition = new Vector3(0f, 0f, -10f);
    public float counterOrthoSize = 3f;

    [Header("Room Panorama")]
    [Tooltip("Drag the room SpriteRenderer here — two clones are auto-created at runtime for seamless wrap")]
    public SpriteRenderer roomSprite;
    public float roomOrthoSize = 6f;
    public float panSpeed = 10f;
    public GameObject roomUI;

    [Header("Transition")]
    public float fadeDuration = 0.25f;
    [Tooltip("CanvasGroup on a full-screen black Image used for the fade")]
    public CanvasGroup fadeOverlay;

    [Header("Counter-Only Objects")]
    [Tooltip("GameObjects to hide when leaving the counter (e.g. the cooking Canvas)")]
    public GameObject[] counterObjects;

    private Camera cam;
    private CameraState state = CameraState.Counter;
    private bool transitioning = false;

    private float roomY;
    private float roomZ;
    private float spriteWidth;
    // The treadmill anchor: camera X stays within [anchorX - halfWidth, anchorX + halfWidth]
    // and teleports by spriteWidth when it crosses either boundary.
    private float anchorX;

    public bool IsInRoomView => state == CameraState.Room;

    void Awake()
    {
        cam = GetComponent<Camera>();
        transform.position = counterPosition;
        cam.orthographicSize = counterOrthoSize;
    }

    void Start()
    {
        if (roomSprite != null)
            AutoFitRoom();
    }

    void AutoFitRoom()
    {
        Bounds b = roomSprite.bounds;
        roomY = b.center.y;
        roomZ = counterPosition.z;
        roomOrthoSize = b.size.y / 2f;
        spriteWidth = b.size.x;
        anchorX = b.center.x;

        // Place two clones flanking the original so the wrap is always covered
        CreateClone(roomSprite, anchorX - spriteWidth, roomY);
        CreateClone(roomSprite, anchorX + spriteWidth, roomY);
    }

    void CreateClone(SpriteRenderer source, float x, float y)
    {
        GameObject clone = new GameObject("RoomClone");
        clone.transform.position = new Vector3(x, y, source.transform.position.z);
        clone.transform.localScale = source.transform.localScale;
        SpriteRenderer sr = clone.AddComponent<SpriteRenderer>();
        sr.sprite = source.sprite;
        sr.material = source.material;
        sr.sortingLayerID = source.sortingLayerID;
        sr.sortingOrder = source.sortingOrder;
    }

    void Update()
    {
        if (transitioning) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (state == CameraState.Counter)
        {
            if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
                StartCoroutine(TransitionToRoom());
        }
        else
        {
            if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)
                StartCoroutine(TransitionToCounter());

            float dir = 0f;
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed)  dir -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) dir += 1f;

            if (dir != 0f)
            {
                Vector3 pos = transform.position;
                pos.x += dir * panSpeed * Time.deltaTime;

                // Treadmill: silently teleport by one sprite width when crossing boundary
                if (pos.x > anchorX + spriteWidth * 0.5f) pos.x -= spriteWidth;
                else if (pos.x < anchorX - spriteWidth * 0.5f) pos.x += spriteWidth;

                transform.position = pos;
            }
        }
    }

    IEnumerator TransitionToRoom()
    {
        transitioning = true;
        yield return StartCoroutine(Fade(0f, 1f));

        state = CameraState.Room;
        cam.orthographicSize = roomOrthoSize;
        // Enter at the center of the panorama
        transform.position = new Vector3(anchorX, roomY, roomZ);

        foreach (var obj in counterObjects) obj?.SetActive(false);
        roomUI.SetActive(true);

        yield return StartCoroutine(Fade(1f, 0f));
        transitioning = false;
    }

    IEnumerator TransitionToCounter()
    {
        transitioning = true;
        yield return StartCoroutine(Fade(0f, 1f));

        state = CameraState.Counter;
        cam.orthographicSize = counterOrthoSize;
        transform.position = counterPosition;

        foreach (var obj in counterObjects) obj?.SetActive(true);
        roomUI.SetActive(false);

        yield return StartCoroutine(Fade(1f, 0f));
        transitioning = false;
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadeOverlay == null) yield break;

        fadeOverlay.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = to;
        if (to == 0f) fadeOverlay.gameObject.SetActive(false);
    }
}
