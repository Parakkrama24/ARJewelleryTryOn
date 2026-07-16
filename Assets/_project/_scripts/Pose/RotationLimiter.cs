using UnityEngine;

/// <summary>
/// Attach this script directly to the Necklace GameObject.
/// It clamps the object's rotation each frame so it never rotates
/// beyond the set limits — regardless of what is driving the rotation
/// (AR face tracking, pose tracking, animator, etc.).
///
/// Usage:
///   1. Add this component to your Necklace GameObject.
///   2. Set Max Rotation limits in the Inspector.
///   3. Press Play — rotation will be clamped automatically.
/// </summary>
public class RotationLimiter : MonoBehaviour
{
    [Header("X Axis Limits (Tilt Up / Down)")]
    [Tooltip("Limit rotation on the X axis (nodding up/down). Uncheck to ignore X.")]
    [SerializeField] private bool _limitX = false;
    [Range(0f, 180f)]
    [SerializeField] private float _maxX = 30f;

    [Header("Y Axis Limits (Turn Left / Right)")]
    [Tooltip("Limit rotation on the Y axis (turning left/right). Uncheck to ignore Y.")]
    [SerializeField] private bool _limitY = false;
    [Range(0f, 180f)]
    [SerializeField] private float _maxY = 30f;

    [Header("Z Axis Limits (Tilt / Roll)")]
    [Tooltip("Limit rotation on the Z axis (shoulder tilt / roll). Uncheck to ignore Z.")]
    [SerializeField] private bool _limitZ = true;
    [Range(0f, 180f)]
    [SerializeField] private float _maxZ = 30f;

    private void LateUpdate()
    {
        // Read current euler angles (Unity returns 0–360, convert to -180..+180)
        Vector3 euler = transform.localEulerAngles;

        float x = WrapAngle(euler.x);
        float y = WrapAngle(euler.y);
        float z = WrapAngle(euler.z);

        // Clamp each enabled axis symmetrically around zero
        if (_limitX) x = Mathf.Clamp(x, -_maxX, _maxX);
        if (_limitY) y = Mathf.Clamp(y, -_maxY, _maxY);
        if (_limitZ) z = Mathf.Clamp(z, -_maxZ, _maxZ);

        transform.localEulerAngles = new Vector3(x, y, z);
    }

    /// <summary>
    /// Converts Unity's 0–360 euler angle to -180..+180 range
    /// so clamping works correctly around the centre (0°).
    /// </summary>
    private float WrapAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
