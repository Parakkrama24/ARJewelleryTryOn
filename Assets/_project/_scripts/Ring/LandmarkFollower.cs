using UnityEngine;

public class LandmarkFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Landmark")]
    [Range(0, 20)]
    [SerializeField] private int landmarkIndex = 14;

    [Header("Debug")]
    [SerializeField] private float distanceFromCamera = 0.35f;
    [SerializeField] private float scaleMultiplier = 0.20f;
    [SerializeField] private bool mirrorY = true;

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

        Vector3 mp = landmark.worldPosition;

        // Convert MediaPipe coordinate system to Unity camera space
        Vector3 localPosition = new Vector3(
            mp.x,
            mirrorY ? -mp.y : mp.y,
            distanceFromCamera + (mp.z * scaleMultiplier)
        );

        // Place object relative to the AR camera
        transform.position = mainCamera.transform.TransformPoint(localPosition);

        // Always face the camera
        transform.rotation = Quaternion.LookRotation(
            transform.position - mainCamera.transform.position
        );
    }
}