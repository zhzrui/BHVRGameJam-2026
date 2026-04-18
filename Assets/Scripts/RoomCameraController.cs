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
    public float transitionDuration = 0.45f;

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
        state = next;

        Vector3 startPos = transform.position;
        float startSize  = cam.orthographicSize;
        Vector3 endPos   = PositionFor(next);
        float endSize    = next == CameraState.Counter ? counterOrthoSize : roomOrthoSize;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);
            transform.position   = Vector3.Lerp(startPos, endPos, t);
            cam.orthographicSize = Mathf.Lerp(startSize, endSize, t);
            yield return null;
        }

        transform.position   = endPos;
        cam.orthographicSize = endSize;
        transitioning = false;
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
