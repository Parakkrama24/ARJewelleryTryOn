using Mediapipe.Tasks.Vision.PoseLandmarker;
using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class PoseTrackingManager : MonoBehaviour
{
    public static PoseTrackingManager Instance { get; private set; }

    public PoseLandmarkerResult CurrentResult { get; private set; }

    public bool HasPose =>
        CurrentResult.poseLandmarks != null &&
        CurrentResult.poseLandmarks.Count > 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UpdateResult(PoseLandmarkerResult result)
    {
        CurrentResult = result;
    }

    public Vector3 GetLandmark(int landmarkIndex)
    {
        if (!HasPose)
            return Vector3.zero;

        var landmark = CurrentResult.poseLandmarks[0].landmarks[landmarkIndex];

        return new Vector3(
            landmark.x,
            landmark.y,
            landmark.z
        );
    }

    public Landmark GetWorldLandmark(int poseIndex, int landmarkIndex)
    {
        if (!HasPose)
            return default;

        return CurrentResult.poseWorldLandmarks[poseIndex].landmarks[landmarkIndex];
    }
}