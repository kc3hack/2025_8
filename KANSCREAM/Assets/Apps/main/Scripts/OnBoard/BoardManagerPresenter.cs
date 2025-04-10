using UnityEngine;
using R3;

namespace refactor
{
    public class BoardManagerPresenter : MonoBehaviour
    {
        private BoardManagerModel _boardManagerModel;
        private BoardManagerView _boardManagerView;

        public void Initialize()
        {
            _boardManagerModel = new BoardManagerModel();
            _boardManagerView = GetComponent<BoardManagerView>();
            _boardManagerView.Initialize();

            
        }
    }
}