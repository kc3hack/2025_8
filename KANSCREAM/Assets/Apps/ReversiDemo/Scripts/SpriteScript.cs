using UnityEngine;

public class SpriteScript : MonoBehaviour
{
    public void SetState(OthelloSystem.SpriteState state)
    {
        var active = state != OthelloSystem.SpriteState.NONE;
        gameObject.SetActive(active);
    }
}