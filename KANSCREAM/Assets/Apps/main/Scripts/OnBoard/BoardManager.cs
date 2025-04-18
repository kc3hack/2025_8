using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections.Generic;

namespace refactor
{
    public class BoardManager : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
    {
        private const int MAX_X = 6;
        private const int MAX_Z = 6;

        [SerializeField] private GameObject _supportObj;
        [SerializeField] private GameObject _supportParentObj;
        [SerializeField] private GameObject _kantoPiece;
        [SerializeField] private GameObject _kansaiPiece;
        [SerializeField] private GameObject _kantoParent;
        [SerializeField] private GameObject _kansaiParent;

        private int _specifidPosX;
        private int _specifidPosZ;
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
        private bool isKantoPlayer;

        public void Initialize()
        {
            _boardState = new CellState[InGamePresenter.MAX_X, InGamePresenter.MAX_Z];
            _boardChecker = new BoardChecker();

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

        /// <summary>
        /// 盤面の初期配置を行うメソッド
        /// 関東と関西の初期配置を行う
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public void InitializeSetUpPiece(int x, int z)
        {
            if (_boardChecker.JudgeGame() == GameSceneStateEnum.GameSceneState.Start || 
                _boardChecker.JudgeGame() == GameSceneStateEnum.GameSceneState.Result)
            {
                //Debugger.Log("ゲームが終了しました。");
                return;
            }
            {
                if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
                {
                    //Debugger.Log($"無効な座標: ({x}, {z})");
                    return;
                }
                _specifidPosX = x;
                _specifidPosZ = z;
                _boardState[x, z] = _turnState;
                Show(x, z);
                TurnChange();
            }
        }

        
        public void SetUpPiece(int x, int z)
        {

            if (_boardChecker.JudgeGame() == GameSceneStateEnum.GameSceneState.Start || 
                _boardChecker.JudgeGame() == GameSceneStateEnum.GameSceneState.Result)
            {
                //Debugger.Log("ゲームが終了しました。");
                return;
            }

            Debug.Log(isKantoPlayer + ":" + _turnState);
            // 自分のターンでない場合は入力を無効化
            if ((isKantoPlayer && _turnState != CellState.KANTO) || (!isKantoPlayer && _turnState != CellState.KANSAI))
            {
                //Debug.Log("自分のターンではありません。");
                return; // 入力を無視
            }

            int index = x * InGamePresenter.MAX_Z + z;
            if (index < 0 || index >= _kantoParent.transform.childCount)
            {
                return;
            }

            if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
            {
                return;
            }

            if (_boardState[x, z] != CellState.NONE)
            {
                return;
            }

            _specifidPosX = x;
            _specifidPosZ = z;

            if (TurnCheck())
            {
                _boardState[x, z] = _turnState;
                Show(x, z);
                FlipPieces();
                

                // 他のクライアントに同期
                if (photonView != null)
                {
                    Debug.Log($"Syncing piece placement: ({x}, {z}) with turn state: {_turnState}");
                    photonView.RPC("SyncSetUpPiece", RpcTarget.Others, x, z, _turnState);
                }
                TurnChange();
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
            _turnState = newTurnState;
        }

        public void TurnChange()
        {
            // ターンを変更
            _turnState = _turnState == CellState.KANTO ? CellState.KANSAI : CellState.KANTO;

            // 全クライアントに同期
            if (photonView != null)
            {
                photonView.RPC("SyncTurnChange", RpcTarget.All, _turnState);
            }
            else
            {
                //Debug.LogError("photonView is null. Ensure PhotonView is attached to the GameObject.");
            }
        }

        private bool TurnCheck()
        {
            _boardChecker.SetPlayerInfo(_specifidPosX, _specifidPosZ, _boardState, _turnState);
            return _boardChecker.TurnCheck(_specifidPosX, _specifidPosZ);
        }

        // IPunTurnManagerCallbacks の実装
        public void OnTurnBegins(int turn)
        {
            //Debug.Log($"Turn {turn} begins.");
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