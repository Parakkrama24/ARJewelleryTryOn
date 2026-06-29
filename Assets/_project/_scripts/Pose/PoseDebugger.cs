using UnityEngine;

public class PoseDebugger : MonoBehaviour
{
    [SerializeField] private int landmark = 11;

    void Update()
    {
        if (PoseTrackingManager.Instance == null)
            return;

        if (!PoseTrackingManager.Instance.HasPose)
            return;

        Debug.Log("hdghg"+PoseTrackingManager.Instance.GetLandmark(landmark));
    }
}