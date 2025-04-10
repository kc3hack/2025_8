using UnityEngine;
using R3;

namespace refactor
{
    public class BoardManagerPresenter : MonoBehaviour
    {
        private BoardManagerModel _model;
        private BoardManagerView _view;
        private BoardManagerModel.CellState _turnState;

        public void Initialize()
        {
            _model = new BoardManagerModel();
            _view = GetComponent<BoardManagerView>();
            _view.Initialize();

            _turnState = BoardManagerModel.CellState.KANTO;// 初手関東
            Bind();
        }

        private void Bind()
        {
            _model.BoardState
                .Subscribe(state =>
                {
                    for (int x = 0; x < InGamePresenter.MAX_X; x++)
                    {
                        for (int z = 0; z < InGamePresenter.MAX_Z; z++)
                        {
                            if (state[x, z] != BoardManagerModel.CellState.NONE)
                            {
                                _view.Show(x, z, _turnState);
                            }
                        }
                    }
                })
                .AddTo(this);
        }
    }
}