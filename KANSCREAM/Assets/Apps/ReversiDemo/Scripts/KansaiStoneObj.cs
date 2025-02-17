using UnityEngine;

public class KansaiStoneObj : MonoBehaviour
{
    void Start()
    {
        this.transform.rotation = Quaternion.Euler(-90, 180, 0);
    }
    public void SetState(OthelloSystem.SpriteState state)
    {
        // var active = state != OthelloSystem.SpriteState.NONE && state != OthelloSystem.SpriteState.KANTO;
        // gameObject.SetActive(active);
        var isActived = state != OthelloSystem.SpriteState.NONE && state != OthelloSystem.SpriteState.KANTO;
        gameObject.SetActive(isActived);
    }
}