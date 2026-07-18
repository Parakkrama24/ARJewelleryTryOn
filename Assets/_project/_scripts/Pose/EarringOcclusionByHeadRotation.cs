using UnityEngine;

/// <summary>
/// Hides earrings that would be occluded by the face when the head
/// rotates beyond a configurable Y-axis threshold (default ≈ 45°).
///
/// Attach this script to the AR Default Face prefab root.
/// Assign the left and right earring GameObjects in the Inspector.
///
/// Logic:
///   • Head turns RIGHT (positive Y rotation) → the LEFT earring goes
///     behind the face → hide left earring.
///   • Head turns LEFT  (negative Y rotation) → the RIGHT earring goes
///     behind the face → hide right earring.
///   • When the head is mostly facing forward (within threshold) → both
///     earrings are visible.
///
/// The transition uses a small fade-out zone (hysteresis) so the
/// earrings don't flicker on/off at the boundary angle.
/// </summary>
public class EarringOcclusionByHeadRotation : MonoBehaviour
{
    [Header("Earring References")]
    [Tooltip("The left earring GameObject (child of this face prefab).")]
    [SerializeField] private GameObject _leftEarring;

    [Tooltip("The right earring GameObject (child of this face prefab).")]
    [SerializeField] private GameObject _rightEarring;

    [Header("Rotation Thresholds")]
    [Tooltip("Y-axis rotation angle (degrees) beyond which the far-side earring is hidden.")]
    [Range(10f, 80f)]
    [SerializeField] private float _hideAngle = 45f;

    [Tooltip("Hysteresis buffer (degrees) to prevent flickering at the threshold. " +
             "The earring reappears when the angle drops below (hideAngle − buffer).")]
    [Range(0f, 15f)]
    [SerializeField] private float _hysteresisBuffer = 5f;

    [Header("Debug")]
    [Tooltip("Log rotation and visibility changes to the console.")]
    [SerializeField] private bool _debugLog = false;

    // Track previous visibility so we only log on state changes
    private bool _leftWasVisible = true;
    private bool _rightWasVisible = true;

    private void LateUpdate()
    {
        // Read the local Y rotation of the face (AR tracking drives this)
        float yAngle = WrapAngle(transform.localEulerAngles.y);

        float showAngle = _hideAngle - _hysteresisBuffer;

        // --- Left earring ---
        // Head turning RIGHT (positive Y) → left earring goes behind face
        bool shouldShowLeft = DetermineVisibility(
            yAngle,
            isPositiveSide: true,
            wasVisible: _leftWasVisible,
            hideAngle: _hideAngle,
            showAngle: showAngle);

        // --- Right earring ---
        // Head turning LEFT (negative Y) → right earring goes behind face
        bool shouldShowRight = DetermineVisibility(
            yAngle,
            isPositiveSide: false,
            wasVisible: _rightWasVisible,
            hideAngle: _hideAngle,
            showAngle: showAngle);

        // Apply visibility
        SetEarringActive(_leftEarring, shouldShowLeft, "Left", ref _leftWasVisible);
        SetEarringActive(_rightEarring, shouldShowRight, "Right", ref _rightWasVisible);
    }

    /// <summary>
    /// Determines whether an earring on a given side should be visible,
    /// incorporating hysteresis to avoid flickering.
    /// </summary>
    /// <param name="yAngle">Current Y rotation in -180..+180 range.</param>
    /// <param name="isPositiveSide">True for the left earring (hidden on positive Y rotation),
    /// false for the right earring (hidden on negative Y rotation).</param>
    /// <param name="wasVisible">Whether the earring was visible last frame.</param>
    /// <param name="hideAngle">Angle at which to hide.</param>
    /// <param name="showAngle">Angle at which to show again (hideAngle − buffer).</param>
    private bool DetermineVisibility(float yAngle, bool isPositiveSide,
                                     bool wasVisible, float hideAngle, float showAngle)
    {
        // For the LEFT earring: occluded when head rotates RIGHT → yAngle > threshold
        // For the RIGHT earring: occluded when head rotates LEFT → yAngle < -threshold
        float relevantAngle = isPositiveSide ? yAngle : -yAngle;

        if (wasVisible)
        {
            // Currently visible → hide when angle exceeds hideAngle
            return relevantAngle < hideAngle;
        }
        else
        {
            // Currently hidden → show again when angle drops below showAngle
            return relevantAngle < showAngle;
        }
    }

    /// <summary>
    /// Activates or deactivates an earring GameObject, with optional debug logging.
    /// </summary>
    private void SetEarringActive(GameObject earring, bool shouldBeActive,
                                  string label, ref bool wasVisible)
    {
        if (earring == null) return;

        if (shouldBeActive != wasVisible)
        {
            if (_debugLog)
            {
                float yAngle = WrapAngle(transform.localEulerAngles.y);
                Debug.Log($"[EarringOcclusion] {label} earring " +
                          $"{(shouldBeActive ? "SHOWN" : "HIDDEN")} " +
                          $"(Y rotation: {yAngle:F1}°)");
            }
        }

        earring.SetActive(shouldBeActive);
        wasVisible = shouldBeActive;
    }

    /// <summary>
    /// Converts Unity's 0–360 euler angle to -180..+180 range.
    /// </summary>
    private float WrapAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
