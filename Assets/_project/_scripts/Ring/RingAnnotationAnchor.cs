using UnityEngine;
using Mediapipe.Unity;

/// <summary>
/// NEW APPROACH — Annotation Anchor Method
/// 
/// Instead of reading raw landmark (x,y,z) and converting through screen space
/// (which caused jitter on mobile), this script directly parents the ring to
/// the PointAnnotation GameObject that MediaPipe ALREADY positions correctly
/// every frame on the annotation canvas.
///
/// Flow:
///   MediaPipe detects hand
///   → MultiHandLandmarkListAnnotation positions PointAnnotation[13] on screen
///   → We parent the ring to that PointAnnotation's transform
///   → Ring moves EXACTLY with the annotation dot — zero extra math needed
///
/// Usage:
///   Attach this script to your Ring GameObject.
///   Assign the MultiHandLandmarkListAnnotation reference in the Inspector.
///   Set landmarkIndex (default 13 = ring finger base).
/// </summary>
public class RingAnnotationAnchor : MonoBehaviour
{
    [Header("Annotation Reference")]
    [Tooltip("Drag the 'Multi Hand Landmark List Annotation' GameObject here.")]
    [SerializeField] private MultiHandLandmarkListAnnotation _multiHandAnnotation;

    [Header("Landmark Settings")]
    [Tooltip("Which landmark to anchor the ring to. 13 = Ring MCP, 14 = Ring PIP.")]
    [SerializeField] private int _landmarkIndex = 13;

    [Header("Ring Offset")]
    [Tooltip("Local position offset from the landmark point (fine-tune ring placement).")]
    [SerializeField] private Vector3 _positionOffset = Vector3.zero;

    [Tooltip("Local rotation offset for the ring.")]
    [SerializeField] private Vector3 _rotationOffset = Vector3.zero;

    [Tooltip("Scale of the ring relative to its anchor parent.")]
    [SerializeField] private Vector3 _localScale    = Vector3.one;

    [Header("Rotation Tracking")]
    [Tooltip("If true, ring will also rotate to follow the finger direction (landmark 13→14).")]
    [SerializeField] private bool _trackRotation = true;

    [Tooltip("Second landmark used to compute finger direction for rotation (ignored if Track Rotation is off).")]
    [SerializeField] private int _directionLandmarkIndex = 14;

    [Tooltip("Smooth speed for rotation tracking. Higher = faster.")]
    [SerializeField] private float _rotationSmoothSpeed = 12f;

    // ---------------------------------------------------------------
    // Private state
    // ---------------------------------------------------------------
    private Transform _anchorTransform;   // The PointAnnotation we're parented to
    private bool      _isAnchored = false;

    private void Update()
    {
        if (_multiHandAnnotation == null)
            return;

        bool handVisible = _multiHandAnnotation.gameObject.activeSelf
                           && _multiHandAnnotation.count > 0;

        if (handVisible)
        {
            TryAnchor();

            if (_isAnchored && _trackRotation)
                UpdateRotation();
        }
        else
        {
            Detach();
        }
    }

    /// <summary>
    /// Parents the ring to the correct PointAnnotation if not already done.
    /// </summary>
    private void TryAnchor()
    {
        // Get the first detected hand's annotation (index 0)
        var handAnnotation = _multiHandAnnotation[0];
        if (handAnnotation == null)
            return;

        // PointAnnotation for our chosen landmark
        var pointAnnotation = handAnnotation[_landmarkIndex];
        if (pointAnnotation == null)
            return;

        Transform pointTransform = pointAnnotation.transform;

        // Only re-parent if the anchor has changed (avoids SetParent every frame)
        if (_anchorTransform != pointTransform)
        {
            transform.SetParent(pointTransform, worldPositionStays: false);
            _anchorTransform = pointTransform;
            _isAnchored      = true;

            // Apply local offset and scale relative to the anchor
            transform.localPosition = _positionOffset;
            transform.localScale    = _localScale;
            transform.localRotation = Quaternion.Euler(_rotationOffset);

            Debug.Log($"[RingAnnotationAnchor] Anchored ring to landmark {_landmarkIndex}");
        }
    }

    /// <summary>
    /// Smoothly rotates the ring to align with the finger direction
    /// using two annotation points (e.g. landmark 13 → 14).
    /// </summary>
    private void UpdateRotation()
    {
        if (_multiHandAnnotation.count == 0)
            return;

        var handAnnotation = _multiHandAnnotation[0];
        if (handAnnotation == null)
            return;

        var startAnnotation = handAnnotation[_landmarkIndex];
        var endAnnotation   = handAnnotation[_directionLandmarkIndex];

        if (startAnnotation == null || endAnnotation == null)
            return;

        Vector3 direction = endAnnotation.transform.position - startAnnotation.transform.position;

        if (direction.sqrMagnitude < 0.00001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction)
                                    * Quaternion.Euler(_rotationOffset);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * _rotationSmoothSpeed);
    }

    /// <summary>
    /// Detaches the ring from the annotation and hides it when no hand is detected.
    /// </summary>
    private void Detach()
    {
        if (!_isAnchored)
            return;

        transform.SetParent(null, worldPositionStays: true);
        _anchorTransform = null;
        _isAnchored      = false;

        Debug.Log("[RingAnnotationAnchor] Detached — no hand detected.");
    }

    /// <summary>
    /// Change the landmark index at runtime (e.g. when user switches fingers).
    /// </summary>
    public void SetLandmarkIndex(int index, int directionIndex = -1)
    {
        _landmarkIndex = index;
        if (directionIndex >= 0)
            _directionLandmarkIndex = directionIndex;

        // Force re-anchor on next Update
        _anchorTransform = null;
        _isAnchored      = false;
    }
}
