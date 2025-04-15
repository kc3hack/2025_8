using System.Collections.Generic;

namespace refactor
{
    public class BoardChecker
    {
        private int _specifidPosX;
        private int _specifidPosY;
        private BoardManager.CellState[,] _boardState;
        private BoardManager.CellState _turnState;
        private SettableCellList _settableCellList;
        public BoardChecker()
        {
            _settableCellList = new SettableCellList();
        }

        /// <summary>
        /// ゲームの勝敗を判定するメソッド
        /// 現状コマのステートで判定しているけど
        /// ゲーム画面ステートで判定する方がいいかも
        /// </summary>
        /// <returns></returns>
        public GameSceneStateEnum.GameSceneState JudgeGame()
        {
            return GameSceneStateEnum.GameSceneState.BeforeScream;// デバッグ用
        }

        /// <summary>
        /// ターン可能かどうかを判定するメソッド
        /// ターン可能な場合はtrueを返す
        /// ターン不可能な場合はfalseを返す
        /// </summary>
        /// <returns></returns>
        public bool TurnCheck(int posX, int posY)
        {
            // for (int i = 0; i < 8; i++)
            // {
            //     if (TurnCheckSpecifidDirection(i))
            //     {
            //         return true;
            //     }
            // }

            if (TurnCheckSpecifidDirection(posX, posY, 0))
            {
                return true;
            }

            return true;
        }

        /// <summary>
        /// 指定した方向にターン可能かどうかを判定するメソッド
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool TurnCheckSpecifidDirection(int posX, int posY, int direction)
        {
            _specifidPosX = posX;
            _specifidPosY = posY;
            var opponentPlayerTurn = _turnState == BoardManager.CellState.KANTO ? BoardManager.CellState.KANSAI : BoardManager.CellState.KANTO;

            var opponentInfoList = new List<(int, int)>();
            var turnCheck = false;

            int i = 0;

            while (0 <= posX && posX <= InGamePresenter.MAX_X && 0 <= posY && posY <= InGamePresenter.MAX_Z)
            {
                i++;
                switch (direction)
                {
                    case 0://左
                        if (posX == 0) { return turnCheck; }
                        posX--;
                        // Debug.Log(i+"番目の左のコマ: " + _boardState[posX, posY]);
                        break;
                        //         case 1://右
                        //             if (posX == FIELD_SIZE_X - 1) { return turnCheck; }
                        //             posX++;
                        //             // Debug.Log(i+"番目の右のコマ: " + _boardState[posX, posY]);
                        //             break;
                        //         case 2://下
                        //             if (posY == 0) { return turnCheck; }
                        //             posY--;
                        //             // Debug.Log(i+"番目の下のコマ: " + _boardState[posX, posY]);
                        //             break;
                        //         case 3://上
                        //             if (posY == FIELD_SIZE_Y - 1) { return turnCheck; }
                        //             posY++;
                        //             // Debug.Log(i+"番目の上のコマ: " + _boardState[posX, posY]);
                        //             break;
                        //         case 4://右上
                        //             if (posX == FIELD_SIZE_X - 1 || posY == FIELD_SIZE_Y - 1) { return turnCheck; }
                        //             posX++;
                        //             posY++;
                        //             // Debug.Log(i+"番目の右上のコマ: " + _boardState[posX, posY]);
                        //             break;
                        //         case 5://左下
                        //             if (posX == 0 || posY == 0) { return turnCheck; }
                        //             posX--;
                        //             posY--;
                        //             // Debug.Log(i+"番目の左下のコマ: " + _boardState[posX, posY]);
                        //             break;
                        //         case 6://左上
                        //             if (posX == 0 || posY == FIELD_SIZE_Y - 1) { return turnCheck; }
                        //             posX--;
                        //             posY++;
                        //             // Debug.Log(i+"番目の左上のコマ: " + _boardState[posX, posY]);
                        //             break;
                        //         case 7://右下
                        //             if (posX == FIELD_SIZE_X - 1 || posY == 0) { return turnCheck; }
                        //             posX++;
                        //             posY--;
                        //             // Debug.Log(i+"番目の右下のコマ: " + _boardState[posX, posY]);
                        //             break;
                }

                //指定した方向に相手のコマがあるときその情報をリストに追加
                if (_boardState[posX, posY] == opponentPlayerTurn)
                {
                    opponentInfoList.Add((posX, posY));
                }

                //1回目のループで左のコマが自分のコマまたは空の場合は終了
                if (opponentInfoList.Count == 0 && (_boardState[posX, posY] == _turnState || _boardState[posX, posY] == BoardManager.CellState.NONE))
                {
                    turnCheck = false;
                    break;
                }

                //2つ以上隣のコマが空白の場合メソッドを終了
                if (opponentInfoList.Count > 0 && (_boardState[posX, posY] == BoardManager.CellState.NONE))
                {
                    turnCheck = false;
                    break;
                }

                //2つ以上隣のコマが自分のコマの場合は置ける
                if (opponentInfoList.Count > 0 && (_boardState[posX, posY] == _turnState))
                {
                    turnCheck = true;
                    foreach (var info in opponentInfoList)
                    {
                        _settableCellList.SetSettableCell(_specifidPosX, _specifidPosY);
                    }
                    break;
                }
            }
            return turnCheck;
            // return true;
        }

        /// <summary>
        /// ターン可能なマスを取得するメソッド
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="boardState"></param>
        /// <param name="playerTurn"></param>
        public void SetPlayerInfo(int x, int y, BoardManager.CellState[,] boardState, BoardManager.CellState playerTurn)
        {
            _specifidPosX = x;
            _specifidPosY = y;
            _boardState = boardState;
            _turnState = playerTurn;
        }

        public void SetCellState(BoardManager.CellState[,] boardState)
        {
            _boardState = boardState;
        }

        public void SetTurnState(BoardManager.CellState turnState)
        {
            _turnState = turnState;
        }
        
    }
}