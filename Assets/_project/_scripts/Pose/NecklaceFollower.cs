using UnityEngine;

public class NecklaceFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Pose Landmarks")]
    [SerializeField] private int leftShoulder = 11;
    [SerializeField] private int rightShoulder = 12;

    [Header("Movement")]
    [SerializeField] private float smoothSpeed = 15f;

    [Header("Offset")]
    [SerializeField] private float neckOffset = 0.08f;

    private void Update()
    {
        if (PoseTrackingManager.Instance == null)
            return;

        if (!PoseTrackingManager.Instance.HasPose)
            return;

        Vector3 left = PoseTrackingManager.Instance.GetLandmark(leftShoulder);
        Vector3 right = PoseTrackingManager.Instance.GetLandmark(rightShoulder);

        //----------------------------------------
        // Convert to screen coordinates
        //----------------------------------------

        Vector3 leftScreen = new Vector3(
            left.x * Screen.width,
            (1f - left.y) * Screen.height,
            0.5f);

        Vector3 rightScreen = new Vector3(
            right.x * Screen.width,
            (1f - right.y) * Screen.height,
            0.5f);

        //----------------------------------------
        // Convert to world
        //----------------------------------------

        Vector3 leftWorld = mainCamera.ScreenToWorldPoint(leftScreen);
        Vector3 rightWorld = mainCamera.ScreenToWorldPoint(rightScreen);

        //----------------------------------------
        // Midpoint
        //----------------------------------------

        Vector3 target =
            (leftWorld + rightWorld) * 0.5f;

        //----------------------------------------
        // Move below shoulders
        //----------------------------------------

        target += Vector3.down * neckOffset;

        //----------------------------------------
        // Smooth movement
        //----------------------------------------

        transform.position = Vector3.Lerp(
            transform.position,
            target,
            Time.deltaTime * smoothSpeed);
    }
}