using UnityEngine;

public class LandmarkFollower : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private int landmarkIndex = 13;

    void Update()
    {
        if (HandTrackingManager.Instance == null)
            return;

        if (!HandTrackingManager.Instance.HasHand)
            return;

        Vector3 landmark = HandTrackingManager.Instance.GetLandmark(landmarkIndex);

        // Convert normalized coordinates to screen pixels
        Vector3 screenPoint = new Vector3(
            landmark.x * Screen.width,
            (1f - landmark.y) * Screen.height,
            0.5f // Distance from the camera
        );

        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);

        transform.position = worldPoint;
    }
}