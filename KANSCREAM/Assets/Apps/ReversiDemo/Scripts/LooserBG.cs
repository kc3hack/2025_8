using UnityEngine;

public class LooserBG : MonoBehaviour
{
    private bool isActive;
    void Start()
    {
        isActive = false;
        this.SetActive(isActive);
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
        this.SetActive(isActive);
    }
}