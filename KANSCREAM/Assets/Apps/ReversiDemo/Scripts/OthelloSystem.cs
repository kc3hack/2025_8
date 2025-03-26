using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Linq;
using UnityEngine.UI;
using System.Threading.Tasks;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.Video;
public class OthelloSystem : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
{
    public GameObject KantoStone;//オセロ駒オブジェクト
    public GameObject KansaiStone;//オセロ駒オブジェクト
    public GameObject SelectedFieldCube;//選択中のフィールドを示すオブジェクト
    //public GameObject restartButton; // 関西プレイヤー用のボタンプレハブ
    //private GameObject restartButton; // 関西プレイヤー用のボタンインスタンス
    const int FIELD_SIZE_X = 6;//盤のサイズ
    const int FIELD_SIZE_Y = 6;//盤のサイズ
    private int SelectedFieldCubePosX;//指定しているマスのX座標
    private int SelectedFieldCubePosY;//指定しているマスのY座標
    private bool isPushButton = false;

    private bool _KantoCheckFlag = true;
    private bool _KansaiCheckFlag = true;
    private SpriteState _PlayerTurn = SpriteState.KANSAI;//プレイヤーのターン(関東先手)
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
    [SerializeField] private AudioSource _gameBGM;
    [SerializeField] private AudioSource _winBGM;
    [SerializeField] private AudioSource _loseBGM;
    [SerializeField] private AudioSource _betraySE;
    [SerializeField] private AudioSource _shineSE;
    [SerializeField] private AudioSource _screamBGM;
    [SerializeField] private GameObject _winnerBG;
    [SerializeField] private GameObject _looserBG;
    [SerializeField] private GameObject _nomalBG;
    [SerializeField] private GameObject _resultBG;


    [SerializeField] private Image CurrentPlayerKantoStone; // 現在のプレイヤーの関東コマを表示するオブジェクト
    [SerializeField] private Image CurrentPlayerKansaiStone; // 現在のプレイヤーの関西コマを表示するオブジェクト

    private SpriteState CurrentPlayerTurn = SpriteState.KANSAI; // 現在のプレイヤーのターン

    private int passNum = 0; // パスした回数

    private bool isKanScreamRPCCompleted = false; // 非同期のメソッドが終了したかのフラグ
    private bool isReverseKantoRPCCompleted = false; // 非同期のメソッドが終了したかのフラグ

     [SerializeField] private Image victoryImage; // 勝利イメージ
    [SerializeField] private Image defeatImage; // 敗北イメージ
    [SerializeField] private Image whiteBack; // ホワイトバック

    [SerializeField] private TextMeshProUGUI finishKantoStoneNum; // 現在のプレイヤーの関東コマの数を表示するテキスト 
    [SerializeField] private TextMeshProUGUI finishKansaiStoneNum; // 現在のプレイヤーの関西コマの数を表示するテキスト

    [SerializeField] private Image finishKantoStone;
    [SerializeField] private Image finishKansaiStone;

  [SerializeField] private TurnText turnText;
  private bool finished = false;

    void Start()
    {
        // それぞれに対応したAudioSourceコンポーネントを取得する
        _gameBGM = GetComponent<AudioSource>().GetComponents<AudioSource>()[0];
        _winBGM = GetComponent<AudioSource>().GetComponents<AudioSource>()[1];
        _loseBGM = GetComponent<AudioSource>().GetComponents<AudioSource>()[2];
        _betraySE = GetComponent<AudioSource>().GetComponents<AudioSource>()[4];
        _shineSE = GetComponent<AudioSource>().GetComponents<AudioSource>()[3];
        _screamBGM = GetComponent<AudioSource>().GetComponents<AudioSource>()[5];
        _gameBGM.Play();


        //選択中のフィールドを示すオブジェクトの初期位置を設定
        SelectedFieldCubePosX = (int)SelectedFieldCube.transform.position.x;
        SelectedFieldCubePosY = (int)SelectedFieldCube.transform.position.z;

        // Awake();

        //初期配置  関東:黒  関西:白
        _KantoStoneObj[3, 2].SetState(SpriteState.KANTO);
        _KansaiStoneObj[2, 2].SetState(SpriteState.KANSAI);
        _KantoStoneObj[2, 3].SetState(SpriteState.KANTO);
        _KansaiStoneObj[3, 3].SetState(SpriteState.KANSAI);

        // _KantoStoneObj[0,0].SetState(SpriteState.KANTO);
        _KantoStoneObj[5, 0].SetState(SpriteState.KANTO);
        // _KantoStoneObj[5,5].SetState(SpriteState.KANTO);
        _KantoStoneObj[0, 5].SetState(SpriteState.KANTO);

        _FieldState[3, 2] = SpriteState.KANTO;
        _FieldState[2, 2] = SpriteState.KANSAI;
        _FieldState[2, 3] = SpriteState.KANTO;
        _FieldState[3, 3] = SpriteState.KANSAI;

        // _FieldState[0, 0] = SpriteState.KANTO;
        _FieldState[5, 0] = SpriteState.KANTO;
        // _FieldState[5, 5] = SpriteState.KANTO;
        _FieldState[0, 5] = SpriteState.KANTO;

        turnManager = FindObjectOfType<PunTurnManager>();
        if (turnManager == null)
        {
            // Debug.LogError("PunTurnManager component is missing from this GameObject");
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

        //ResetBG();
        NomalBGCheange();
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
        _gameBGM.Stop();
        // ボタンがクリックされたときの処理
        // Debug.Log("Kansai button clicked");

        GetComponent<AudioRecorder>().StartRecording();
        StartCoroutine(WaitAndCheckSimilarity(duration));
    }

    public void onClick() {
        photonView.RPC("ReStartReversi", RpcTarget.All);
    }

    [PunRPC]
    public void ReStartReversi() {
        // 盤面の初期化
    for (int y = 0; y < FIELD_SIZE_Y; y++)
    {
        for (int x = 0; x < FIELD_SIZE_X; x++)
        {
            _FieldState[x, y] = SpriteState.NONE;
            _KantoStoneObj[x, y].SetState(SpriteState.NONE);
            _KansaiStoneObj[x, y].SetState(SpriteState.NONE);
        }
    }

    Awake();

    // 初期配置 関東:黒 関西:白
    _KantoStoneObj[3, 2].SetState(SpriteState.KANTO);
    _KansaiStoneObj[2, 2].SetState(SpriteState.KANSAI);
    _KantoStoneObj[2, 3].SetState(SpriteState.KANTO);
    _KansaiStoneObj[3, 3].SetState(SpriteState.KANSAI);

    _FieldState[3, 2] = SpriteState.KANTO;
    _FieldState[2, 2] = SpriteState.KANSAI;
    _FieldState[2, 3] = SpriteState.KANTO;
    _FieldState[3, 3] = SpriteState.KANSAI;


    _KantoStoneObj[5, 0].SetState(SpriteState.KANTO);
    _KantoStoneObj[0, 5].SetState(SpriteState.KANTO);
    _FieldState[5, 0] = SpriteState.KANTO;
    _FieldState[0, 5] = SpriteState.KANTO;

    // ターンの初期化
    _PlayerTurn = SpriteState.KANTO;
    CurrentPlayerTurn = SpriteState.KANTO;
    // turnManagerのturnを0にする
    turnManager.SetTurn(1);

    isPushButton = false;
    _KantoCheckFlag = true;
    _KansaiCheckFlag = true;
    turnCheck = false;
    passNum = 0;

    finishKantoStoneNum.gameObject.SetActive(false);
    finishKansaiStoneNum.gameObject.SetActive(false);
    whiteBack.gameObject.SetActive(false);
    //restartButton.gameObject.SetActive(false);
    finishKantoStone.gameObject.SetActive(false);
    finishKansaiStone.gameObject.SetActive(false);
    victoryImage.gameObject.SetActive(false);
    defeatImage.gameObject.SetActive(false);
    _resultBG.gameObject.SetActive(false);

    CurrentPlayerKansaiStone.gameObject.SetActive(true);
    CurrentPlayerKantoStone.gameObject.SetActive(true);
    //CurrentPlayerTurn.gameObject.SetActive(true);
    turnText.TurnTextOn();

    _winBGM.Stop();
    _loseBGM.Stop();
    _screamBGM.Stop();
    _gameBGM.Play();
    NomalBGCheange();
    finished = false;
    }

    IEnumerator<object> WaitAndCheckSimilarity(float duration)
    {
        // 3秒待機
        yield return new WaitForSeconds(duration);

        // 類似度を取得して処理を行う
        float similarity = GetComponent<AudioRecorder>().GetSimilarity();
        // Debug.Log("OthelloSystem 類似度: " + similarity);

        JudgeScream().Forget();

        if (similarity < 10000f)
        {
            PlayScreamBGM().Forget();
            KanScream(similarity);
        }
        else
        {
            _gameBGM.UnPause();
        }

    }

    private async UniTask PlayScreamBGM()
    {
        await UniTask.Delay(5000);
        _screamBGM.Play();
    }

    private async UniTask JudgeScream()
    {
        _shineSE.PlayOneShot(_shineSE.clip);
        await UniTask.Delay(1000);
        _betraySE.PlayOneShot(_betraySE.clip);
        await UniTask.Delay(2000);
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

        //photonView.RPC("UpdateCurrentPlayerStone", RpcTarget.All);
    }

    private void WinnerBGChange()
    {
        ResetBG();
        _winnerBG.SetActive(true);
        _winnerBG.GetComponent<VideoPlayer>().Play();
    }

    private void LooserBGChange()
    {
        ResetBG();
        _looserBG.SetActive(true);
        _looserBG.GetComponent<VideoPlayer>().Play();
    }

    private void NomalBGCheange()
    {
        ResetBG();
        _nomalBG.SetActive(true);
        // _nomalBG.GetComponent<VideoPlayer>().Play();
    }

    private void ResetBG()
    {
        _winnerBG.SetActive(false);
        _looserBG.SetActive(false);
        _nomalBG.SetActive(false);
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
                                        , new Vector3(x, 0, y)
                                        , Quaternion.Euler(0, 0, 0));
                var kansai = Instantiate(KansaiStone
                                        , new Vector3(x, 0, y)
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
    private void UpdateCurrentPlayerStone()
    {
        if(finished) return;

        if (_PlayerTurn == SpriteState.KANTO)
        {
            CurrentPlayerKantoStone.gameObject.SetActive(true);
            CurrentPlayerKansaiStone.gameObject.SetActive(false);
        }
        else
        {
            CurrentPlayerKantoStone.gameObject.SetActive(false);
            CurrentPlayerKansaiStone.gameObject.SetActive(true);
        }
    }

    private void UpdateSelectedFieldPosition()
    {
        var position = SelectedFieldCube.transform.position;

        // if (!_KantoCheckFlag)
        // {
        //     turnManager.SendMove(null, true);
        //     turnManager.BeginTurn();
        // }

        // if (!_KansaiCheckFlag)
        // {
        //     turnManager.SendMove(null, true);
        //     turnManager.BeginTurn();
        // }

        //選択中のフィールドを移動
        if (Input.GetKeyDown(KeyCode.W) && SelectedFieldCubePosY < FIELD_SIZE_Y - 1)
        {
            SelectedFieldCubePosY++;
            SelectedFieldCube.transform.position = new Vector3(position.x, position.y, position.z + 1);
        }
        else if (Input.GetKeyDown(KeyCode.S) && SelectedFieldCubePosY > 0)
        {
            SelectedFieldCubePosY--;
            SelectedFieldCube.transform.position = new Vector3(position.x, position.y, position.z - 1);
        }
        else if (Input.GetKeyDown(KeyCode.A) && SelectedFieldCubePosX > 0)
        {
            SelectedFieldCubePosX--;
            SelectedFieldCube.transform.position = new Vector3(position.x - 1, position.y, position.z);
        }
        else if (Input.GetKeyDown(KeyCode.D) && SelectedFieldCubePosX < FIELD_SIZE_X - 1)
        {
            SelectedFieldCubePosX++;
            SelectedFieldCube.transform.position = new Vector3(position.x + 1, position.y, position.z);
        }

        //スペースキーで録音開始
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(_PlayerTurn + "のターン");
            if (!isKantoPlayer)
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

            // if (_KantoCheckFlag)
            // {
            //     Debug.Log("関東は置けるマスがある");
            // }
            // else
            // {
            //     Debug.Log("関東は置けるマスがない");
            // }
            // if (_KansaiCheckFlag)
            // {
            //     Debug.Log("関西は置けるマスがある");
            // }
            // else
            // {
            //     Debug.Log("関西は置けるマスがない");
            // }

            // Debug.Log("115:turnCheck: " + turnCheck);

            if (turnCheck && _FieldState[SelectedFieldCubePosX, SelectedFieldCubePosY] == SpriteState.NONE)
            {
                var playerTurn = isKantoPlayer ? SpriteState.KANTO : SpriteState.KANSAI;
                Vector3 move = new Vector3(SelectedFieldCubePosX, SelectedFieldCubePosY, (int)playerTurn);

                // _InfoList を int[] に変換
                //int[] infoArray = _InfoList.SelectMany(info => new int[] { info.Item1, info.Item2 }).ToArray();

                //CurrentPlayerTurn = CurrentPlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
                turnManager.SendMove(move, false);
                //photonView.RPC("PlaceStone", RpcTarget.All, move, infoArray);
                turnManager.SendMove(null, true);
                turnManager.BeginTurn();

                // if (!FieldStateCheck(_PlayerTurn))
                // {
                //     //CurrentPlayerTurn = CurrentPlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
                //     turnManager.SendMove(null, true);
                //     turnManager.BeginTurn();

                //     // 次のプレイヤーのターンをチェック
                //     var nextPlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
                //     if (!FieldStateCheck(nextPlayerTurn))
                //     {
                //         //CurrentPlayerTurn = CurrentPlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
                //         // 次のプレイヤーも置けない場合はターンを飛ばす
                //         turnManager.SendMove(null, true);
                //         turnManager.BeginTurn();
                //     }
                // }
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            turnManager.SendMove(null, true);
            turnManager.BeginTurn();
            photonView.RPC("ClacPassNum", RpcTarget.All);
        }
    }

    [PunRPC]
    private void ClacPassNum()
    {
        passNum++;
        if (passNum == 2)
        {
            _KansaiCheckFlag = false;
            _KantoCheckFlag = false;
            CalcTotalStoneNum();
        }
    }

    [PunRPC]
    private void PlaceStone(Vector3 move, int[] infoArray)
    {
        passNum = 0;//連続のパス回数をリセット
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
                ReverseKansai(posX, posY).Forget();
            }
            else if (playerTurn == SpriteState.KANSAI)
            {
                ReverseKanto(posX, posY).Forget();
            }
        }

        CheckCanSettingStone();

        _InfoList.Clear();
    }

    private async UniTask ReverseKansai(int posX, int posY)
    {
        await _KansaiStoneObj[posX, posY].transform.DORotate(new Vector3(0, 0, 90), 0.1f);
        _KansaiStoneObj[posX, posY].SetState(SpriteState.NONE);
        _KantoStoneObj[posX, posY].transform.rotation = Quaternion.Euler(0, 180, 90);
        _KantoStoneObj[posX, posY].SetState(SpriteState.KANTO);
        await _KantoStoneObj[posX, posY].transform.DORotate(new Vector3(0, 180, 0), 0.1f);
    }

    private async UniTask ReverseKanto(int posX, int posY)
    {
        await _KantoStoneObj[posX, posY].transform.DORotate(new Vector3(0, 0, 90), 0.1f);
        _KantoStoneObj[posX, posY].SetState(SpriteState.NONE);
        _KansaiStoneObj[posX, posY].transform.rotation = Quaternion.Euler(0, 180, 90);
        _KansaiStoneObj[posX, posY].SetState(SpriteState.KANSAI);
        await _KansaiStoneObj[posX, posY].transform.DORotate(new Vector3(0, 180, 0), 0.1f);
        isReverseKantoRPCCompleted = true;
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
                // Debug.Log("関東の勝ち");
                ShowResultPanel(true);
            }
            else if (kantoStoneNum < kansaiStoneNum)
            {
                // Debug.Log("関西の勝ち");
                ShowResultPanel(false);
            }
            else
            {
                // Debug.Log("引き分け");
            }
        }
    }

    public async UniTask KanScream(float similarity)
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

        // Kantoのコマが1つ以上ある場合
        if (kantoPositions.Count >= 5)
        {
            var selectedPositions = new List<(int, int)>();
            
            if (similarity < 3000f)
            {
                // ランダムに5つの位置を選択
                System.Random rand = new System.Random();
                selectedPositions = kantoPositions.OrderBy(_ => rand.Next()).Take(5).ToList();
            }
            else if (similarity < 4000f)
            {
                // ランダムに4つの位置を選択
                System.Random rand = new System.Random();
                selectedPositions = kantoPositions.OrderBy(_ => rand.Next()).Take(4).ToList();
            }
            else if (similarity < 5500f)
            {
                // ランダムに3つの位置を選択
                System.Random rand = new System.Random();
                selectedPositions = kantoPositions.OrderBy(_ => rand.Next()).Take(3).ToList();
            }
            else if (similarity < 7000f)
            {
                // ランダムに2つの位置を選択
                System.Random rand = new System.Random();
                selectedPositions = kantoPositions.OrderBy(_ => rand.Next()).Take(2).ToList();
            }
            else
            {
                // ランダムに1つの位置を選択
                System.Random rand = new System.Random();
                selectedPositions = kantoPositions.OrderBy(_ => rand.Next()).Take(1).ToList();
            }

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

                // フラグを初期化
                isKanScreamRPCCompleted = false;
                // 変更を同期
                photonView.RPC("KanScreamRPC", RpcTarget.All, x, y, infoList.ToArray());
                infoList.Clear();
                // フラグが設定されるまで待機
                await UniTask.WaitUntil(() => isKanScreamRPCCompleted);
            }
        }
    }

    /// <summary>
    /// 叫んだ時にひっくり返す
    /// アニメーション
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="infoArray"></param>
    /// <returns></returns>
    [PunRPC]
    private async UniTask KanScreamRPC(int x, int y, int[] infoArray)
    {
        isKanScreamRPCCompleted = false; // 終了フラグをリセット
        var rotateNum = 360 * 3;

        // Kansaiコマに置き換え
        _FieldState[x, y] = SpriteState.KANSAI;
        await _KantoStoneObj[x, y].transform.DOLocalMoveY(1.5f, 0.5f).SetEase(Ease.OutBounce);
        // 0.5秒待機
        await UniTask.Delay(500);
        await _KantoStoneObj[x, y].transform.DORotate(new Vector3(rotateNum, rotateNum, rotateNum), 0.5f, RotateMode.FastBeyond360);
        _KantoStoneObj[x, y].transform.position = new Vector3(_KantoStoneObj[x, y].transform.position.x, 0, _KantoStoneObj[x, y].transform.position.z);
        _KantoStoneObj[x, y].SetState(SpriteState.NONE);
        _KansaiStoneObj[x, y].transform.position = new Vector3(_KansaiStoneObj[x, y].transform.position.x, 1.5f, _KansaiStoneObj[x, y].transform.position.z);
        _KansaiStoneObj[x, y].SetState(SpriteState.KANSAI);
        await _KansaiStoneObj[x, y].transform.DORotate(new Vector3(rotateNum, 180, 0), 0.2f);
        await UniTask.Delay(700);
        await _KansaiStoneObj[x, y].transform.DOLocalMoveY(0, 0.3f).SetEase(Ease.OutBounce);


        // ひっくり返す処理
        for (int i = 0; i < infoArray.Length; i += 2)
        {
            int posX = infoArray[i];
            int posY = infoArray[i + 1];
            // Kansaiコマに置き換え
            _FieldState[posX, posY] = SpriteState.KANSAI;
            ReverseKanto(posX, posY).Forget();
            await UniTask.WaitUntil(() => isReverseKantoRPCCompleted);
            isReverseKantoRPCCompleted = false;
        }

        isKanScreamRPCCompleted = true; // 終了フラグを設定
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
                //_PlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
            }
        }
        CalcTotalStoneNum();
    }

    /// <summary>
    /// コマを置けるかどうかの判定
    /// </summary>
    /// <returns></returns>
    private bool TurnCheck(int direction)
    {
        var posX = SelectedFieldCubePosX;//選択中のフィールドの移動範囲調整
        var posY = SelectedFieldCubePosY;//選択中のフィールドの移動範囲調整

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

   private void ShowResultPanel(bool isKantoWinner)
{
    Debug.Log("ShowResultPanel called");

    // コマの数を計算
    int kantoStoneNum = 0;
    int kansaiStoneNum = 0;
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

    // Debug.Log("Kanto Stone Num: " + kantoStoneNum);
    // Debug.Log("Kansai Stone Num: " + kansaiStoneNum);
    
    finished = true;
    CurrentPlayerKansaiStone.gameObject.SetActive(false);
    CurrentPlayerKantoStone.gameObject.SetActive(false);
    //CurrentPlayerTurn.gameObject.SetActive(false);


    finishKantoStoneNum.text = "×" + kantoStoneNum.ToString();
    finishKantoStoneNum.gameObject.SetActive(true);
    finishKansaiStoneNum.text = "×" + kansaiStoneNum.ToString();
    finishKansaiStoneNum.gameObject.SetActive(true);
    whiteBack.gameObject.SetActive(true);
    //restartButton.gameObject.SetActive(true); // リスタートボタンを表示
    finishKantoStone.gameObject.SetActive(true);
    finishKansaiStone.gameObject.SetActive(true);
    _resultBG.gameObject.SetActive(true);
    turnText.TurnTextOff();

    victoryImage.gameObject.SetActive(false);
    defeatImage.gameObject.SetActive(false);
    

    if (victoryImage != null && defeatImage != null)
    {
        if (isKantoPlayer && isKantoWinner)
        {
            WinnerBGChange();
            victoryImage.gameObject.SetActive(true);
            _gameBGM.Stop();
            _winBGM.Play();
        }
        else if (!isKantoPlayer && !isKantoWinner)
        {
            WinnerBGChange();
            victoryImage.gameObject.SetActive(true);
            _gameBGM.Stop();
            _winBGM.Play();
        }
        else
        {
            LooserBGChange();
            defeatImage.gameObject.SetActive(true);
            _gameBGM.Stop();
            _loseBGM.Play();
        }
    }
    else
    {
        Debug.LogError("victoryImage or defeatImage is not assigned");
    }

    Debug.Log("ShowResultPanel completed");
}

    public void OnTurnBegins(int turn)
    {
        // Debug.Log("Turn begins: " + turn);
        photonView.RPC("UpdateCurrentPlayerStone", RpcTarget.All); // ターンが始まったタイミングでUIを更新
        CurrentPlayerTurn = CurrentPlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
        _PlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
    }

    public void OnTurnCompleted(int turn)
    {
        // Debug.Log("Turn completed: " + turn);
    }
    public void OnPlayerMove(Player player, int turn, object move)
    {
        // Debug.Log("Player move: " + player.NickName + " Turn: " + turn);

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
        // Debug.Log("Player finished: " + player.NickName + " Turn: " + turn);
    }

    public void OnTurnTimeEnds(int turn)
    {
        // Debug.Log("Turn time ends: " + turn);
    }
}