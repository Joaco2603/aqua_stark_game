using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraStackManager : MonoBehaviour
{
    [Tooltip("If true: move10 units forward and then10 units back. If false: alternate between original and forward position each trigger.")]
    public bool returnMode = true;

    [Tooltip("Distance in units to move the camera forward.")]
    public float distance =10f;

    [Tooltip("Time in seconds for the move (one way).")]
    public float duration =0.5f;

    // If false, each trigger will alternate between original and moved position.
    private bool isMovedForward = false;
    private bool isMoving = false;
    private Vector3 originalPosition;

    void Start()
    {
        if (Camera.main != null)
        {
            originalPosition = Camera.main.transform.position;
        }
        else
        {
            Debug.LogWarning("CameraStackManager: No Camera.main found. Attach this script to the main camera or ensure a camera has the MainCamera tag.");
            originalPosition = transform.position;
        }
    }

    // Public method to trigger the movement
    public void TriggerMove()
    {
        if (isMoving) return;

        if (returnMode)
        {
            StartCoroutine(MoveForwardAndBack());
        }
        else
        {
            StartCoroutine(ToggleMove());
        }
    }

    private IEnumerator MoveForwardAndBack()
    {
        isMoving = true;

        Transform cam = GetCameraTransform();
        Vector3 start = cam.position;
        Vector3 target = start + cam.forward * distance;

        yield return StartCoroutine(SmoothMove(cam, start, target, duration));
        yield return StartCoroutine(SmoothMove(cam, target, start, duration));

        isMoving = false;
    }

    private IEnumerator ToggleMove()
    {
        isMoving = true;

        Transform cam = GetCameraTransform();
        Vector3 start = cam.position;
        Vector3 target;

        if (!isMovedForward)
        {
            target = originalPosition + cam.forward * distance;
            yield return StartCoroutine(SmoothMove(cam, start, target, duration));
            isMovedForward = true;
        }
        else
        {
            yield return StartCoroutine(SmoothMove(cam, start, originalPosition, duration));
            isMovedForward = false;
        }

        isMoving = false;
    }

    private IEnumerator SmoothMove(Transform cam, Vector3 from, Vector3 to, float time)
    {
        if (time <=0f)
        {
            cam.position = to;
            yield break;
        }

        float elapsed =0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            cam.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f,1f, t));
            yield return null;
        }

        cam.position = to;
    }

    private Transform GetCameraTransform()
    {
        if (Camera.main != null) return Camera.main.transform;
        return transform;
    }
}
