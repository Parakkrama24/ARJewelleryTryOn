using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.XR.ARFoundation;
#endif

public class HomeScreen : MonoBehaviour
{
    [Header("Scene Index Settings")]
    [Tooltip("Scene build indices that are Unity AR scenes (ARDemo 1, ARDemo 2, etc.)")]
    [SerializeField] private int[] arSceneIndices = { };   // fill in the Inspector

    void Start() { }

    void Update() { }

    /// <summary>
    /// Called by UI buttons to load any scene by index.
    /// Automatically resets the MediaPipe camera permission flag when
    /// navigating TO a non-AR (MediaPipe/Hand Landmark) scene, so
    /// the camera can be re-acquired after an AR session released it.
    /// </summary>
    public void onStartAR(int sceneIndex)
    {
        Debug.Log($"Loading scene index: {sceneIndex}");

        bool isARScene = System.Array.IndexOf(arSceneIndices, sceneIndex) >= 0;

        if (!isARScene)
        {
            // Going to a MediaPipe / Hand Landmark scene.
            // The AR session holds the camera exclusively — reset the permission
            // flag so WebCamSource re-acquires the camera cleanly.
            Mediapipe.Unity.WebCamSource.ResetPermission();
            Debug.Log("WebCamSource permission reset for MediaPipe scene.");
        }

#if UNITY_ANDROID || UNITY_IOS
        // Stop any active AR session before switching scenes so the
        // camera hardware is fully released before the next scene starts.
        var arSession = FindObjectOfType<ARSession>();
        if (arSession != null)
        {
            arSession.Reset();
            Debug.Log("AR Session reset before scene transition.");
        }
#endif

        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Called by the back button. Always resets the camera permission
    /// so returning to Hand Landmark scenes works correctly.
    /// </summary>
    public void backtoHome()
    {
        // Reset permission so any subsequent MediaPipe scene loads cleanly.
        Mediapipe.Unity.WebCamSource.ResetPermission();

#if UNITY_ANDROID || UNITY_IOS
        var arSession = FindObjectOfType<ARSession>();
        if (arSession != null)
        {
            arSession.Reset();
        }
#endif

        SceneManager.LoadScene(0); // Home scene is always index 0
    }
}
