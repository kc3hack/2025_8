using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace refactor
{
    /// <summary>
    /// 盤面の管理を行うクラス
    /// 盤面の状態を保持し、駒を置く処理やターンの交代を行う
    /// 盤面の状態は2次元配列で保持し、各マスの状態を管理する
    /// 駒の表示は親オブジェクトを持ち、子オブジェクトとして駒を配置する
    /// 駒の表示は非アクティブにしておき、駒を置くときにアクティブにする
    /// 駒の種類は関東と関西の2種類を用意し、ターンごとに交代する
    /// </summary>
    public class BoardManager : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
    {
        [SerializeField] private GameObject _kantoPiece;// 関東の駒
        [SerializeField] private GameObject _kansaiPiece;// 関西の駒
        [SerializeField] private GameObject _kantoParent;// 関東の駒をまとめる親オブジェクト
        [SerializeField] private GameObject _kansaiParent;// 関西の駒をまとめる親オブジェクト
        private int _specifiedPosX;// 現在のX座標
        private int _specifiedPosZ;// 現在のZ座標
        private BoardChecker _boardChecker;
        private KanScream _kanScream;

        public enum CellState
        {
            NONE,
            KANTO,
            KANSAI,
        }

        private CellState[,] _boardState;
        private CellState _turnState = CellState.KANTO;

        private PunTurnManager turnManager;
        public bool isKantoPlayer;
        private InGamePresenter _inGamePresenter;
        private Judge _judge;
        private float _similarity;
        private bool isSyncFlip = false;

        /// <summary>
        /// 盤面の初期化を行うメソッド
        /// 盤面の状態を初期化し、駒を配置する
        /// ターンの初期化も行う
        /// </summary>
        public void Initialize()
        {
            _boardState = new CellState[InGamePresenter.MAX_X, InGamePresenter.MAX_Z];
            _boardChecker = new BoardChecker();
            _inGamePresenter = GetComponent<InGamePresenter>();
            _judge = GetComponent<Judge>();
            _kanScream = new KanScream();
            _kanScream.Awake();
            for (int x = 0; x < InGamePresenter.MAX_X; x++)
            {
                for (int z = 0; z < InGamePresenter.MAX_Z; z++)
                {
                    _boardState[x, z] = CellState.NONE;
                    _boardChecker.SetCellState(_boardState);

                    var kantoPiece = Instantiate(_kantoPiece, _kantoParent.transform);
                    kantoPiece.transform.localPosition = new Vector3(x, 0.015f, z);
                    kantoPiece.SetActive(false);

                    var kansaiPiece = Instantiate(_kansaiPiece, _kansaiParent.transform);
                    kansaiPiece.transform.localPosition = new Vector3(x, 0.015f, z);
                    kansaiPiece.SetActive(false);
                }
            }

            turnManager = FindObjectOfType<PunTurnManager>();
            if (turnManager == null)
            {
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

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space) && !isKantoPlayer)
            {
                float duration = 3.0f;
                StartCoroutine(SyncKanScreamCoroutine(duration));
            }
        }

    
        public void KanScream(int x, int z)
        {
            photonView.RPC("SyncKanScream", RpcTarget.All, x, z);
        }

        private IEnumerator<object> SyncKanScreamCoroutine(float duration)
        {
            GetComponent<AudioRecorder>().StartRecording();
            yield return new WaitForSeconds(duration);
            _similarity = GetComponent<AudioRecorder>().GetSimilarity();
            _kanScream.kanScream(_similarity);
        }

        /// <summary>
        /// 関東の駒をひっくり返す処理
        /// </summary>
         [PunRPC]
        public void SyncKanScream(int x, int z)
        {
            // 関東の駒をひっくり返す処理
            _boardState[x, z] = CellState.NONE;
            _boardState[x, z] = CellState.KANSAI;
            var piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
            piece.SetActive(false);
            piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
            piece.SetActive(true);
            _turnState = CellState.KANSAI;
            _boardChecker.SetTurnState(_turnState);
            FlipPieces(x, z, CellState.KANSAI);
            _turnState = CellState.KANTO;
            _boardChecker.SetTurnState(_turnState);

        }

        /// <summary>
        /// 盤面の初期配置を行うメソッド
        /// 関東と関西の初期配置を行う
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public void InitializeSetUpPiece(int x, int z)
        {
            {
                if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
                {
                    //Debugger.Log($"無効な座標: ({x}, {z})");
                    return;
                }

                _specifiedPosX = x;
                _specifiedPosZ = z;

                _boardState[x, z] = _turnState;
                Show(x, z);
                //Debug.Log($"Before TurnChange: _turnState = {_turnState}");
                TurnChange(); // ターンを変更
                //Debug.Log($"After TurnChange: _turnState = {_turnState}");
            }
        }

        /// <summary>
        /// 指定した位置に駒を配置するメソッド
        /// ターン状態に応じて駒を配置し、ひっくり返す処理を行う
        /// ターン状態を変更し、他のクライアントに同期する
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="z">Z座標</param>
        public void SetUpPiece(int x, int z)
        {
            {
                //isKantoPlayerと _turnStateの状態を確認
                Debugger.Log($"isKantoPlayer: {isKantoPlayer}, _turnState: {_turnState}");
                // 自分のターンでない場合は入力を無効化
                if ((isKantoPlayer && _turnState != CellState.KANTO) || (!isKantoPlayer && _turnState != CellState.KANSAI))
                {
                    //Debug.Log("自分のターンではありません。");
                    return; // 入力を無視
                }

                // 座標が無効な場合は処理を終了
                if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
                {
                    Debugger.Log($"無効な座標: ({x}, {z})");
                    return;
                }

                // すでに駒が置かれている場合は処理を終了
                if (_boardState[x, z] != CellState.NONE)
                {
                    return;
                }

                _specifiedPosX = x;
                _specifiedPosZ = z;

                BoardChecker._PieceNum = 0;
                _judge.SetCanPlace(false);

                // 駒を置く処理
                if (TurnCheck())
                {
                    _boardState[x, z] = _turnState;
                    Show(x, z);
                    FlipPieces(); // 駒をひっくり返す処理

                    // 他のクライアントに同期
                    if (photonView != null)
                    {
                        Debug.Log($"Syncing piece placement: ({x}, {z}) with turn state: {_turnState}");
                        photonView.RPC("SyncSetUpPiece", RpcTarget.Others, x, z, _turnState);
                    }

                    TurnChange();
                    turnManager.BeginTurn();
                    _boardChecker.SetTurnState(_turnState == CellState.KANTO ? CellState.KANTO : CellState.KANSAI);
                }
                _judge.JudgeGame();
            }
        }

        /// <summary>
        /// 指定した位置に駒を配置するメソッド(同期用)
        /// ターン状態に応じて駒を配置し、ひっくり返す処理を行う
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="z">Z座標</param>
        /// <param name="turnState">ターン状態</param>
        [PunRPC]
        private void SyncSetUpPiece(int x, int z, CellState turnState)
        {
            // ボード状態を更新
            _boardState[x, z] = turnState;
            Show(x, z);

            // ひっくり返す処理を実行
            FlipPieces(x, z, turnState);
        }

        /// <summary>
        /// 指定した位置とターン状態からひっくり返すリストを取得する
        /// </summary>
        /// <param name="x">置いた位置のX座標</param>
        /// <param name="z">置いた位置のZ座標</param>
        /// <param name="turnState">現在のターン状態</param>
        /// <returns>ひっくり返す位置のリスト</returns>
        private List<(int, int)> GetFlipPositions(int x, int z, CellState turnState)
        {
            _boardChecker.SetPlayerInfo(x, z, _boardState, turnState);
            return _boardChecker.GetFlipPositionsForOpponentMove(x, z, turnState);
        }

        public async UniTask FlipPieces()
        {
            var flipPositions = _boardChecker.GetFlipPositions();
            
            if(flipPositions.Count == 0) {
                Debug.Log("flipPositions is empty");
            }
            foreach (var pos in flipPositions)
            {
                int x = pos.Item1;
                int z = pos.Item2;
                _boardState[x, z] = _turnState;
                Flip(x, z);
            }
            _boardChecker.ClearFlipPositions();
            flipPositions.Clear();
        }

        public async UniTask FlipPieces(int posX, int posZ, CellState _turnState)
        {
            var flipPositions = this.GetFlipPositions(posX, posZ, _turnState);
            
            if(flipPositions.Count == 0) {
                Debug.Log("flipPositions is empty");
            }
            foreach (var pos in flipPositions)
            {
                int x = pos.Item1;
                int z = pos.Item2;
                _boardState[x, z] = _turnState;
                Flip(x, z);
            }
            _boardChecker.ClearFlipPositions();
            flipPositions.Clear();
        }

        /// <summary>
        /// 指定されたマスに指定されたコマを表示する
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="z">Z座標</param>
        public async UniTask Show(int x, int z)
        {
            if (_turnState == CellState.KANTO)
            {
                var piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                piece.SetActive(true);
                piece.transform.position = new Vector3(x, 1, z);
                await piece.transform.DOMove(new Vector3(x, 0.015f, z), 0.2f);
            }
            else if (_turnState == CellState.KANSAI)
            {
                var piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                piece.SetActive(true);
                piece.transform.position = new Vector3(x, 1, z);
                await piece.transform.DOMove(new Vector3(x, 0.015f, z), 0.2f);
            }
        }

        /// <summary>
        /// 指定されたマスの駒をひっくり返すメソッド
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public async UniTask Flip(int x, int z)
        {
            await UniTask.Delay(200);// アニメーションの待機時間
            if (_turnState == CellState.KANSAI)
            {
                var piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                await piece.transform.DORotate(new Vector3(0, 180, -90), 0.2f);
                piece.SetActive(false);
                piece.transform.localRotation = Quaternion.Euler(0, 180, 0);

                piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                piece.SetActive(true);

                piece.transform.localRotation = Quaternion.Euler(0, 180, 90);//z軸に沿って90度回転させる
                await piece.transform.DORotate(new Vector3(0, 180, 0), 0.2f);// ひっくり返すアニメーション
            }
            else if (_turnState == CellState.KANTO)
            {
                var piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                await piece.transform.DORotate(new Vector3(0, 180, -90), 0.2f);
                piece.SetActive(false);
                piece.transform.localRotation = Quaternion.Euler(0, 180, 0);

                piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                piece.SetActive(true);
                
                piece.transform.localRotation = Quaternion.Euler(0, 180, 90);//z軸に沿って90度回転させる
                await piece.transform.DORotate(new Vector3(0, 180, 0), 0.2f);// ひっくり返すアニメーション
            }
        }


        /// <summary>
        /// ターン状態を同期するメソッド
        /// 全クライアントにターン状態を同期する
        /// </summary>
        [PunRPC]
        public void SyncTurnChange(CellState newTurnState)
        {
            Debug.Log($"SyncTurnChange called. New turn state: {newTurnState}");
            _turnState = newTurnState;
        }

        /// <summary>
        /// ターンを変更するメソッド
        /// ターン状態を変更し、全クライアントに同期する
        /// </summary>
        public void TurnChange()
        {
            // ターンを変更
            var newTurnState = _turnState == CellState.KANTO ? CellState.KANSAI : CellState.KANTO;

            Debug.Log($"TurnChange called. Current turn state: {_turnState}, New turn state: {newTurnState}");

            // 現在のターン状態を更新
            _turnState = newTurnState;

            // 全クライアントに同期
            if (photonView != null)
            {
                photonView.RPC("SyncTurnChange", RpcTarget.All, _turnState);
            }
            else
            {
                Debug.LogError("photonView is null. Ensure PhotonView is attached to the GameObject.");
            }
        }


        /// <summary>
        /// ターンのチェックを行うメソッド
        /// 指定した位置に駒を置けるかどうかを判定する
        /// </summary>
        private bool TurnCheck()
        {
            _boardChecker.SetPlayerInfo(_specifiedPosX, _specifiedPosZ, _boardState, _turnState);
            return _boardChecker.TurnCheck(_specifiedPosX, _specifiedPosZ);
        }

        /// <summary>
        /// 盤面の状態をリセットするメソッド
        /// 盤面の状態を初期化し、駒を非アクティブにする
        /// </summary>
        public void Reset()
        {

            _turnState = CellState.KANTO;
            for (int x = 0; x < InGamePresenter.MAX_X; x++)
            {
                for (int z = 0; z < InGamePresenter.MAX_Z; z++)
                {
                    _boardState[x, z] = CellState.NONE;
                    var piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                    piece.SetActive(false);
                    piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                    piece.SetActive(false);
                }
            }
            _kanScream.Reset();
        }



        public BoardChecker GetBoardChecker()
        {
            return _boardChecker;
        }

        public void InitializeSetCellState()
        {
            _boardChecker.SetCellState(_boardState);
        }

        public CellState GetTurnState()
        {
            return _turnState;
        }

        public CellState[,] GetBoardState()
        {
            return _boardState;
        }
    
        /// <summary>
        /// ターンの開始時に呼ばれるメソッド
        /// </summary>
        public void OnTurnBegins(int turn)
        {
            //_turnState = _turnState == CellState.KANTO ? CellState.KANSAI : CellState.KANTO;
            //Debug.Log($"Turn {turn} begins.");
            _inGamePresenter.SetSupportHundler();
        }

        public void OnTurnCompleted(int turn)
        {
            //Debug.Log($"Turn {turn} completed.");
        }

        public void OnPlayerMove(Player player, int turn, object move)
        {
            //Debug.Log($"Player {player.NickName} made a move on turn {turn}.");
        }

        public void OnPlayerFinished(Player player, int turn, object move)
        {
            //Debug.Log($"Player {player.NickName} finished their turn {turn}.");
        }

        public void OnTurnTimeEnds(int turn)
        {
            //Debug.Log($"Turn {turn} time ended.");
        }
    }
}