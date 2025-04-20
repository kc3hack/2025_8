using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace refactor
{
    public class KanScream : MonoBehaviourPun
    {
        private BoardManager _boardManager;

        private bool isKanScreamRPCCompleted = false;
        private bool isPushButton = false;

        public void Awake()
        {
            // BoardManager のインスタンスを取得
            _boardManager = FindObjectOfType<BoardManager>();
            if (_boardManager == null)
            {
                Debug.LogError("BoardManager が見つかりません。");
            }
        }

       public void kanScream(float similarity)
       {
            if (isPushButton) return;
            isPushButton = true;

            // 現在のboardStateを取得
            var boardState = _boardManager.GetBoardState();
            List<(int, int)> kantoPicesPositions = new List<(int, int)>();
            // boardStateの中から、関東コマの座標を取得し、その二次元配列を作成
            for (int x = 0; x < InGamePresenter.MAX_X; x++)
            {
                for (int z = 0; z < InGamePresenter.MAX_Z; z++)
                {
                    if (boardState[x, z] == BoardManager.CellState.KANTO)
                    {
                        kantoPicesPositions.Add((x, z));
                    }
                }
            }

            int kantoFlipNum = 0;
            //90点以上 5枚、90点未満 4枚、70点未満 3枚、50点未満 2枚、30点未満 1枚、10点未満 0枚をランダムに選択
            if (similarity >= 90)
            {
                kantoFlipNum = 5;
            }
            else if (similarity >= 70)
            {
                kantoFlipNum = 4;
            }
            else if (similarity >= 50)
            {
                kantoFlipNum = 3;
            }
            else if (similarity >= 30)
            {
                kantoFlipNum = 2;
            }
            else if (similarity >= 10)
            {
                kantoFlipNum = 1;
            }
            else
            {
                kantoFlipNum = 0;
            }


            // kantoFlipNumの数だけランダムに関東コマを選択し、flipする
            for (int i = 0; i < kantoFlipNum; i++)
            {
                // ランダムに関東コマを選択
                int randomIndex = Random.Range(0, kantoPicesPositions.Count);
                int x = kantoPicesPositions[randomIndex].Item1;
                int z = kantoPicesPositions[randomIndex].Item2;

                // 関東コマをflipする
                _boardManager.KanScream(x, z);

                // 選択した関東コマを削除
                kantoPicesPositions.RemoveAt(randomIndex);
                //5秒待機


            }

       }

       public void Reset()
       {
            isKanScreamRPCCompleted = false;
            isPushButton = false;
       }
    }
}