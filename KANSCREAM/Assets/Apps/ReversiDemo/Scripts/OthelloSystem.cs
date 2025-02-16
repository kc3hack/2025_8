using UnityEngine;
using System.Collections.Generic;
using System.Threading;
public class OthelloSystem : MonoBehaviour
{
    public GameObject KantoStone;//オセロ駒オブジェクト
    public GameObject KansaiStone;//オセロ駒オブジェクト
    public GameObject SelectedFieldCube;//選択中のフィールドを示すオブジェクト
    const int FIELD_SIZE_X = 6;//盤のサイズ
    const int FIELD_SIZE_Y = 6;//盤のサイズ
    private int SelectedFieldCubePosX;//指定しているマスのX座標
    private int SelectedFieldCubePosY;//指定しているマスのY座標
    private SpriteState _PlayerTurn = SpriteState.KANTO;//プレイヤーのターン(関東先手)
    private bool turnCheck = false;

    private List<(int, int)> _InfoList = new List<(int, int)>();//置ける場所のリスト
    public enum SpriteState
    {
        NONE,
        KANTO,
        KANSAI,
    }

    private SpriteState[,] _FieldState = new SpriteState[FIELD_SIZE_X, FIELD_SIZE_Y];//盤の状態
    private KantoStoneObj[,] _KantoStoneObj = new KantoStoneObj[FIELD_SIZE_X, FIELD_SIZE_Y];//オセロ駒オブジェクト
    private KansaiStoneObj[,] _KansaiStoneObj = new KansaiStoneObj[FIELD_SIZE_X, FIELD_SIZE_Y];//オセロ駒オブジェクト

    void Start()
    {
        //選択中のフィールドを示すオブジェクトの初期位置を設定
        SelectedFieldCubePosX = (int)SelectedFieldCube.transform.position.x;
        SelectedFieldCubePosY = (int)SelectedFieldCube.transform.position.z;

        //盤の初期化
        for (int y = 0; y < FIELD_SIZE_Y; y++)
        {
            for (int x = 0; x < FIELD_SIZE_X; x++)
            {
                var kanto = Instantiate(KantoStone
                                        , new Vector3(-2f + x, 0.02f, -2f + y)
                                        , Quaternion.Euler(0, 0, 0));
                var kansai = Instantiate(KansaiStone
                                        , new Vector3(-2f + x, 0.02f, -2f + y)
                                        , Quaternion.Euler(0, 0, 0));
                _FieldState[x, y] = SpriteState.NONE;
                _KantoStoneObj[x, y] = kanto.GetComponent<KantoStoneObj>();
                _KansaiStoneObj[x, y] = kansai.GetComponent<KansaiStoneObj>();
                _KantoStoneObj[x, y].SetState(SpriteState.NONE);
                _KansaiStoneObj[x, y].SetState(SpriteState.NONE);
            }
        }

        //初期配置  関東:黒  関西:白
        _KantoStoneObj[2, 2].SetState(SpriteState.KANTO);
        _KansaiStoneObj[3, 2].SetState(SpriteState.KANSAI);
        _KantoStoneObj[3, 3].SetState(SpriteState.KANTO);
        _KansaiStoneObj[2, 3].SetState(SpriteState.KANSAI);

        _FieldState[2, 2] = SpriteState.KANTO;
        _FieldState[3, 2] = SpriteState.KANSAI;
        _FieldState[3, 3] = SpriteState.KANTO;
        _FieldState[2, 3] = SpriteState.KANSAI;

        // Debug.Log("[2,2]関東 " + _FieldState[2, 2]);
        // Debug.Log("[3,2]関西 " + _FieldState[3, 2]);
        // Debug.Log("[3,3]関東 " + _FieldState[3, 3]);
        // Debug.Log("[2,3]関西 " + _FieldState[2, 3]);
    }

    void Update()
    {
        UpdateSelectedFieldPosition();
    }

    private void UpdateSelectedFieldPosition()
    {
        var position = SelectedFieldCube.transform.position;

        //選択中のフィールドを移動
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

        turnCheck = false;

        //スペースキーで駒を置く
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (TurnCheck())
            {
                turnCheck = true;
            }

            // Debug.Log("turnCheck: " + turnCheck);

            if (turnCheck)
            {
                _FieldState[SelectedFieldCubePosX + 2, SelectedFieldCubePosY + 2] = _PlayerTurn;
                // Debug.Log("SelectedFieldCubePosX: " + SelectedFieldCubePosX + " SelectedFieldCubePosY: " + SelectedFieldCubePosY);
                // Debug.Log(_PlayerTurn);
                if (_PlayerTurn == SpriteState.KANTO)
                {
                    _KantoStoneObj[SelectedFieldCubePosX + 2, SelectedFieldCubePosY + 2].SetState(SpriteState.KANTO);
                }
                else
                {
                    _KansaiStoneObj[SelectedFieldCubePosX + 2, SelectedFieldCubePosY + 2].SetState(SpriteState.KANSAI);
                }
                _PlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
                Thread.Sleep(100);
            }
        }
    }

    private bool TurnCheck()
    {
        var posX = SelectedFieldCubePosX + 2;//選択中のフィールドの移動範囲調整
        var posY = SelectedFieldCubePosY + 2;//選択中のフィールドの移動範囲調整

        var opponentPlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;

        var opponentInfoList = new List<(int, int)>();
        int i = 0;

        while (0 <= posX && posX <= FIELD_SIZE_X && 0 <= posY && posY <= FIELD_SIZE_Y)
        {
            if (posX == 0)
            {
                break;
            }
            posX--;

            // Debug.Log("PlayerTurn: "+_PlayerTurn);
            // Debug.Log("opponentPlayerTurn: " + opponentPlayerTurn);
            Debug.Log(++i + "つ隣のコマ: " + _FieldState[posX, posY]);//<-これちゃんと取れてる
            // _KansaiStoneObj[posX, posY].SetState(SpriteState.KANSAI);
            // _KantoStoneObj[posX, posY].SetState(SpriteState.KANTO);

            //左隣に相手のコマがあるときその情報をリストに追加
            if (_FieldState[posX, posY] == opponentPlayerTurn)
            {
                opponentInfoList.Add((posX, posY));
                // Debug.Log("opponentInfoList: " + opponentInfoList);
            }

            //1回目のループで左のコマが自分のコマまたは空の場合は終了
            if (opponentInfoList.Count == 0 && (_FieldState[posX, posY] == _PlayerTurn || _FieldState[posX, posY] == SpriteState.NONE))
            {
                turnCheck = false;
                break;
            }

            //2つ以上隣のコマが自分のコマの場合は置ける
            if (opponentInfoList.Count > 0 && (_FieldState[posX, posY] == _PlayerTurn || _FieldState[posX, posY] == SpriteState.NONE))
            {
                turnCheck = true;
                foreach (var info in opponentInfoList)
                {
                    _InfoList.Add(info);
                }
                break;
            }
        }
        return turnCheck;
        // return true;
    }
}