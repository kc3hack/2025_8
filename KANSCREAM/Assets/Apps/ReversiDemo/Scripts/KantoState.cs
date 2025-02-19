using UnityEngine;

public class KantoState : MonoBehaviour
{
    void Start()
    {
        this.transform.rotation = Quaternion.Euler(-90, 180, 0);
    }
    public void SetState(StateManager.SpriteState state)
    {
        var isActived = state != StateManager.SpriteState.NONE && state != StateManager.SpriteState.KANSAI;
        gameObject.SetActive(isActived);
    }
}