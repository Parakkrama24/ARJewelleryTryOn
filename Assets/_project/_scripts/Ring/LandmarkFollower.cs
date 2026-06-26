using UnityEngine;

public class LandmarkFollower : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private int landmarkIndex = 13;

    [Header("World Settings")]
    [SerializeField] private float worldScale = 0.25f;
    [SerializeField] private bool invertY = true;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (HandTrackingManager.Instance == null)
            return;

        if (!HandTrackingManager.Instance.HasHand)
            return;

        LandmarkData landmark =
            HandTrackingManager.Instance.GetWorldLandmark(landmarkIndex);

        if (landmark == null || !landmark.tracked)
            return;

        // MediaPipe world coordinates
        Vector3 cameraSpace = landmark.worldPosition;

        // Convert to Unity camera space
        if (invertY)
            cameraSpace.y = -cameraSpace.y;

        // Scale coordinates
        cameraSpace *= worldScale;

        // Convert to Unity world coordinates
        Vector3 worldPosition =
            mainCamera.transform.TransformPoint(cameraSpace);

        transform.position = worldPosition;
    }
}