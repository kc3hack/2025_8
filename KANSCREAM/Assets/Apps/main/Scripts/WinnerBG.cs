using UnityEngine;

public class WinnerBG : MonoBehaviour
{
    private bool isActive;
    void Start()
    {
        isActive = true;
        this.SetActive(isActive);
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
        this.SetActive(isActive);
    }
}