using UnityEngine;

namespace refactor
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private GameObject _kantoPiece;
        [SerializeField] private GameObject _kansaiPiece;
        [SerializeField] private GameObject _kantoParent;
        [SerializeField] private GameObject _kansaiParent;
        private int _currentPosX;
        private int _currentPosZ;
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
                _currentPosX = x;
                _currentPosZ = z;
                _boardState[x, z] = _turnState;
                if(TurnCheck())
                    Show(x, z);
                else
                    Debugger.Log($"ここには置けない: ({x}, {z})");
                TurnChange();
            }
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
            _boardChecker.SetPlayerInfo(_currentPosX, _currentPosZ, _boardState, _turnState);
            return _boardChecker.TurnCheck(_currentPosX, _currentPosZ);
        }
    }
}