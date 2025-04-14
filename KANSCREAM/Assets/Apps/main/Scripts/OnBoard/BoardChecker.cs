namespace refactor
{
    public class BoardChecker
    {
        private int _specifidPosX;
        private int _specifidPosY;
        private BoardManager.CellState[,] _boardState;
        private BoardManager.CellState _turnState;
        public BoardChecker()
        {
            
        }

        public BoardManager.CellState JudgeGame()
        {
            return BoardManager.CellState.NONE;
        }

        public bool TurnCheck(int direction)
        {
            // var opponentPlayerTurn = _PlayerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;

            // var opponentInfoList = new List<(int, int)>();
            // var turnCheck = false;

            // int i = 0;

            // while (0 <= posX && posX <= FIELD_SIZE_X && 0 <= posY && posY <= FIELD_SIZE_Y)
            // {
            //     i++;
            //     switch (direction)
            //     {
            //         case 0://左
            //             if (posX == 0) { return localTurnCheck; }
            //             posX--;
            //             // Debug.Log(i+"番目の左のコマ: " + _FieldState[posX, posY]);
            //             break;
            //         case 1://右
            //             if (posX == FIELD_SIZE_X - 1) { return localTurnCheck; }
            //             posX++;
            //             // Debug.Log(i+"番目の右のコマ: " + _FieldState[posX, posY]);
            //             break;
            //         case 2://下
            //             if (posY == 0) { return localTurnCheck; }
            //             posY--;
            //             // Debug.Log(i+"番目の下のコマ: " + _FieldState[posX, posY]);
            //             break;
            //         case 3://上
            //             if (posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
            //             posY++;
            //             // Debug.Log(i+"番目の上のコマ: " + _FieldState[posX, posY]);
            //             break;
            //         case 4://右上
            //             if (posX == FIELD_SIZE_X - 1 || posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
            //             posX++;
            //             posY++;
            //             // Debug.Log(i+"番目の右上のコマ: " + _FieldState[posX, posY]);
            //             break;
            //         case 5://左下
            //             if (posX == 0 || posY == 0) { return localTurnCheck; }
            //             posX--;
            //             posY--;
            //             // Debug.Log(i+"番目の左下のコマ: " + _FieldState[posX, posY]);
            //             break;
            //         case 6://左上
            //             if (posX == 0 || posY == FIELD_SIZE_Y - 1) { return localTurnCheck; }
            //             posX--;
            //             posY++;
            //             // Debug.Log(i+"番目の左上のコマ: " + _FieldState[posX, posY]);
            //             break;
            //         case 7://右下
            //             if (posX == FIELD_SIZE_X - 1 || posY == 0) { return localTurnCheck; }
            //             posX++;
            //             posY--;
            //             // Debug.Log(i+"番目の右下のコマ: " + _FieldState[posX, posY]);
            //             break;
            //     }

            //     //指定した方向に相手のコマがあるときその情報をリストに追加
            //     if (_FieldState[posX, posY] == opponentPlayerTurn)
            //     {
            //         opponentInfoList.Add((posX, posY));
            //     }

            //     //1回目のループで左のコマが自分のコマまたは空の場合は終了
            //     if (opponentInfoList.Count == 0 && (_FieldState[posX, posY] == _PlayerTurn || _FieldState[posX, posY] == SpriteState.NONE))
            //     {
            //         localTurnCheck = false;
            //         break;
            //     }

            //     //2つ以上隣のコマが空白の場合メソッドを終了
            //     if (opponentInfoList.Count > 0 && (_FieldState[posX, posY] == SpriteState.NONE))
            //     {
            //         localTurnCheck = false;
            //         break;
            //     }

            //     //2つ以上隣のコマが自分のコマの場合は置ける
            //     if (opponentInfoList.Count > 0 && (_FieldState[posX, posY] == _PlayerTurn))
            //     {
            //         localTurnCheck = true;
            //         foreach (var info in opponentInfoList)
            //         {
            //             _InfoList.Add(info);
            //         }
            //         break;
            //     }
            // }
            // // Debug.Log("242:turnCheck: " + localTurnCheck);
            // return turnCheck;
            return true;
        }

        public void SetPlayerInfo(int x, int y, BoardManager.CellState[,] boardState, BoardManager.CellState playerTurn)
        {
            _specifidPosX = x;
            _specifidPosY = y;
            _boardState = boardState;
            _turnState = playerTurn;
        }
    }
}