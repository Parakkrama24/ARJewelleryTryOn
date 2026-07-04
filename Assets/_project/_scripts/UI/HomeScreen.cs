using UnityEngine;

public class HomeScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onStartAR(int SceneIndex)
    {
        // Load the next scene or perform any other action when the start button is clicked
        Debug.Log("Start button clicked!");

        // Example: Load the AR scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneIndex);
    }

    public void backtoHome()
    {
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Load the home scene (index 0)
    }
}
