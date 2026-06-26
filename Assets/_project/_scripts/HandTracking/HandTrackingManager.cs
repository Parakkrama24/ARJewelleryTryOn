using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;

public class HandTrackingManager : MonoBehaviour
{
    public static HandTrackingManager Instance { get; private set; }

    public LandmarkData[] WorldLandmarks { get; private set; }

    public HandLandmarkerResult CurrentResult { get; private set; }

    public bool HasHand { get; private set; }

    public bool IsLeftHand { get; private set; }

    public bool IsRightHand { get; private set; }

    private const int LandmarkCount = 21;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        WorldLandmarks = new LandmarkData[LandmarkCount];

        for (int i = 0; i < LandmarkCount; i++)
        {
            WorldLandmarks[i] = new LandmarkData
            {
                index = i,
                tracked = false,
                worldPosition = Vector3.zero
            };
        }
    }

    public void UpdateResult(HandLandmarkerResult result)
    {
        CurrentResult = result;

        HasHand =
            result.handLandmarks != null &&
            result.handLandmarks.Count > 0 &&
            result.handWorldLandmarks != null &&
            result.handWorldLandmarks.Count > 0;

        if (!HasHand)
        {
            ResetTracking();
            return;
        }

        //-----------------------------
        // Handedness
        //-----------------------------
        IsLeftHand = false;
        IsRightHand = false;

        if (result.handedness != null &&
            result.handedness.Count > 0 &&
            result.handedness[0].categories.Count > 0)
        {
            string label = result.handedness[0].categories[0].categoryName;

            IsLeftHand = label == "Left";
            IsRightHand = label == "Right";
        }

        //-----------------------------
        // World Landmarks
        //-----------------------------
        var hand = result.handWorldLandmarks[0];

        for (int i = 0; i < LandmarkCount; i++)
        {
            Landmark lm = hand.landmarks[i];

            WorldLandmarks[i].tracked = true;

            WorldLandmarks[i].worldPosition = new Vector3(
                lm.x,
                lm.y,
                lm.z
            );
        }
    }

    private void ResetTracking()
    {
        HasHand = false;

        IsLeftHand = false;
        IsRightHand = false;

        for (int i = 0; i < LandmarkCount; i++)
        {
            WorldLandmarks[i].tracked = false;
            WorldLandmarks[i].worldPosition = Vector3.zero;
        }
    }

    public LandmarkData GetWorldLandmark(int index)
    {
        if (index < 0 || index >= LandmarkCount)
            return null;

        return WorldLandmarks[index];
    }

    public Vector3 GetNormalizedLandmark(int index)
    {
        if (!HasHand)
            return Vector3.zero;

        if (index < 0 || index >= LandmarkCount)
            return Vector3.zero;

        var lm = CurrentResult.handLandmarks[0].landmarks[index];

        return new Vector3(lm.x, lm.y, lm.z);
    }
}