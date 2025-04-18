using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections.Generic;

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

        public void Initialize()
        {
            _boardState = new CellState[InGamePresenter.MAX_X, InGamePresenter.MAX_Z];
            _boardChecker = new BoardChecker();
            _inGamePresenter = GetComponent<InGamePresenter>();
            _judge = GetComponent<Judge>();
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

        public void Update()
        {
            if(_turnState == CellState.KANSAI)
            {
                Debug.Log($"_turnState: {_turnState}");
            //Debug.Log($"isKantoPlayer: {isKantoPlayer}");
            }
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

                if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
                {
                    Debugger.Log($"無効な座標: ({x}, {z})");
                    return;
                }

            if (_boardState[x, z] != CellState.NONE)
            {
                return;
            }

                _specifiedPosX = x;
                _specifiedPosZ = z;

                BoardChecker._PieceNum = 0;
                _judge.SetCanPlace(false);

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

        [PunRPC]
        private void SyncSetUpPiece(int x, int z, CellState turnState)
        {
            // ボード状態を更新
            _boardState[x, z] = turnState;
            Show(x, z, turnState);

            // ひっくり返すリストを取得
            var flipPositions = GetFlipPositions(x, z, turnState);
            //flipPositionsの中身を全て表示
            foreach (var pos in flipPositions)
            {
                Debug.Log($"Flip Position: {pos.Item1}, {pos.Item2}");
            }

            if(flipPositions.Count == 0)
            {
                Debug.Log("Flip positions are empty.");
                return;
            }

            // ひっくり返す処理を実行
            FlipPieces(flipPositions, turnState);
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

        private void Show(int x, int z)
        {
            if (_turnState == CellState.KANTO)
            {
                var piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                piece.SetActive(true);
            }
            else if (_turnState == CellState.KANSAI)
            {
                var piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                piece.SetActive(true);
            }
        }

        private void Show(int x, int z, CellState turnState)
        {
            if (turnState == CellState.KANTO)
            {
                var piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                piece.SetActive(true);
            }
            else if (turnState == CellState.KANSAI)
            {
                var piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                piece.SetActive(true);
            }
        }

        private void FlipPieces()
        {
            var flipPositions = _boardChecker.GetFlipPositions();

            foreach (var pos in flipPositions)
            {
                int x = pos.Item1;
                int z = pos.Item2;
                _boardState[x, z] = _turnState;

                if (_turnState == CellState.KANTO)
                {
                    var piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                    piece.SetActive(false);

                    piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                    piece.SetActive(true);
                }
                else if (_turnState == CellState.KANSAI)
                {
                    var piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                    piece.SetActive(false);

                    piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                    piece.SetActive(true);
                }
            }

            _boardChecker.ClearFlipPositions();
        }

        /// <summary>
        /// 指定されたリストの位置をひっくり返す
        /// </summary>
        /// <param name="flipPositions">ひっくり返す位置のリスト</param>
        /// <param name="turnState">現在のターン状態</param>
        private void FlipPieces(List<(int, int)> flipPositions, CellState turnState)
        {
            foreach (var pos in flipPositions)
            {
                int x = pos.Item1;
                int z = pos.Item2;
                _boardState[x, z] = turnState;

                if (turnState == CellState.KANTO)
                {
                    var piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                    piece.SetActive(false);

                    piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                    piece.SetActive(true);
                }
                else if (turnState == CellState.KANSAI)
                {
                    var piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                    piece.SetActive(false);

                    piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                    piece.SetActive(true);
                }
            }
        }

        [PunRPC]
        public void SyncTurnChange(CellState newTurnState)
        {
            Debug.Log($"SyncTurnChange called. New turn state: {newTurnState}");
            _turnState = newTurnState;
        }

        /// <summary>
        /// 指定されたマスに指定されたコマを表示する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        // public void Show(int x, int z)
        // {
        //     if (_turnState == CellState.KANTO)
        //     {
        //         var piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
        //         piece.SetActive(false);

        //         piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
        //         piece.SetActive(true);
        //     }
        //     else if (_turnState == CellState.KANSAI)
        //     {
        //         var piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
        //         piece.SetActive(false);

        //         piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
        //         piece.SetActive(true);
        //     }
        // }

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


        private bool TurnCheck()
        {
            _boardChecker.SetPlayerInfo(_specifiedPosX, _specifiedPosZ, _boardState, _turnState);
            return _boardChecker.TurnCheck(_specifiedPosX, _specifiedPosZ);
        }

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
    

        // IPunTurnManagerCallbacks の実装
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