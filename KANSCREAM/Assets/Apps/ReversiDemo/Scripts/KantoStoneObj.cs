using UnityEngine;

public class KantoStoneObj : MonoBehaviour
{
    public void SetState(OthelloSystem.SpriteState state)
    {
        var active = state != OthelloSystem.SpriteState.NONE && state != OthelloSystem.SpriteState.KANSAI;
        gameObject.SetActive(active);
    }
}