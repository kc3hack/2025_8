using System.Collections.Generic;
using UnityEngine;

namespace refactor
{
    /// <summary>
    /// 盤面の状態を管理するクラス
    /// 指定したマスにコマを置けるかどうかを判定する
    /// 全探索をして、置けるマスをtrueにして配列に格納
    /// </summary>
    public class BoardChecker
    {
        private int _specifiedPosX;// 指定したマスのX座標
        private int _specifiedPosZ;// 指定したマスのY座標
        private BoardManager.CellState[,] _boardState;// 盤面の状態を保持する2次元配列
        private BoardManager.CellState _turnState = BoardManager.CellState.KANTO;// 現在のターン
        private SettableCellList _settableCellList;
        private List<(int, int)> _flipPositions = new List<(int, int)>(); // ひっくり返されるコマのリスト
        public BoardChecker()
        {
            _settableCellList = new SettableCellList();
        }
        public static int _PieceNum = 0;

        /// <summary>
        /// ターン可能かどうかを判定するメソッド
        /// ターン可能な場合はtrueを返す
        /// ターン不可能な場合はfalseを返す
        /// 未完成
        /// </summary>
        /// <returns></returns>
        public bool TurnCheck(int posX, int posY)
        {
            if(_boardState[posX, posY] != BoardManager.CellState.NONE)
            {
                _PieceNum++;
                return false;
            }
            bool canPlace = false;

            for (int direction = 0; direction < 8; direction++)
            {
                if (TurnCheckSpecifidDirection(posX, posY, direction))
                {
                    canPlace = true;
                }
            }

            return canPlace;
        }


        /// <summary>
        /// 指定した方向にターン可能かどうかを判定するメソッド
        /// 未完成
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool TurnCheckSpecifidDirection(int posX, int posY, int direction)
        {
            int startX = posX;
            int startY = posY;

            var opponentPlayerTurn = _turnState == BoardManager.CellState.KANTO ? BoardManager.CellState.KANSAI : BoardManager.CellState.KANTO;
            var turnCheck = false;

            var FIELD_SIZE_X = InGamePresenter.MAX_X;
            var FIELD_SIZE_Y = InGamePresenter.MAX_Z;

            // 方向ごとの移動量を定義
            int[] dx = { -1, 1, 0, 0, 1, -1, -1, 1 }; // 左, 右, 下, 上, 右上, 左下, 左上, 右下
            int[] dy = { 0, 0, -1, 1, 1, -1, 1, -1 };

            // 指定方向に移動
            posX += dx[direction];
            posY += dy[direction];

            bool hasOpponent = false;
            var tempFlipPositions = new List<(int, int)>();

            while (posX >= 0 && posX < FIELD_SIZE_X && posY >= 0 && posY < FIELD_SIZE_Y)
            {
                if (_boardState[posX, posY] == opponentPlayerTurn)
                {
                    hasOpponent = true;
                    tempFlipPositions.Add((posX, posY)); // 挟まれた駒を一時リストに追加
                }
                else if (_boardState[posX, posY] == _turnState)
                {
                    // 自分の駒があり、間に相手の駒がある場合は置ける
                    if (hasOpponent)
                    {
                        turnCheck = true;
                        _flipPositions.AddRange(tempFlipPositions); // 一時リストをひっくり返すリストに追加
                        _settableCellList.SetSettableCell(startX, startY); // 置ける場所を記録
                    }
                    break;
                }
                else
                {
                    // 空白の場合は終了
                    break;
                }

                // 次のマスに移動
                posX += dx[direction];
                posY += dy[direction];
            }

            return turnCheck;
        }

        /// <summary>
        /// 相手のコマが置かれた位置を基にひっくり返すリストを取得する
        /// </summary>
        /// <param name="x">相手が置いた位置のX座標</param>
        /// <param name="z">相手が置いた位置のZ座標</param>
        /// <param name="turnState">相手のターン状態</param>
        /// <returns>ひっくり返す位置のリスト</returns>
         public List<(int, int)> GetFlipPositionsForOpponentMove(int posX, int posY, BoardManager.CellState turnState)
        {
            var flipPositions = new List<(int, int)>();
            var opponentPlayerTurn = turnState == BoardManager.CellState.KANTO ? BoardManager.CellState.KANSAI : BoardManager.CellState.KANTO;

            var FIELD_SIZE_X = InGamePresenter.MAX_X;
            var FIELD_SIZE_Y = InGamePresenter.MAX_Z;

            // 方向ごとの移動量を定義
            int[] dx = { -1, 1, 0, 0, 1, -1, -1, 1 }; // 左, 右, 下, 上, 右上, 左下, 左上, 右下
            int[] dy = { 0, 0, -1, 1, 1, -1, 1, -1 };

            for (int direction = 0; direction < 8; direction++)
            {
                int currentX = posX + dx[direction];
                int currentY = posY + dy[direction];

                bool hasOpponent = false;
                var tempFlipPositions = new List<(int, int)>();

                while (currentX >= 0 && currentX < FIELD_SIZE_X && currentY >= 0 && currentY < FIELD_SIZE_Y)
                {
                    if (_boardState[currentX, currentY] == opponentPlayerTurn)
                    {
                        hasOpponent = true;
                        tempFlipPositions.Add((currentX, currentY)); // 挟まれた駒を一時リストに追加
                    }
                    else if (_boardState[currentX, currentY] == turnState)
                    {
                        // 自分の駒があり、間に相手の駒がある場合はひっくり返す
                        if (hasOpponent)
                        {
                            flipPositions.AddRange(tempFlipPositions);
                        }
                        break;
                    }
                    else
                    {
                        // 空白の場合は終了
                        break;
                    }

                    // 次のマスに移動
                    currentX += dx[direction];
                    currentY += dy[direction];
                }
            }

            return flipPositions;
        }

        /// <summary>
        /// ターン可能かどうかを判定するメソッド
        /// ターン可能な場合はtrueを返す
        /// ターン不可能な場合はfalseを返す
        /// </summary>
        public bool TurnCheck(int posX, int posY, BoardManager.CellState turnState)
        {
            if (_boardState[posX, posY] != BoardManager.CellState.NONE)
            {
                _PieceNum++;
                return false;
            }

            bool canPlace = false;
            _turnState = turnState; // ターン状態を設定

            for (int direction = 0; direction < 8; direction++)
            {
                if (TurnCheckSpecifidDirection(posX, posY, direction))
                {
                    canPlace = true;
                }
            }

            return canPlace;
        }

        /// <summary>
        /// 置きたいマスを取得するメソッド
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="boardState"></param>
        /// <param name="playerTurn"></param>
        public void SetPlayerInfo(int x, int z, BoardManager.CellState[,] boardState, BoardManager.CellState playerTurn)
        {
            _specifiedPosX = x;
            _specifiedPosZ = z;
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

        public List<(int, int)> GetFlipPositions()
        {
            return _flipPositions;
        }

        public void ClearFlipPositions()
        {
            //　_flipPositionsの中身を全部消去
            _flipPositions.Clear();
        }

        public SettableCellList GetSettableCellList()
        {
            return _settableCellList;
        }
    }
}