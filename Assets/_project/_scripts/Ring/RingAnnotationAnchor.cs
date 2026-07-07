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

    [Header("Landmark Settings")]
    [Tooltip("Landmark to place ring at. 13=Ring MCP (base), 14=Ring PIP.")]
    [SerializeField] private int _landmarkIndex = 13;

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
    /// Change the finger at runtime (called from UI buttons).
    /// Direction is always anchorIndex+1 automatically.
    /// Examples: 13=ring base, 14=ring PIP, 10=middle base, 3=thumb.
    /// </summary>
    public void SetLandmarkIndex(int anchorIndex)
    {
        _landmarkIndex       = anchorIndex;
        _rotationInitialized = false;  // snap rotation immediately on finger switch
    }
}
