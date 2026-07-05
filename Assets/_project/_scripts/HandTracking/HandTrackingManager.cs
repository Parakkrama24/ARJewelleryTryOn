using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class HandTrackingManager : MonoBehaviour
{
    public static HandTrackingManager Instance { get; private set; }

    public HandLandmarkerResult CurrentResult { get; private set; }

    // Fix 2: Per-landmark smoothing to reduce MediaPipe noise on mobile.
    // _smoothing = 0 → no smoothing (raw values).
    // _smoothing = 1 → fully frozen (never updates). Keep between 0.4–0.7.
    [SerializeField] private float _smoothing = 0.6f;
    private readonly Vector3[] _smoothedLandmarks = new Vector3[21];

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

        if (CurrentResult.handLandmarks.Count == 0)
            return Vector3.zero;

        var hand = CurrentResult.handLandmarks[0];

        if (hand.landmarks == null)
            return Vector3.zero;

        if (landmarkIndex < 0 || landmarkIndex >= hand.landmarks.Count)
        {
            Debug.LogWarning($"Invalid landmark index {landmarkIndex}. Count = {hand.landmarks.Count}");
            return Vector3.zero;
        }

        var landmark = hand.landmarks[landmarkIndex];
        Vector3 raw = new Vector3(landmark.x, landmark.y, landmark.z);

        // Apply exponential low-pass filter to reduce per-frame jitter.
        // On mobile, MediaPipe runs at lower FPS with noisier camera input,
        // so raw landmarks jump noticeably without this filter.
        _smoothedLandmarks[landmarkIndex] = Vector3.Lerp(
            raw,
            _smoothedLandmarks[landmarkIndex],
            _smoothing);

        return _smoothedLandmarks[landmarkIndex];
    }

    public Landmark GetWorldLandmark(int handIndex, int landmarkIndex)
    {
        if (!HasHand)
            return default;

        return CurrentResult.handWorldLandmarks[handIndex].landmarks[landmarkIndex];
    }
}

