using TMPro;
using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class HandDebugger : MonoBehaviour
{
    [SerializeField] private GameObject landmarkPrefab;
    [SerializeField] private Camera mainCamera;

    private GameObject[] landmarks = new GameObject[21];

    void Start()
    {
        for (int i = 0; i < 21; i++)
        {
            landmarks[i] = Instantiate(landmarkPrefab, transform);

            landmarks[i].name = $"Landmark_{i}";

            var text = landmarks[i].GetComponentInChildren<TextMeshPro>();

            if (text != null)
                text.text = i.ToString();
        }
    }

    void Update()
    {
        if (HandTrackingManager.Instance == null)
            return;

        if (!HandTrackingManager.Instance.HasHand)
            return;

        for (int i = 0; i < 21; i++)
        {
            Landmark lm = HandTrackingManager.Instance.GetWorldLandmark(0, i);

            Vector3 position = new Vector3(
                lm.x,
                -lm.y,
                lm.z
            );

            // Convert from camera space to Unity world space
            landmarks[i].transform.position =
                mainCamera.transform.TransformPoint(position);

            // Make the label face the camera
            landmarks[i].transform.LookAt(mainCamera.transform);
        }
    }
}