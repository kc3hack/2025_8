using R3;
using System.Collections;

namespace refactor
{
    public class BoardManagerModel
    {
        public enum CellState
        {
            NONE,
            KANTO,
            KANSAI,
        }

        private ReactiveProperty<CellState[,]> _boardState = new ReactiveProperty<CellState[,]>();
        public ReadOnlyReactiveProperty<CellState[,]> BoardState => _boardState.ToReadOnlyReactiveProperty();

        public void Initialize()
        {
            var initialState = new CellState[InGamePresenter.MAX_X, InGamePresenter.MAX_Z];

            for (int x = 0; x < InGamePresenter.MAX_X; x++)
            {
                for (int z = 0; z < InGamePresenter.MAX_Z; z++)
                {
                    initialState[x, z] = CellState.NONE;
                }
            }

            _boardState.Value = initialState;
        }

        public void SetUpPiece(int x, int z, CellState state)
        {
            if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
            {
                Debugger.Log($"存在しないマス: ({x}, {z})");
                return;
            }

            var currentState = _boardState.Value;
            currentState[x, z] = state;
            _boardState.Value = currentState; // 値を更新して通知
        }

        public void CapturePiece(int x, int z)
        {
            if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
            {
                Debugger.Log($"存在しないマス: ({x}, {z})");
                return;
            }

            var currentState = _boardState.Value;
            currentState[x, z] = CellState.NONE;
            _boardState.Value = currentState; // 値を更新して通知
        }
    }
}