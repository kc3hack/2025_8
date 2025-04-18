using UnityEngine;
using R3;

namespace refactor
{
    public class InGamePresenter : MonoBehaviour
    {
        public const int MAX_X = 6;
        public const int MAX_Z = 6;
        [SerializeField] private GameObject _supportObj;
        [SerializeField] private GameObject _supportParentObj;
        private BoardManager _boardManager;
        private GameObject[,] _supportHandlerList;

        void Start()
        {
            Initialize();
        }
    

        private void Initialize()
        {
            _boardManager = GetComponent<BoardManager>();
            _boardManager.Initialize();
            _supportHandlerList = new GameObject[MAX_X, MAX_Z];
            var i = 0;

            for (int x = 0; x < MAX_X; x++)
            {
                for (int z = 0; z < MAX_Z; z++)
                {
                    Debugger.Log($"i = {++i}");
                    var supportObj = Instantiate(_supportObj, _supportParentObj.transform);
                    supportObj.transform.localPosition = new Vector3(x, 0.015f, z);
                    var supportHandler = supportObj.GetComponentInChildren<SupportHandler>();
                    supportHandler.Initialize(x, z);
                    _supportHandlerList[x, z] = supportObj;
                    supportObj.SetActive(false);
                }
            }
            
            // 関東の初期配置
            _boardManager.InitializeSetUpPiece(2, 3);
            _boardManager.InitializeSetUpPiece(2, 2);
            _boardManager.InitializeSetUpPiece(3, 2);
            _boardManager.InitializeSetUpPiece(3, 3);
            _boardManager.InitializeSetUpPiece(0, 5);
            _boardManager.TurnChange();// 次のターンを関東に変更
            _boardManager.InitializeSetUpPiece(5, 0);
            _boardManager.TurnChange();// 初期のターンを関東に変更

            _boardManager.InitializeSetCellState();
            SetSupportHundler();
        }

        public void SetSupportHundler()
        {
            for(int x = 0; x < MAX_X; x++)
            {
                for (int z = 0; z < MAX_Z; z++)
                {
                    if(_boardManager.GetBoardChecker().TurnCheck(x,z)){
                        _supportHandlerList[x, z].gameObject.SetActive(true);
                        Debugger.Log($"SetSupportHundler x:{x} z:{z}");
                    }
                    else
                    {
                        _supportHandlerList[x, z].gameObject.SetActive(false);
                    }
                }
            }
            _boardManager.GetBoardChecker().ClearFlipPositions();
        }

        public void Restart()
        {
            _boardManager.Reset();
            _boardManager.GetBoardChecker().GetSettableCellList().Reset();

            // 関東の初期配置
            _boardManager.InitializeSetUpPiece(2, 3);
            _boardManager.InitializeSetUpPiece(2, 2);
            _boardManager.InitializeSetUpPiece(3, 2);
            _boardManager.InitializeSetUpPiece(3, 3);
            _boardManager.InitializeSetUpPiece(0, 5);
            _boardManager.TurnChange();// 次のターンを関東に変更
            _boardManager.InitializeSetUpPiece(5, 0);
            _boardManager.TurnChange();// 初期のターンを関東に変更
            _boardManager.InitializeSetCellState();
            SetSupportHundler();
        }
    }
}