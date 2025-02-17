using UnityEngine;

public class KantoStoneObj : MonoBehaviour
{
    void Start()
    {
        this.transform.rotation = Quaternion.Euler(-90, 180, 0);
    }
    public void SetState(OthelloSystem.SpriteState state)
    {
        // var active = state != OthelloSystem.SpriteState.NONE && state != OthelloSystem.SpriteState.KANSAI;
        // gameObject.SetActive(active);
        var isActived = state != OthelloSystem.SpriteState.NONE && state != OthelloSystem.SpriteState.KANSAI;
        gameObject.SetActive(isActived);
    }
}