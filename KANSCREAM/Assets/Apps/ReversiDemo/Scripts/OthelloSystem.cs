using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Linq;

public class OthelloSystem : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
{
    public GameObject KantoStone;//オセロ駒オブジェクト
    public GameObject KansaiStone;//オセロ駒オブジェクト
    public GameObject SelectedFieldCube;//選択中のフィールドを示すオブジェクト
    const int FIELD_SIZE_X = 6;//盤のサイズ
    const int FIELD_SIZE_Y = 6;//盤のサイズ
    private int SelectedFieldCubePosX;//指定しているマスのX座標
    private int SelectedFieldCubePosY;//指定しているマスのY座標

    private bool _KantoCheckFlag = true;
    private bool _KansaiCheckFlag = true;
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

    private PunTurnManager turnManager;
    private bool isKantoPlayer;

    void Start()
    {
        //選択中のフィールドを示すオブジェクトの初期位置を設定
        SelectedFieldCubePosX = (int)SelectedFieldCube.transform.position.x;
        SelectedFieldCubePosY = (int)SelectedFieldCube.transform.position.z;

        Awake();

        //初期配置  関東:黒  関西:白
        _KantoStoneObj[2, 2].SetState(SpriteState.KANTO);
        _KansaiStoneObj[3, 2].SetState(SpriteState.KANSAI);
        _KantoStoneObj[3, 3].SetState(SpriteState.KANTO);
        _KansaiStoneObj[2, 3].SetState(SpriteState.KANSAI);

        _FieldState[2, 2] = SpriteState.KANTO;
        _FieldState[3, 2] = SpriteState.KANSAI;
        _FieldState[3, 3] = SpriteState.KANTO;
        _FieldState[2, 3] = SpriteState.KANSAI;

        turnManager =  FindObjectOfType<PunTurnManager>();
        if (turnManager == null)
        {
            Debug.LogError("PunTurnManager component is missing from this GameObject");
            return;
        }
        turnManager.TurnManagerListener = this;

        if (PhotonNetwork.IsMasterClient)
        {
            turnManager.BeginTurn(); // ゲーム開始時にターンを開始
            isKantoPlayer = true; // マスタークライアントはKantoプレイヤー
        }
        else
        {
            isKantoPlayer = false; // 2人目のプレイヤーはKansaiプレイヤー
        }
    }

void Update()
{
    if (turnManager == null)
    {
        return;
    }

    if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
    {
        // プレイヤーのターンかどうかを確認
        if ((turnManager.Turn % 2 == 1 && isKantoPlayer) || (turnManager.Turn % 2 == 0 && !isKantoPlayer))
        {
            UpdateSelectedFieldPosition();
        }
    }
}

public void UpdateSelectedFieldPosition()
{
    var position = SelectedFieldCube.transform.position;

    // 選択中のフィールドを移動
    if (Input.GetKeyDown(KeyCode.W) && SelectedFieldCubePosY < FIELD_SIZE_Y - 3)
    {
        SelectedFieldCubePosY++;
        SelectedFieldCube.transform.position = new Vector3(position.x, position.y, position.z + 1);
    }
    else if (Input.GetKeyDown(KeyCode.S) && SelectedFieldCubePosY > -2)
    {
        SelectedFieldCubePosY--;
        SelectedFieldCube.transform.position = new Vector3(position.x, position.y, position.z - 1);
    }
    else if (Input.GetKeyDown(KeyCode.A) && SelectedFieldCubePosX > -2)
    {
        SelectedFieldCubePosX--;
        SelectedFieldCube.transform.position = new Vector3(position.x - 1, position.y, position.z);
    }
    else if (Input.GetKeyDown(KeyCode.D) && SelectedFieldCubePosX < FIELD_SIZE_X - 3)
    {
        SelectedFieldCubePosX++;
        SelectedFieldCube.transform.position = new Vector3(position.x + 1, position.y, position.z);
    }

    turnCheck = false;

    // エンターキーで駒を置く
    if (Input.GetKeyDown(KeyCode.Return))
    {
        // 全方向に対してコマを置けるかどうかの判定
        for (int i = 0; i < 8; i++)
        {
            if (TurnCheck(i))
            {
                turnCheck = true;
            }
        }

        if (turnCheck && _FieldState[SelectedFieldCubePosX + 2, SelectedFieldCubePosY + 2] == SpriteState.NONE)
        {
            var playerTurn = isKantoPlayer ? SpriteState.KANTO : SpriteState.KANSAI;
            Vector3 move = new Vector3(SelectedFieldCubePosX + 2, SelectedFieldCubePosY + 2, (int)playerTurn);

            // _InfoList を int[] に変換
            //int[] infoArray = _InfoList.SelectMany(info => new int[] { info.Item1, info.Item2 }).ToArray();

            turnManager.SendMove(move, false);
            //photonView.RPC("PlaceStone", RpcTarget.All, move, infoArray);
            turnManager.SendMove(null, true);
            turnManager.BeginTurn();
        }
    }
}

    //盤の初期設定
    private void Awake()
    {
        //盤の初期化
        for (int y = 0; y < FIELD_SIZE_Y; y++)
        {
            for (int x = 0; x < FIELD_SIZE_X; x++)
            {
                var kanto = Instantiate(KantoStone
                                        , new Vector3(-0.937f+x, -0.119f,-2.465f+y)
                                        , Quaternion.Euler(0, 0, 0));
                var kansai = Instantiate(KansaiStone
                                        , new Vector3(-2.314f + x, 0.155f, -2.378f + y)
                                        , Quaternion.Euler(0, 0, 0));
                _FieldState[x, y] = SpriteState.NONE;
                _KantoStoneObj[x, y] = kanto.GetComponent<KantoStoneObj>();
                _KansaiStoneObj[x, y] = kansai.GetComponent<KansaiStoneObj>();
                _KantoStoneObj[x, y].SetState(SpriteState.NONE);
                _KansaiStoneObj[x, y].SetState(SpriteState.NONE);
            }
        }
    }



[PunRPC]
private void PlaceStone(Vector3 move, int[] infoArray)
{
    int x = (int)move.x;
    int y = (int)move.y;
    SpriteState playerTurn = (SpriteState)(int)move.z;

    _FieldState[x, y] = playerTurn;
    if (playerTurn == SpriteState.KANTO)
    {
        _KantoStoneObj[x, y].SetState(SpriteState.KANTO);
    }
    else
    {
        _KansaiStoneObj[x, y].SetState(SpriteState.KANSAI);
    }

    // 駒をひっくり返す
    for (int i = 0; i < infoArray.Length; i += 2)
    {
        int posX = infoArray[i];
        int posY = infoArray[i + 1];
        _FieldState[posX, posY] = playerTurn;
        if (playerTurn == SpriteState.KANTO)
        {
            _KantoStoneObj[posX, posY].SetState(SpriteState.KANTO);
            _KansaiStoneObj[posX, posY].SetState(SpriteState.NONE);
        }
        else if (playerTurn == SpriteState.KANSAI)
        {
            _KansaiStoneObj[posX, posY].SetState(SpriteState.KANSAI);
            _KantoStoneObj[posX, posY].SetState(SpriteState.NONE);
        }
    }

    CheckCanSettingStone();

    _InfoList.Clear();
}

    private void CalcTotalStoneNum()
    {
        var kantoStoneNum = 0;
        var kansaiStoneNum = 0;
        for (int y = 0; y < FIELD_SIZE_Y; y++)
        {
            for (int x = 0; x < FIELD_SIZE_X; x++)
            {
                if (_FieldState[x, y] == SpriteState.KANTO)
                {
                    kantoStoneNum++;
                }
                else if (_FieldState[x, y] == SpriteState.KANSAI)
                {
                    kansaiStoneNum++;
                }
            }
        }

        Debug.Log("関東の石の数: " + kantoStoneNum);
        Debug.Log("関西の石の数: " + kansaiStoneNum);

        if (kantoStoneNum + kansaiStoneNum == FIELD_SIZE_X * FIELD_SIZE_Y || (!_KantoCheckFlag && !_KansaiCheckFlag))
        {
            if (kantoStoneNum > kansaiStoneNum)
            {
                Debug.Log("関東の勝ち");
            }
            else if (kantoStoneNum < kansaiStoneNum)
            {
                Debug.Log("関西の勝ち");
            }
            else
            {
                Debug.Log("引き分け");
            }
        }
    }

/// <summary>
/// 置ける場所があるかどうかを判定
/// </summary>
private void CheckCanSettingStone()
{
    _KantoCheckFlag = false;
    _KansaiCheckFlag = false;

    for (int y = 0; y < FIELD_SIZE_Y; y++)
    {
        for (int x = 0; x < FIELD_SIZE_X; x++)
        {
            if (_FieldState[x, y] == SpriteState.NONE)
            {
                for (int i = 0; i < 8; i++)
                {
                    // 現在のプレイヤーに基づいて判定
                    if (TurnCheck(i, x, y))
                    {
                        if (_PlayerTurn == SpriteState.KANTO)
                        {
                            _KantoCheckFlag = true;
                        }
                        else if (_PlayerTurn == SpriteState.KANSAI)
                        {
                            _KansaiCheckFlag = true;
                        }
                    }
                }
            }
        }
    }

    // どちらも置けない場合、ターンを変更
    if (!_KantoCheckFlag && !_KansaiCheckFlag)
    {
        _PlayerTurn = (_PlayerTurn == SpriteState.KANTO) ? SpriteState.KANSAI : SpriteState.KANTO;
    }

    CalcTotalStoneNum();
}



    /// <summary>
    /// コマを置けるかどうかの判定
    /// </summary>
    /// <returns></returns>
   private bool TurnCheck(int direction)
{
    var posX = SelectedFieldCubePosX + 2;//選択中のフィールドの移動範囲調整
    var posY = SelectedFieldCubePosY + 2;//選択中のフィールドの移動範囲調整

    var opponentPlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;

    var opponentInfoList = new List<(int, int)>();
    var localTurnCheck = false;

    while (0 <= posX && posX < FIELD_SIZE_X && 0 <= posY && posY < FIELD_SIZE_Y)
    {
        switch (direction)
        {
            case 0://左
                if (posX == 0) { return localTurnCheck; }
                posX--;
                break;
            case 1://右
                if (posX == FIELD_SIZE_X - 1) { return localTurnCheck; }
                posX++;
                break;
            case 2://下
                if (posY == 0) { return localTurnCheck; }
                posY--;
                break;
            case 3://上
                if (posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
                posY++;
                break;
            case 4://右上
                if (posX == FIELD_SIZE_X - 1 || posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
                posX++;
                posY++;
                break;
            case 5://左下
                if (posX == 0 || posY == 0) { return localTurnCheck; }
                posX--;
                posY--;
                break;
            case 6://左上
                if (posX == 0 || posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
                posX--;
                posY++;
                break;
            case 7://右下
                if (posX == FIELD_SIZE_X - 1 || posY == 0) { return localTurnCheck; }
                posX++;
                posY--;
                break;
        }

        //左隣に相手のコマがあるときその情報をリストに追加
        if (_FieldState[posX, posY] == opponentPlayerTurn)
        {
            opponentInfoList.Add((posX, posY));
        }

        //1回目のループで左のコマが自分のコマまたは空の場合は終了
        if (opponentInfoList.Count == 0 && (_FieldState[posX, posY] == _PlayerTurn || _FieldState[posX, posY] == SpriteState.NONE))
        {
            localTurnCheck = false;
            break;
        }

        //2つ以上隣のコマが自分のコマの場合は置ける
        if (opponentInfoList.Count > 0 && (_FieldState[posX, posY] == _PlayerTurn))
        {
            localTurnCheck = true;
            foreach (var info in opponentInfoList)
            {
                _InfoList.Add(info);
            }
            break;
        }
    }
    return localTurnCheck;
}


/// <summary>
/// コマを置けるかどうかの判定
/// </summary>
/// <returns></returns>
private bool TurnCheck(int direction, int pointX, int pointY)
{
    int posX = pointX;
    int posY = pointY;

    // 現在のプレイヤーの反対のプレイヤーを取得
    var opponentPlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;

    var opponentInfoList = new List<(int, int)>();
    bool localTurnCheck = false;

    // フィールドの範囲内かチェック
    if (posX < 0 || posX >= FIELD_SIZE_X || posY < 0 || posY >= FIELD_SIZE_Y) 
    {
        return false;
    }

    while (true)
    {
        switch (direction)
        {
            case 0: posX--; break; // 左
            case 1: posX++; break; // 右
            case 2: posY--; break; // 下
            case 3: posY++; break; // 上
            case 4: posX++; posY++; break; // 右上
            case 5: posX--; posY--; break; // 左下
            case 6: posX--; posY++; break; // 左上
            case 7: posX++; posY--; break; // 右下
        }

        // 範囲外なら終了
        if (posX < 0 || posX >= FIELD_SIZE_X || posY < 0 || posY >= FIELD_SIZE_Y)
        {
            return false;
        }

        // 相手のコマならリストに追加
        if (_FieldState[posX, posY] == opponentPlayerTurn)
        {
            opponentInfoList.Add((posX, posY));
            continue;
        }

        // もし相手のコマが挟まれていて、現在のプレイヤーのコマにぶつかったらOK
        if (opponentInfoList.Count > 0 && _FieldState[posX, posY] == _PlayerTurn)
        {
            localTurnCheck = true;
            _InfoList.AddRange(opponentInfoList);
            break;
        }

        // それ以外（空マスまたは自分のコマが続いた場合）は無効
        break;
    }

    return localTurnCheck;
}



    public void OnTurnBegins(int turn)
{
    Debug.Log("Turn begins: " + turn);
}

public void OnTurnCompleted(int turn)
{
    Debug.Log("Turn completed: " + turn);
}
public void OnPlayerMove(Player player, int turn, object move)
{
    Debug.Log("Player move: " + player.NickName + " Turn: " + turn);

    // move オブジェクトを Vector3 にキャスト
    Vector3 movePosition = (Vector3)move;
    int posX = (int)movePosition.x;
    int posY = (int)movePosition.y;
    SpriteState playerTurn = (SpriteState)(int)movePosition.z;

    // _InfoList を int[] に変換
    int[] infoArray = _InfoList.SelectMany(info => new int[] { info.Item1, info.Item2 }).ToArray();

    // PlaceStone メソッドを呼び出して駒を置く
    photonView.RPC("PlaceStone", RpcTarget.All, movePosition, infoArray);
}

public void OnPlayerFinished(Player player, int turn, object move)
{
    Debug.Log("Player finished: " + player.NickName + " Turn: " + turn);
}

public void OnTurnTimeEnds(int turn)
{
    Debug.Log("Turn time ends: " + turn);
}
}
