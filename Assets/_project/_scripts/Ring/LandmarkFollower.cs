using UnityEngine;

public class LandmarkFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Ring Position")]
    [SerializeField] private int startLandmark = 13;     // Ring finger MCP
    [SerializeField] private int endLandmark = 14;       // Ring finger PIP

    [Header("Hand Scale")]
    [SerializeField] private int handScaleStart = 5;     // Index MCP
    [SerializeField] private int handScaleEnd = 17;      // Pinky MCP

    [Header("Movement")]
    [SerializeField] private float smoothSpeed = 15f;

    [Header("Rotation")]
    [SerializeField] private Vector3 rotationOffset;

    [Header("Scale")]
    [SerializeField] private float referenceHandWidth = 0.25f;
    [SerializeField] private float scaleMultiplier = 1.0f;
    [SerializeField] private float scaleSmoothSpeed = 10f;

    [Header("HandSelection")]
    [SerializeField] private int toggleHand = 1;

    private Vector3 originalScale;
    private int fingerState = 0;

    private void Start()
    {
        originalScale = transform.localScale;

        // Fix 3: On mobile, lower smooth speeds reduce jitter caused by
        // GPU throttling, heat, and lower MediaPipe frame rates.
#if UNITY_ANDROID || UNITY_IOS
        smoothSpeed = 8f;
        scaleSmoothSpeed = 5f;
#endif
    }


    public void setFinger()
    {
        if (fingerState == 2)
        {
           fingerState = 0;
        }
        else
        {
            fingerState +=1;
        }
        switch (fingerState)
        {
            case 0:
                startLandmark = 14;     // Ring finger MCP
                endLandmark = 15;
                break;
            case 1:
                startLandmark = 10;
                endLandmark = 11;
                break;
            case 2:
                startLandmark = 3; endLandmark = 4;
                break;

        }

        Debug.Log("FingerState" + fingerState);
        Debug.Log("Landmarks :"+startLandmark + " " + endLandmark);

    }

    private void Update()
    {
        if (HandTrackingManager.Instance == null)
            return;

        if (!HandTrackingManager.Instance.HasHand)
            return;

        //----------------------------
        // Ring Landmarks
        //----------------------------

        Vector3 start = HandTrackingManager.Instance.GetLandmark(startLandmark);
        Vector3 end = HandTrackingManager.Instance.GetLandmark(endLandmark);

        //----------------------------
        // Hand Scale Landmarks
        //----------------------------

        Vector3 handStart = HandTrackingManager.Instance.GetLandmark(handScaleStart);
        Vector3 handEnd = HandTrackingManager.Instance.GetLandmark(handScaleEnd);

        //----------------------------
        // Convert Ring Landmarks
        //----------------------------

        // Fix 1: Use a physically correct camera depth instead of the hardcoded
        // z=0.5f that caused the ring to jump when the phone was tilted on mobile.
        // nearClipPlane + 0.3f places the ring just in front of the near plane
        // regardless of camera orientation.
        float renderDepth = mainCamera.nearClipPlane + 0.3f;

        Vector3 startScreen = new Vector3(
            start.x * Screen.width,
            (1f - start.y) * Screen.height,
            renderDepth);

        Vector3 endScreen = new Vector3(
            end.x * Screen.width,
            (1f - end.y) * Screen.height,
            renderDepth);

        Vector3 startWorld = mainCamera.ScreenToWorldPoint(startScreen);
        Vector3 endWorld = mainCamera.ScreenToWorldPoint(endScreen);

        //----------------------------
        // Position (35% along finger)
        //----------------------------

        Vector3 targetPosition = Vector3.Lerp(startWorld, endWorld, 0.35f);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * smoothSpeed);

        //----------------------------
        // Rotation
        //----------------------------

        Vector3 direction = endWorld - startWorld;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            // rotationOffset.y = toggleHand * rotationOffset.y;
            targetRotation *= Quaternion.Euler(rotationOffset);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * smoothSpeed);
        }

        //----------------------------
        // Hand Scale
        //----------------------------

        Vector3 handStartScreen = new Vector3(
            handStart.x * Screen.width,
            (1f - handStart.y) * Screen.height,
            renderDepth);

        Vector3 handEndScreen = new Vector3(
            handEnd.x * Screen.width,
            (1f - handEnd.y) * Screen.height,
            renderDepth);

        // Fix 2: Normalize hand width against Screen.dpi so the scale calculation
        // is consistent across high-DPI mobile screens and laptop displays.
        float dpiNormalized = Screen.dpi > 0 ? Screen.dpi / 160f : 1f;
        float currentHandWidth =
            Vector2.Distance(handStartScreen, handEndScreen) / (Screen.width * dpiNormalized);

        float scaleFactor =
            (currentHandWidth / referenceHandWidth) * scaleMultiplier;

        Vector3 targetScale = originalScale * scaleFactor;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSmoothSpeed);
    }
}