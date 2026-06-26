using TMPro;
using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class HandDebugger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject landmarkPrefab;
    [SerializeField] private Camera mainCamera;

    [Header("Debug Settings")]
    [SerializeField] private float worldScale = 0.25f;
    [SerializeField] private bool invertY = true;
    [SerializeField] private bool faceCamera = true;

    private GameObject[] landmarkObjects = new GameObject[21];

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        for (int i = 0; i < 21; i++)
        {
            landmarkObjects[i] = Instantiate(landmarkPrefab, transform);
            landmarkObjects[i].name = $"Landmark_{i}";

            var text = landmarkObjects[i].GetComponentInChildren<TextMeshPro>();

            if (text != null)
            {
                text.text = i.ToString();
            }
        }
    }

    private void Update()
    {
        if (HandTrackingManager.Instance == null)
            return;

        if (!HandTrackingManager.Instance.HasHand)
        {
            foreach (var obj in landmarkObjects)
                obj.SetActive(false);

            return;
        }

        for (int i = 0; i < 21; i++)
        {
            landmarkObjects[i].SetActive(true);

            LandmarkData landmark = HandTrackingManager.Instance.GetWorldLandmark(i);

            if (landmark == null || !landmark.tracked)
                continue;

            Vector3 cameraSpace = landmark.worldPosition;

            if (invertY)
                cameraSpace.y = -cameraSpace.y;

            // Scale MediaPipe coordinates
            cameraSpace *= worldScale;

            // Convert from camera space to Unity world space
            Vector3 worldPosition = mainCamera.transform.TransformPoint(cameraSpace);

            landmarkObjects[i].transform.position = worldPosition;

            if (faceCamera)
            {
                landmarkObjects[i].transform.rotation =
                    Quaternion.LookRotation(
                        landmarkObjects[i].transform.position - mainCamera.transform.position
                    );
            }

            var label = landmarkObjects[i].GetComponentInChildren<TextMeshPro>();

            if (label != null)
            {
                label.text =
     $"{i}\n" +
     $"({landmark.worldPosition.x:F2}, " +
     $"{landmark.worldPosition.y:F2}, " +
     $"{landmark.worldPosition.z:F2})";
            }
        }
    }
}