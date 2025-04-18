using UnityEngine;

namespace refactor
{
    public class Judge : MonoBehaviour
    {
        private InGamePresenter _inGamePresenter;
        private BoardManager _boardManager;
        private BoardChecker _boardChecker;
        private BoardManager.CellState _turnState;
        private BoardManager.CellState[,] _boardState;
        private bool _canPlace;

        public void Initialize()
        {
            _inGamePresenter = GetComponent<InGamePresenter>();
            _boardManager = GetComponent<BoardManager>();
            _boardChecker = _boardManager.GetBoardChecker();
        }
        public void JudgeGame()
        {
            if(_canPlace) return;// 置けるマスがある場合は何もしない
            _turnState = _boardManager.GetTurnState();//ここで切り替えた後のターンを取得

            if(BoardChecker._PieceNum == 36)
            {
                Debugger.Log("ゲーム終了");
                return;
            }
            // 置けるマスがなく、自分のコマが存在する場合はターンを交代する
            _boardState = _boardManager.GetBoardState();
            foreach(BoardManager.CellState cellState in _boardState)
            {
                if(cellState == _turnState)
                {
                    Debugger.Log("ターン交代");
                    _boardManager.TurnChange();
                    _boardChecker.SetTurnState(_turnState == BoardManager.CellState.KANTO ? BoardManager.CellState.KANSAI : BoardManager.CellState.KANTO);
                    _inGamePresenter.SetSupportHundler();
                    return;
                }
            }
            
            Debugger.Log("ゲーム終了");
            return;
        }

        public void SetCanPlace(bool canPlace)
        {
            _canPlace = canPlace;
        }
    }
}