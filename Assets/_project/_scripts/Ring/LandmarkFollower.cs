using UnityEngine;

public class LandmarkFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Finger Landmarks")]
    [SerializeField] private int startLandmark = 13;
    [SerializeField] private int endLandmark = 14;

    [Header("Movement")]
    [SerializeField] private float smoothSpeed = 15f;

    [Header("Rotation")]
    [SerializeField] private Vector3 rotationOffset;

    private void Update()
    {
        if (HandTrackingManager.Instance == null)
            return;

        if (!HandTrackingManager.Instance.HasHand)
            return;

        // Get normalized landmarks
        Vector3 start = HandTrackingManager.Instance.GetLandmark(startLandmark);
        Vector3 end = HandTrackingManager.Instance.GetLandmark(endLandmark);

        // Convert start landmark to screen space
        Vector3 startScreen = new Vector3(
            start.x * Screen.width,
            (1f - start.y) * Screen.height,
            0.5f);

        // Convert end landmark to screen space
        Vector3 endScreen = new Vector3(
            end.x * Screen.width,
            (1f - end.y) * Screen.height,
            0.5f);

        // Convert to world space
        Vector3 startWorld = mainCamera.ScreenToWorldPoint(startScreen);
        Vector3 endWorld = mainCamera.ScreenToWorldPoint(endScreen);

        // -----------------------------
        // Smooth Position
        // -----------------------------
        transform.position = Vector3.Lerp(
            transform.position,
            startWorld,
            Time.deltaTime * smoothSpeed);

        // -----------------------------
        // Finger Direction
        // -----------------------------
        Vector3 direction = endWorld - startWorld;

        if (direction.sqrMagnitude > 0.00001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            targetRotation *= Quaternion.Euler(rotationOffset);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * smoothSpeed);
        }
    }
}