using UnityEngine;
using UnityEngine.UI;

public class OthelloView : MonoBehaviour
{
    public GameObject KantoStone; // オセロ駒オブジェクト
    public GameObject KansaiStone; // オセロ駒オブジェクト

    public void SetStonePos(StateManager state, int x, int y)
    {
        if (state.GetState(x, y) == StateManager.SpriteState.KANTO)
        {
            KantoStone.transform.position = new Vector3(x, 0, y);
        }
        else if (state.GetState(x, y) == StateManager.SpriteState.KANSAI)
        {
            KansaiStone.transform.position = new Vector3(x, 0, y);
        }
    }
}