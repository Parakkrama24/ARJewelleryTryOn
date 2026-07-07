using UnityEngine;
using Mediapipe.Unity;

/// <summary>
/// NEW APPROACH — Annotation Position Tracking
///
/// Tracks the world position of a MediaPipe PointAnnotation each frame
/// and moves the ring there directly — WITHOUT parenting to the annotation.
///
/// WHY NOT SetParent:
///   When landmarks are hidden (_isLandMarkVisible = false), MediaPipe calls
///   SetLandmarkRadius(0) which sets the PointAnnotation's localScale to zero.
///   A parented ring would also inherit scale=0 and disappear.
///   By copying worldPosition instead, ring visibility is fully independent.
///
/// Ring visible   = hand is detected (annotation is active)
/// Dots visible   = controlled separately by MultiHandLandmarkListAnnotation
/// These two are COMPLETELY independent.
///
/// Usage:
///   1. Attach this to your Ring GameObject.
///   2. Assign _multiHandAnnotation in the Inspector.
///   3. Set landmarkIndex (13 = ring finger base, 14 = ring finger PIP).
/// </summary>
public class RingAnnotationAnchor : MonoBehaviour
{
    [Header("Annotation Reference")]
    [Tooltip("Drag the 'Multi HandLandmarkList Annotation' GameObject here.")]
    [SerializeField] private MultiHandLandmarkListAnnotation _multiHandAnnotation;

    [Header("Finger Landmarks")]
    [Tooltip("List of landmark indices the toggle button cycles through.\n" +
             "Default: 14=Ring PIP, 10=Middle PIP, 3=Thumb IP\n" +
             "Add or remove entries freely in the Inspector.")]
    [SerializeField] private int[] _fingerLandmarks = { 14, 10, 3 };

    [Tooltip("Optional display names shown in UI (must match _fingerLandmarks length).")]
    [SerializeField] private string[] _fingerNames = { "Ring", "Middle", "Thumb" };

    // Which finger is currently active (index into _fingerLandmarks)
    private int _currentFingerIndex = 0;

    // Active landmark read from _fingerLandmarks[_currentFingerIndex]
    private int _landmarkIndex => _fingerLandmarks != null && _fingerLandmarks.Length > 0
        ? _fingerLandmarks[_currentFingerIndex]
        : 14;

    /// <summary>Display name of the currently active finger.</summary>
    public string CurrentFingerName =>
        (_fingerNames != null && _currentFingerIndex < _fingerNames.Length)
            ? _fingerNames[_currentFingerIndex]
            : _landmarkIndex.ToString();

    [Header("Ring Offset (World Space)")]
    [Tooltip("World-space offset added to the annotation point position.")]
    [SerializeField] private Vector3 _positionOffset = Vector3.zero;

    [Header("Ring Scale")]
    [Tooltip("World-space scale of the ring (independent of annotation scale).")]
    [SerializeField] private Vector3 _ringScale = Vector3.one;

    [Header("Rotation Tracking")]
    [Tooltip("Rotate ring to align with the finger direction.")]
    [SerializeField] private bool _trackRotation = true;

    // Direction landmark is always _landmarkIndex + 1 automatically.
    // e.g. anchor=13 → direction=14, anchor=14 → direction=15.
    // No need to set this manually.

    [Tooltip("Rotation offset applied on top of the finger direction.")]
    [SerializeField] private Vector3 _rotationOffset = Vector3.zero;

    [Tooltip("Smooth speed for rotation. Higher = snappier.")]
    [SerializeField] private float _rotationSmoothSpeed = 10f;

    // ---------------------------------------------------------------
    // Cached renderers — used to show/hide ring WITHOUT touching scale
    // ---------------------------------------------------------------
    private Renderer[] _renderers;
    private bool       _ringVisible = false;
    private Quaternion _smoothedRotation;  // persisted across frames for smooth slerp
    private bool       _rotationInitialized = false;

    private void Awake()
    {
        // Cache all renderers on this object and its children
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        // Apply the fixed world scale once — never changes after this
        transform.localScale = _ringScale;

        // Make sure ring starts hidden
        SetRingVisible(false);
    }

    private void Update()
    {
        if (_multiHandAnnotation == null)
            return;

        // Hand is present when the annotation container is active AND has children
        bool handDetected = _multiHandAnnotation.gameObject.activeInHierarchy
                            && _multiHandAnnotation.count > 0;

        if (!handDetected)
        {
            SetRingVisible(false);
            return;
        }

        var handAnnotation = _multiHandAnnotation[0];
        if (handAnnotation == null)
        {
            SetRingVisible(false);
            return;
        }

        var pointAnnotation = handAnnotation[_landmarkIndex];
        if (pointAnnotation == null)
        {
            SetRingVisible(false);
            return;
        }

        // ---------------------------------------------------------------
        // POSITION — copy world position from annotation, add offset
        // ---------------------------------------------------------------
        transform.position = pointAnnotation.transform.position + _positionOffset;

        // ---------------------------------------------------------------
        // ROTATION — align ring with finger direction
        // Direction is always anchorIndex + 1 (e.g. 13→14, 14→15, 3→4)
        // ---------------------------------------------------------------
        if (_trackRotation)
        {
            int dirIndex = _landmarkIndex + 1;

            // Guard: don't go out of the 21-landmark range
            if (dirIndex < 21)
            {
                var dirAnnotation = handAnnotation[dirIndex];
                if (dirAnnotation != null)
                {
                    Vector3 direction = dirAnnotation.transform.position
                                        - pointAnnotation.transform.position;

                    if (direction.sqrMagnitude > 0.00001f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(direction)
                                              * Quaternion.Euler(_rotationOffset);

                        // On first valid frame, snap immediately so the ring
                        // doesn't spin in from a wrong orientation.
                        if (!_rotationInitialized)
                        {
                            _smoothedRotation    = targetRot;
                            _rotationInitialized = true;
                        }
                        else
                        {
                            // Smooth the rotation independently of frame rate
                            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(_rotationSmoothSpeed * Time.deltaTime), 1f);
                            _smoothedRotation = Quaternion.Slerp(_smoothedRotation, targetRot, t);
                        }

                        transform.rotation = _smoothedRotation;
                    }
                }
            }
        }

        // ---------------------------------------------------------------
        // VISIBILITY — show ring as soon as hand is detected
        // ---------------------------------------------------------------
        SetRingVisible(true);
    }

    /// <summary>
    /// Shows or hides all renderers on the ring — does NOT change scale,
    /// so annotation dot visibility has zero effect on the ring.
    /// </summary>
    private void SetRingVisible(bool visible)
    {
        if (_ringVisible == visible)
            return;

        _ringVisible = visible;
        foreach (var r in _renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    /// <summary>
    /// Cycles to the next finger in the _fingerLandmarks list.
    /// Call this from your toggle UI button's OnClick event.
    /// </summary>
    public void ToggleFinger()
    {
        if (_fingerLandmarks == null || _fingerLandmarks.Length == 0)
            return;

        _currentFingerIndex = (_currentFingerIndex + 1) % _fingerLandmarks.Length;
        _rotationInitialized = false;   // snap rotation immediately on switch

        Debug.Log($"[RingAnnotationAnchor] Switched to {CurrentFingerName} (landmark {_landmarkIndex})");
    }

    /// <summary>
    /// Jump directly to a specific finger by its index in _fingerLandmarks.
    /// </summary>
    public void SetFingerIndex(int index)
    {
        if (_fingerLandmarks == null || index < 0 || index >= _fingerLandmarks.Length)
            return;

        _currentFingerIndex  = index;
        _rotationInitialized = false;
    }

    /// <summary>
    /// Legacy helper — set by raw landmark number directly.
    /// </summary>
    public void SetLandmarkIndex(int anchorIndex)
    {
        // Find matching entry in the array, or just override index 0
        if (_fingerLandmarks != null)
        {
            for (int i = 0; i < _fingerLandmarks.Length; i++)
            {
                if (_fingerLandmarks[i] == anchorIndex)
                {
                    _currentFingerIndex  = i;
                    _rotationInitialized = false;
                    return;
                }
            }
        }
        // Not found — set first slot directly
        if (_fingerLandmarks != null && _fingerLandmarks.Length > 0)
            _fingerLandmarks[0] = anchorIndex;
        _currentFingerIndex  = 0;
        _rotationInitialized = false;
    }
}
