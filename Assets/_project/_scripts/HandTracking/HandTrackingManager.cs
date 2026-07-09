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

    /// <summary>
    /// Returns true when the PALM is facing the camera, false when BACK of hand faces camera.
    /// Uses Method 2: cross product of three palm landmarks (Wrist=0, Index MCP=5, Pinky MCP=17)
    /// in world coordinates. The sign of the resulting normal's z-component determines orientation.
    /// </summary>
    public bool IsPalmFacingCamera
    {
        get
        {
            if (!HasHand || CurrentResult.handWorldLandmarks == null ||
                CurrentResult.handWorldLandmarks.Count == 0)
                return false;

            var wl = CurrentResult.handWorldLandmarks[0].landmarks;
            if (wl == null || wl.Count < 18)
                return false;

            // Three palm reference landmarks (world coords in metres)
            Vector3 wrist    = new Vector3(wl[0].x,  wl[0].y,  wl[0].z);
            Vector3 indexMCP = new Vector3(wl[5].x,  wl[5].y,  wl[5].z);
            Vector3 pinkyMCP = new Vector3(wl[17].x, wl[17].y, wl[17].z);

            // Vectors lying in the palm plane
            Vector3 a = indexMCP - wrist;
            Vector3 b = pinkyMCP - wrist;

            // Cross product gives palm surface normal
            Vector3 palmNormal = Vector3.Cross(a, b);

            // In MediaPipe world coords, z is positive away from the camera.
            // Palm facing camera → normal z < 0.
            // Back facing camera → normal z > 0.
            return palmNormal.z < 0f;
        }
    }

    /// <summary>
    /// "Left", "Right", or "Unknown" from MediaPipe handedness classification.
    /// </summary>
    public string HandSide
    {
        get
        {
            if (!HasHand || CurrentResult.handedness == null ||
                CurrentResult.handedness.Count == 0)
                return "Unknown";

            var cats = CurrentResult.handedness[0].categories;
            if (cats == null || cats.Count == 0)
                return "Unknown";

            return cats[0].categoryName ?? "Unknown";
        }
    }

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

