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
        private InGameModel _model;
        private BoardManager _boardManager;
        private GameObject[,] _supportHandlerList;
        private SoundManager _soundManager;
        private Judge _judge;

        void Start()
        {
            Initialize();
            //HighlightSettableCells();
        }
    

        private void Initialize()
        {
            _model = new InGameModel();
            _boardManager = GetComponent<BoardManager>();
            _boardManager.Initialize();
            _soundManager = GetComponent<SoundManager>();

            StartGame();
            Bind();

            _judge = GetComponent<Judge>();
            _judge.Initialize();
            _supportHandlerList = new GameObject[MAX_X, MAX_Z];
            _judge = GetComponent<Judge>();
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

            gameObject.GetComponent<SoundManager>().Initialize();
            SetSupportHundler();
        }

        /// <summary>
        /// サポートオブジェクトを生成するメソッド
        /// </summary>
        public void SetSupportHundler()
        {
            // 現在のプレイヤーのターン状態を取得
            var currentTurnState = _boardManager.GetTurnState();

            for (int x = 0; x < MAX_X; x++)
            {
                for (int z = 0; z < MAX_Z; z++)
                {
                    // _boardManager.isKantoPlayer に応じて関東または関西のターン状態を設定
                    var targetTurnState = _boardManager.isKantoPlayer ? BoardManager.CellState.KANTO : BoardManager.CellState.KANSAI;

                    // 指定したターン状態で置けるかどうかを判定
                    if (_boardManager.GetBoardChecker().TurnCheck(x, z, targetTurnState))
                    {
                        _supportHandlerList[x, z].gameObject.SetActive(true);
                        //Debugger.Log($"SetSupportHundler x:{x} z:{z} for {(_boardManager.isKantoPlayer ? "Kanto" : "Kansai")}");
                        _judge.SetCanPlace(true);
                    }
                    else
                    {
                        _supportHandlerList[x, z].gameObject.SetActive(false);
                    }
                }
            }

            // ひっくり返すリストをクリア
            _boardManager.GetBoardChecker().ClearFlipPositions();
        }

        /// <summary>
        /// ゲーム開始時にBGMを再生するメソッド
        /// </summary>
        public void StartGame()
        {
            _model.GameStateProp.Value = InGameModel.GameState.BeforeScream;
        }

        /// <summary>
        /// 盤面をリセットするメソッド
        /// 盤面の状態を初期化し、関東と関西の初期配置を行う
        /// </summary>
        public void Restart()
        {
            _model.GameStateProp.Value = InGameModel.GameState.Start;
            _boardManager.Reset();// ボードマネージャーのリセット
            _boardManager.GetBoardChecker().GetSettableCellList().Reset();// ボードチェッカーのリセット

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

        private void Bind()
        {
            _model.GameStateProp
                .Subscribe(state =>
                {
                    _soundManager.PlayBGM(state);
                })
                .AddTo(this);
        }
    }
}