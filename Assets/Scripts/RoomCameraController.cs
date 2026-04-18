using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoomCameraController : MonoBehaviour
{
    public enum CameraState { Counter, RoomCenter, RoomLeft, RoomRight }

    [Header("Counter Screen")]
    public Vector3 counterPosition = new Vector3(0f, 0f, -10f);
    public float counterOrthoSize = 3f;

    [Header("Room Screens")]
    public Vector3 roomCenterPosition = new Vector3(0f, 0f, -10f);
    public Vector3 roomLeftPosition   = new Vector3(-8f, 0f, -10f);
    public Vector3 roomRightPosition  = new Vector3( 8f, 0f, -10f);
    public float roomOrthoSize = 6f;

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

    public bool IsInRoomView => state != CameraState.Counter;

    void Awake()
    {
        cam = GetComponent<Camera>();
        transform.position = counterPosition;
        cam.orthographicSize = counterOrthoSize;
    }

    void Update()
    {
        if (transitioning) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        switch (state)
        {
            case CameraState.Counter:
                if (kb.upArrowKey.wasPressedThisFrame)
                    StartCoroutine(TransitionTo(CameraState.RoomCenter));
                break;

            case CameraState.RoomCenter:
                if (kb.downArrowKey.wasPressedThisFrame)
                    StartCoroutine(TransitionTo(CameraState.Counter));
                else if (kb.leftArrowKey.wasPressedThisFrame)
                    StartCoroutine(TransitionTo(CameraState.RoomLeft));
                else if (kb.rightArrowKey.wasPressedThisFrame)
                    StartCoroutine(TransitionTo(CameraState.RoomRight));
                break;

            case CameraState.RoomLeft:
                if (kb.rightArrowKey.wasPressedThisFrame)
                    StartCoroutine(TransitionTo(CameraState.RoomCenter));
                break;

            case CameraState.RoomRight:
                if (kb.leftArrowKey.wasPressedThisFrame)
                    StartCoroutine(TransitionTo(CameraState.RoomCenter));
                break;
        }
    }

    IEnumerator TransitionTo(CameraState next)
    {
        transitioning = true;

        yield return StartCoroutine(Fade(0f, 1f));

        state = next;
        transform.position   = PositionFor(next);
        cam.orthographicSize = next == CameraState.Counter ? counterOrthoSize : roomOrthoSize;

        bool atCounter = next == CameraState.Counter;
        foreach (var obj in counterObjects)
            obj?.SetActive(atCounter);

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

    Vector3 PositionFor(CameraState s) => s switch
    {
        CameraState.Counter    => counterPosition,
        CameraState.RoomCenter => roomCenterPosition,
        CameraState.RoomLeft   => roomLeftPosition,
        CameraState.RoomRight  => roomRightPosition,
        _                      => counterPosition
    };
}
