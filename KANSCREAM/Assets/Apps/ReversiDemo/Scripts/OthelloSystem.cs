using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Linq;
using UnityEngine.UI;
public class OthelloSystem : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
{
    public GameObject KantoStone;//オセロ駒オブジェクト
    public GameObject KansaiStone;//オセロ駒オブジェクト
    public GameObject SelectedFieldCube;//選択中のフィールドを示すオブジェクト
    public GameObject KansaiButtonPrefab; // 関西プレイヤー用のボタンプレハブ
    private GameObject kansaiButtonInstance; // 関西プレイヤー用のボタンインスタンス
    const int FIELD_SIZE_X = 6;//盤のサイズ
    const int FIELD_SIZE_Y = 6;//盤のサイズ
    private int SelectedFieldCubePosX;//指定しているマスのX座標
    private int SelectedFieldCubePosY;//指定しているマスのY座標
    private bool isPushButton = false;

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
    private float duration = 3.0f;

    void Start()
    {
        //選択中のフィールドを示すオブジェクトの初期位置を設定
        SelectedFieldCubePosX = (int)SelectedFieldCube.transform.position.x;
        SelectedFieldCubePosY = (int)SelectedFieldCube.transform.position.z;

        Awake();

        //初期配置  関東:黒  関西:白
        _KantoStoneObj[3, 2].SetState(SpriteState.KANTO);
        _KansaiStoneObj[2, 2].SetState(SpriteState.KANSAI);
        _KantoStoneObj[2, 3].SetState(SpriteState.KANTO);
        _KansaiStoneObj[3, 3].SetState(SpriteState.KANSAI);

        // _KantoStoneObj[0,0].SetState(SpriteState.KANTO);
        _KantoStoneObj[5,0].SetState(SpriteState.KANTO);
        // _KantoStoneObj[5,5].SetState(SpriteState.KANTO);
        _KantoStoneObj[0,5].SetState(SpriteState.KANTO);

        _FieldState[3, 2] = SpriteState.KANTO;
        _FieldState[2, 2] = SpriteState.KANSAI;
        _FieldState[2, 3] = SpriteState.KANTO;
        _FieldState[3, 3] = SpriteState.KANSAI;

        // _FieldState[0, 0] = SpriteState.KANTO;
        _FieldState[5, 0] = SpriteState.KANTO;
        // _FieldState[5, 5] = SpriteState.KANTO;
        _FieldState[0, 5] = SpriteState.KANTO;

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
            // CreateKansaiButton(); // 関西プレイヤー用のボタンを生成
        }
    }

    // void CreateKansaiButton()
    // {
    //     if (KansaiButtonPrefab != null)
    //     {
    //         kansaiButtonInstance = Instantiate(KansaiButtonPrefab, GameObject.Find("Canvas").transform);
    //         kansaiButtonInstance.GetComponent<Button>().onClick.AddListener(OnKansaiButtonClick);
    //     }
    // }

    void OnKansaiButtonClick()
    {
        // ボタンがクリックされたときの処理
        Debug.Log("Kansai button clicked");

        GetComponent<AudioRecorder>().StartRecording();
        StartCoroutine(WaitAndCheckSimilarity(duration));
    }

    IEnumerator<object> WaitAndCheckSimilarity(float duration)
    {
        // 3秒待機
        yield return new WaitForSeconds(duration);

        // 類似度を取得して処理を行う
        float similarity = GetComponent<AudioRecorder>().GetSimilarity();
        Debug.Log("OthelloSystem 類似度: " + similarity);

        if (similarity < 6000f)
        {
            KanScream();
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

    private void UpdateSelectedFieldPosition()
    {
        var position = SelectedFieldCube.transform.position;

        if (!_KantoCheckFlag) {
            turnManager.SendMove(null, true);
            turnManager.BeginTurn();
        }

        if(!_KansaiCheckFlag) {
            turnManager.SendMove(null, true);
            turnManager.BeginTurn();
        }

        //選択中のフィールドを移動
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

        //スペースキーで録音開始
        if(Input.GetKeyDown(KeyCode.Space)){
            //Debug.Log(_PlayerTurn + "のターン");
            if(!isKantoPlayer)
            {
                OnKansaiButtonClick();
            }
        }

        //エンターキーで駒を置く
        if (Input.GetKeyDown(KeyCode.Return))
        {
            turnCheck = false;
            //全方向に対してコマを置けるかどうかの判定
            for (int i = 0; i < 8; i++)
            {
                if (TurnCheck(i))
                {
                    turnCheck = true;
                }
            }

            if(_KantoCheckFlag){
                Debug.Log("関東は置けるマスがある");
            }else{
                Debug.Log("関東は置けるマスがない");
            }
            if(_KansaiCheckFlag){
                Debug.Log("関西は置けるマスがある");
            }else{
                Debug.Log("関西は置けるマスがない");
            }

            // Debug.Log("115:turnCheck: " + turnCheck);

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

                if(!FieldStateCheck(_PlayerTurn))
                {
                    turnManager.SendMove(null, true);
                    turnManager.BeginTurn();

                    // 次のプレイヤーのターンをチェック
                    var nextPlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
                    if(!FieldStateCheck(nextPlayerTurn))
                    {
                        // 次のプレイヤーも置けない場合はターンを飛ばす
                        turnManager.SendMove(null, true);
                        turnManager.BeginTurn();
                    }
                }
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

private bool FieldStateCheck(SpriteState playerTurn)
{
    for (int y = 0; y < FIELD_SIZE_Y; y++)
    {
        for (int x = 0; x < FIELD_SIZE_X; x++)
        {
            if (_FieldState[x, y] == SpriteState.NONE)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (TurnCheck(i, x, y, playerTurn))
                    {
                        return true;
                    }
                }
            }
        }
    }
    return false;
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

        // Debug.Log("関東の石の数: " + kantoStoneNum);
        // Debug.Log("関西の石の数: " + kansaiStoneNum);

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

public void KanScream()
{
    if (isPushButton) return;
    isPushButton = true;

    // Kantoのコマの位置をリストに収集
    List<(int, int)> kantoPositions = new List<(int, int)>();
    for (int y = 0; y < FIELD_SIZE_Y; y++)
    {
        for (int x = 0; x < FIELD_SIZE_X; x++)
        {
            if (_FieldState[x, y] == SpriteState.KANTO)
            {
                kantoPositions.Add((x, y));
            }
        }
    }

    // Kantoのコマが2つ以上ある場合
    if (kantoPositions.Count >= 2)
    {
        // ランダムに2つの位置を選択
        System.Random rand = new System.Random();
        var selectedPositions = kantoPositions.OrderBy(_ => rand.Next()).Take(2).ToList();

        // ひっくり返す位置を収集
        List<int> infoList = new List<int>();
        foreach (var pos in selectedPositions)
        {
            int x = pos.Item1;
            int y = pos.Item2;

            for (int i = 0; i < 8; i++)
            {
                if (TurnCheck(i, x, y))
                {
                    foreach (var info in _InfoList)
                    {
                        infoList.Add(info.Item1);
                        infoList.Add(info.Item2);
                    }
                    _InfoList.Clear();
                }
            }
        }

        // 変更を同期
        photonView.RPC("KanScreamRPC", RpcTarget.All, selectedPositions[0].Item1, selectedPositions[0].Item2, selectedPositions[1].Item1, selectedPositions[1].Item2, infoList.ToArray());
    }
}

[PunRPC]
private void KanScreamRPC(int x1, int y1, int x2, int y2, int[] infoArray)
{
    List<(int, int)> positions = new List<(int, int)> { (x1, y1), (x2, y2) };

    foreach (var pos in positions)
    {
        int x = pos.Item1;
        int y = pos.Item2;

        // Kansaiコマに置き換え
        _FieldState[x, y] = SpriteState.KANSAI;
        _KantoStoneObj[x, y].SetState(SpriteState.NONE);
        _KansaiStoneObj[x, y].SetState(SpriteState.KANSAI);
    }

    // ひっくり返す処理
    for (int i = 0; i < infoArray.Length; i += 2)
    {
        int posX = infoArray[i];
        int posY = infoArray[i + 1];
        _FieldState[posX, posY] = SpriteState.KANSAI;
        _KantoStoneObj[posX, posY].SetState(SpriteState.NONE);
        _KansaiStoneObj[posX, posY].SetState(SpriteState.KANSAI);
    }
}


    /// <summary>
    /// 置ける場所があるかどうかを判定
    /// </summary>
    private void CheckCanSettingStone()
    {
        for (int y = 0; y < FIELD_SIZE_Y; y++)
        {
            for (int x = 0; x < FIELD_SIZE_X; x++)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (TurnCheck(i, x, y) && _FieldState[x, y] == SpriteState.NONE)
                    {
                        if (_PlayerTurn == SpriteState.KANTO)
                        {
                            _KantoCheckFlag = true;
                            turnCheck = true;
                            if (!_KansaiCheckFlag)
                            {
                                _KansaiCheckFlag = true;
                            }
                            break;
                        }
                        else if (_PlayerTurn == SpriteState.KANSAI)
                        {
                            _KansaiCheckFlag = true;
                            turnCheck = true;
                            if (!_KantoCheckFlag)
                            {
                                _KantoCheckFlag = true;
                            }
                            break;
                        }
                    }
                }
            }
            //一箇所も置ける場所がない場合
            if (!turnCheck)
            {
                if (_PlayerTurn == SpriteState.KANTO)
                {
                    _KantoCheckFlag = false;
                }
                else if (_PlayerTurn == SpriteState.KANSAI)
                {
                    _KansaiCheckFlag = false;
                }
                _PlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
            }
            CalcTotalStoneNum();
        }
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
        int i = 0;

        while (0 <= posX && posX <= FIELD_SIZE_X && 0 <= posY && posY <= FIELD_SIZE_Y)
        {
            i++;
            switch (direction)
            {
                case 0://左
                    if (posX == 0) { return localTurnCheck; }
                    posX--;
                    // Debug.Log(i+"番目の左のコマ: " + _FieldState[posX, posY]);
                    break;
                case 1://右
                    if (posX == FIELD_SIZE_X - 1) { return localTurnCheck; }
                    posX++;
                    // Debug.Log(i+"番目の右のコマ: " + _FieldState[posX, posY]);
                    break;
                case 2://下
                    if (posY == 0) { return localTurnCheck; }
                    posY--;
                    // Debug.Log(i+"番目の下のコマ: " + _FieldState[posX, posY]);
                    break;
                case 3://上
                    if (posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
                    posY++;
                    // Debug.Log(i+"番目の上のコマ: " + _FieldState[posX, posY]);
                    break;
                case 4://右上
                    if (posX == FIELD_SIZE_X - 1 || posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
                    posX++;
                    posY++;
                    // Debug.Log(i+"番目の右上のコマ: " + _FieldState[posX, posY]);
                    break;
                case 5://左下
                    if (posX == 0 || posY == 0) { return localTurnCheck; }
                    posX--;
                    posY--;
                    // Debug.Log(i+"番目の左下のコマ: " + _FieldState[posX, posY]);
                    break;
                case 6://左上
                    if (posX == 0 || posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
                    posX--;
                    posY++;
                    // Debug.Log(i+"番目の左上のコマ: " + _FieldState[posX, posY]);
                    break;
                case 7://右下
                    if (posX == FIELD_SIZE_X - 1 || posY == 0) { return localTurnCheck; }
                    posX++;
                    posY--;
                    // Debug.Log(i+"番目の右下のコマ: " + _FieldState[posX, posY]);
                    break;
            }

            //指定した方向に相手のコマがあるときその情報をリストに追加
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

            //2つ以上隣のコマが空白の場合メソッドを終了
            if (opponentInfoList.Count > 0 && (_FieldState[posX, posY] == SpriteState.NONE))
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
        // Debug.Log("242:turnCheck: " + localTurnCheck);
        return localTurnCheck;
    }

    /// <summary>
    /// コマを置けるかどうかの判定
    /// </summary>
    /// <returns></returns>
    private bool TurnCheck(int direction, int pointX, int pointY)
    {
        var posX = pointX;//選択中のフィールドの移動範囲調整
        var posY = pointY;//選択中のフィールドの移動範囲調整

        var opponentPlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;

        var opponentInfoList = new List<(int, int)>();
        var localTurnCheck = false;
        int i = 0;

        while (0 <= posX && posX <= FIELD_SIZE_X && 0 <= posY && posY <= FIELD_SIZE_Y)
        {
            i++;
            switch (direction)
            {
                case 0://左
                    if (posX == 0) { return localTurnCheck; }
                    posX--;
                    // Debug.Log(i+"番目の左のコマ: " + _FieldState[posX, posY]);
                    break;
                case 1://右
                    if (posX == FIELD_SIZE_X - 1) { return localTurnCheck; }
                    posX++;
                    // Debug.Log(i+"番目の右のコマ: " + _FieldState[posX, posY]);
                    break;
                case 2://下
                    if (posY == 0) { return localTurnCheck; }
                    posY--;
                    // Debug.Log(i+"番目の下のコマ: " + _FieldState[posX, posY]);
                    break;
                case 3://上
                    if (posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
                    posY++;
                    // Debug.Log(i+"番目の上のコマ: " + _FieldState[posX, posY]);
                    break;
                case 4://右上
                    if (posX == FIELD_SIZE_X - 1 || posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
                    posX++;
                    posY++;
                    // Debug.Log(i+"番目の右上のコマ: " + _FieldState[posX, posY]);
                    break;
                case 5://左下
                    if (posX == 0 || posY == 0) { return localTurnCheck; }
                    posX--;
                    posY--;
                    // Debug.Log(i+"番目の左下のコマ: " + _FieldState[posX, posY]);
                    break;
                case 6://左上
                    if (posX == 0 || posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
                    posX--;
                    posY++;
                    // Debug.Log(i+"番目の左上のコマ: " + _FieldState[posX, posY]);
                    break;
                case 7://右下
                    if (posX == FIELD_SIZE_X - 1 || posY == 0) { return localTurnCheck; }
                    posX++;
                    posY--;
                    // Debug.Log(i+"番目の右下のコマ: " + _FieldState[posX, posY]);
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
        // Debug.Log("242:turnCheck: " + localTurnCheck);
        return localTurnCheck;
    }

    private bool TurnCheck(int direction, int pointX, int pointY, SpriteState playerTurn)
{
    var posX = pointX;
    var posY = pointY;

    var opponentPlayerTurn = playerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;

    var opponentInfoList = new List<(int, int)>();
    var localTurnCheck = false;
    int i = 0;

    while (0 <= posX && posX < FIELD_SIZE_X && 0 <= posY && posY < FIELD_SIZE_Y)
    {
        i++;
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

        //指定した方向に相手のコマがあるときその情報をリストに追加
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

            //2つ以上隣のコマが空白の場合メソッドを終了
            if (opponentInfoList.Count > 0 && (_FieldState[posX, posY] == SpriteState.NONE))
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