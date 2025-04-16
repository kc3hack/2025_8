using UnityEngine;

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
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private GameObject _kantoPiece;// 関東の駒
        [SerializeField] private GameObject _kansaiPiece;// 関西の駒
        [SerializeField] private GameObject _kantoParent;// 関東の駒をまとめる親オブジェクト
        [SerializeField] private GameObject _kansaiParent;// 関西の駒をまとめる親オブジェクト
        [SerializeField] private GameObject _supportParent;// サポートオブジェクトをまとめる親オブジェクト
        private int _specifidPosX;// 現在のX座標
        private int _specifidPosZ;// 現在のZ座標
        private BoardChecker _boardChecker;
        public enum CellState// 盤上のマスの状態
        {
            NONE,
            KANTO,
            KANSAI,
        }
        private CellState[,] _boardState;// 盤面の状態を保持する2次元配列
        private CellState _turnState = CellState.KANTO; // 初手関東

        public void Initialize()
        {
            _boardState = new CellState[InGamePresenter.MAX_X, InGamePresenter.MAX_Z];

            _boardChecker = new BoardChecker();

            for (int x = 0; x < InGamePresenter.MAX_X; x++)
            {
                for (int z = 0; z < InGamePresenter.MAX_Z; z++)
                {
                    _boardState[x, z] = CellState.NONE;
                    var kantoPiece = Instantiate(_kantoPiece, _kantoParent.transform);
                    kantoPiece.transform.localPosition = new Vector3(x, 0.015f, z);
                    kantoPiece.SetActive(false);

                    var kansaiPiece = Instantiate(_kansaiPiece, _kansaiParent.transform);
                    kansaiPiece.transform.localPosition = new Vector3(x, 0.015f, z);
                    kansaiPiece.SetActive(false);
                }
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
                Debugger.Log("ゲームが終了しました。");
                return;
            }
            if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
            {
                Debugger.Log($"無効な座標: ({x}, {z})");
                return;
            }
            _specifidPosX = x;
            _specifidPosZ = z;
            _boardState[x, z] = _turnState;
            Show(x, z);
            UpdateSupport();
            TurnChange();
        }

        /// <summary>
        /// 盤面の状態を更新するメソッド
        /// 指定されたマスに指定されたコマを置く
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public void SetUpPiece(int x, int z)
        {
            if (_boardChecker.JudgeGame() == GameSceneStateEnum.GameSceneState.Start ||
                _boardChecker.JudgeGame() == GameSceneStateEnum.GameSceneState.Result)
            {
                Debugger.Log("ゲームが終了しました。");
                return;
            }
            {
                if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
                {
                    Debugger.Log($"無効な座標: ({x}, {z})");
                    return;
                }

                if (_boardState[x, z] != CellState.NONE)
                {
                    Debugger.Log($"この場所には既に駒が置かれています: ({x}, {z})");
                    return;
                }

                _specifidPosX = x;
                _specifidPosZ = z;
                if (TurnCheck())
                {
                    _boardState[x, z] = _turnState;
                    Show(x, z);
                    FlipPieces(); // 駒をひっくり返す処理
                    UpdateSupport();
                    TurnChange();
                }
                else
                    Debugger.Log($"ここには置けない: ({x}, {z})");
            }
        }

        /// <summary>
        /// ひっくり返す駒の位置を取得し、駒をひっくり返すメソッド
        /// ひっくり返す駒の位置はBoardCheckerクラスで取得する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public void FlipPieces()
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
            flipPositions.Clear();
        }

        /// <summary>
        /// 指定されたマスに指定されたコマを表示する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public void Show(int x, int z)
        {
            if (_turnState == CellState.KANTO)
            {
                var piece = _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                piece.SetActive(true);
                Debugger.Log($"関東の駒を表示: ({x}, {z})");
            }
            else if (_turnState == CellState.KANSAI)
            {
                var piece = _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject;
                piece.SetActive(true);
                Debugger.Log($"関西の駒を表示: ({x}, {z})");
            }
        }


        /// <summary>
        /// ターンを交代するメソッド
        /// </summary>
        public void TurnChange()
        {
            _turnState = _turnState == CellState.KANTO ? CellState.KANSAI : CellState.KANTO;
        }

        /// <summary>
        /// ターンのチェックを行うメソッド
        /// 盤面の状態と現在のターンを引数に渡して、ターンが可能かどうかを判定する
        /// ターンが可能な場合はtrueを返す
        /// ターンが不可能な場合はfalseを返す
        /// </summary>
        /// <returns></returns>
        private bool TurnCheck()
        {
            _boardChecker.SetPlayerInfo(_specifidPosX, _specifidPosZ, _boardState, _turnState);
            return _boardChecker.TurnCheck(_specifidPosX, _specifidPosZ);
        }

        private void UpdateSupport()
        {
            Debugger.Log($"_supportParent の子オブジェクト数: {_supportParent.transform.childCount}");

            for (int x = 0; x < InGamePresenter.MAX_X; x++)
            {
                for (int z = 0; z < InGamePresenter.MAX_Z; z++)
                {
                    int index = x * InGamePresenter.MAX_Z + z;
                    if (index >= _supportParent.transform.childCount)
                    {
                        Debugger.Log($"インデックス {index} が子オブジェクトの範囲外です");
                        continue;
                    }

                    var supportObj = _supportParent.transform.GetChild(index).gameObject;
                    _boardChecker.SetPlayerInfo(x, z, _boardState, _turnState);
                    if (_boardChecker.TurnCheck(x, z))
                    {
                        supportObj.SetActive(true);
                    }
                    else
                    {
                        supportObj.SetActive(false);
                    }
                }
            }
        }
    }
}