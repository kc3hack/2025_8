using UnityEngine;

public class KansaiState : MonoBehaviour
{
    void Start()
    {
        this.transform.rotation = Quaternion.Euler(-90, 180, 0);
    }
    public void SetState(StateManager.SpriteState state)
    {
        var isActived = state != StateManager.SpriteState.NONE && state != StateManager.SpriteState.KANTO;
        gameObject.SetActive(isActived);
    }
}