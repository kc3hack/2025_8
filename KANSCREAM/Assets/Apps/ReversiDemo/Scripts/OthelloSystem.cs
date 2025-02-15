using UnityEngine;
using System.Threading;
public class OthelloSystem : MonoBehaviour
{
    public GameObject KantoStone;//オセロ駒オブジェクト
    public GameObject KansaiStone;//オセロ駒オブジェクト
    public GameObject SelectedFieldCube;//選択中のフィールドを示すオブジェクト
    const int FIELD_SIZE_X = 6;
    const int FIELD_SIZE_Y = 6;
    private int SelectedFieldCubePosX;
    private int SelectedFieldCubePosY;
    private SpriteState _PlayerTurn = SpriteState.KANTO;
    public enum SpriteState
    {
        NONE,
        KANTO,
        KANSAI,
    }

    private SpriteState[,] _FieldState = new SpriteState[FIELD_SIZE_X, FIELD_SIZE_Y];
    private KantoStoneObj[,] _KantoStoneObj = new KantoStoneObj[FIELD_SIZE_X, FIELD_SIZE_Y];
    private KansaiStoneObj[,] _KansaiStoneObj = new KansaiStoneObj[FIELD_SIZE_X, FIELD_SIZE_Y];

    void Start()
    {
        SelectedFieldCubePosX = (int)SelectedFieldCube.transform.position.x;
        SelectedFieldCubePosY = (int)SelectedFieldCube.transform.position.z;

        for (int y = 0; y < FIELD_SIZE_Y; y++)
        {
            for (int x = 0; x < FIELD_SIZE_X; x++)
            {
                var kanto = Instantiate(KantoStone
                                        , new Vector3(-2.5f + x, 0.02f, -2.5f + y)
                                        , Quaternion.Euler(0, 0, 0));
                var kansai = Instantiate(KansaiStone
                                        , new Vector3(-2.5f + x, 0.02f, -2.5f + y)
                                        , Quaternion.Euler(0, 0, 0));
                _FieldState[x, y] = SpriteState.NONE;
                _KantoStoneObj[x, y] = kanto.GetComponent<KantoStoneObj>();
                _KansaiStoneObj[x, y] = kansai.GetComponent<KansaiStoneObj>();
                _KantoStoneObj[x, y].SetState(SpriteState.NONE);
                _KansaiStoneObj[x, y].SetState(SpriteState.NONE);
            }
        }
        _KantoStoneObj[2, 2].SetState(SpriteState.KANTO);
        _KansaiStoneObj[3, 2].SetState(SpriteState.KANSAI);
        _KantoStoneObj[3, 3].SetState(SpriteState.KANTO);
        _KansaiStoneObj[2, 3].SetState(SpriteState.KANSAI);
    }

    void Update()
{
    UpdateSelectedFieldPosition();
}

private void UpdateSelectedFieldPosition()
{
    var position = SelectedFieldCube.transform.position;

    if (Input.GetKeyDown(KeyCode.UpArrow) && SelectedFieldCubePosY < FIELD_SIZE_Y - 3)
    {
        SelectedFieldCubePosY++;
        SelectedFieldCube.transform.position = new Vector3(position.x, position.y, position.z + 1);
        // Debug.Log("UpArrow");
    }
    else if (Input.GetKeyDown(KeyCode.DownArrow) && SelectedFieldCubePosY > -2)
    {
        SelectedFieldCubePosY--;
        SelectedFieldCube.transform.position = new Vector3(position.x, position.y, position.z - 1);
        // Debug.Log("DownArrow");
    }
    else if (Input.GetKeyDown(KeyCode.LeftArrow) && SelectedFieldCubePosX > -2)
    {
        SelectedFieldCubePosX--;
        SelectedFieldCube.transform.position = new Vector3(position.x - 1, position.y, position.z);
        // Debug.Log("LeftArrow");
    }
    else if (Input.GetKeyDown(KeyCode.RightArrow) && SelectedFieldCubePosX < FIELD_SIZE_X - 3)
    {
        SelectedFieldCubePosX++;
        SelectedFieldCube.transform.position = new Vector3(position.x + 1, position.y, position.z);
        // Debug.Log("RightArrow");
    }

    if(Input.GetKeyDown(KeyCode.Return))
    {
        _FieldState[SelectedFieldCubePosX+2, SelectedFieldCubePosY+2] = _PlayerTurn;
        _KantoStoneObj[SelectedFieldCubePosX+2, SelectedFieldCubePosY+2].SetState(_PlayerTurn);
        // Debug.Log("SelectedFieldCubePosX: " + SelectedFieldCubePosX + " SelectedFieldCubePosY: " + SelectedFieldCubePosY);
        // Debug.Log(_PlayerTurn); 
    }
}
}