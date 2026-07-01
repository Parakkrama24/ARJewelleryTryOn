using UnityEngine;

public class ModelManager : MonoBehaviour
{
    [SerializeField] private GameObject[] ringModels;

    private int currentIndex = 0;

    

    private void Start()
    {
        ShowModel(currentIndex);
    }

    public void NextModel()
    {
        currentIndex++;

        if (currentIndex >= ringModels.Length)
            currentIndex = 0;

        ShowModel(currentIndex);
    }

    public void PreviousModel()
    {
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = ringModels.Length - 1;

        ShowModel(currentIndex);
    }

    private void ShowModel(int index)
    {
        for (int i = 0; i < ringModels.Length; i++)
        {
            ringModels[i].SetActive(i == index);
        }

        Debug.Log($"Current Ring: {ringModels[index].name}");
    }
}