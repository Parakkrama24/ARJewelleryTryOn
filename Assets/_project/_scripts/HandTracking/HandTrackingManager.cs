using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class HandTrackingManager : MonoBehaviour
{
    public static HandTrackingManager Instance { get; private set; }

    public HandLandmarkerResult CurrentResult { get; private set; }

    public bool HasHand =>
        CurrentResult.handLandmarks != null &&
        CurrentResult.handLandmarks.Count > 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UpdateResult(HandLandmarkerResult result)
    {
        CurrentResult = result;
    }

    public Vector3 GetLandmark(int landmarkIndex)
    {
        if (!HasHand)
            return Vector3.zero;

        var landmark = CurrentResult.handLandmarks[0].landmarks[landmarkIndex];

        return new Vector3(
            landmark.x,
            landmark.y,
            landmark.z
        );
    }

    public Landmark GetWorldLandmark(int handIndex, int landmarkIndex)
    {
        if (!HasHand)
            return default;

        return CurrentResult.handWorldLandmarks[handIndex].landmarks[landmarkIndex];
    }
}

