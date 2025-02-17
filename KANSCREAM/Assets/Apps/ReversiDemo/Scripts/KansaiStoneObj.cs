using UnityEngine;

public class KansaiStoneObj : MonoBehaviour
{
    public void SetState(OthelloSystem.SpriteState state)
    {
        // var active = state != OthelloSystem.SpriteState.NONE && state != OthelloSystem.SpriteState.KANTO;
        // gameObject.SetActive(active);
        var isActived = state != OthelloSystem.SpriteState.NONE && state != OthelloSystem.SpriteState.KANTO;
        gameObject.SetActive(isActived);
    }
}