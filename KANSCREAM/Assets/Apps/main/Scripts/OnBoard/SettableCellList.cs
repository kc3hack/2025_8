namespace refactor
{
    /// <summary>
    /// 置くことができるマスのリストを管理するクラス
    /// BoardManagerで置けるかどうかを8方向探索する
    /// そのとき、置けるマスをtrueにして配列に格納しその情報を保存する
    /// このクラスは置ける場所がどこかをサポートするオブジェクトを表示する時に使う
    /// </summary>
    public class SettableCellList
    {
        private bool[,] _settableCellList;// 置くことができるマスのリスト
        public SettableCellList()
        {
            _settableCellList = new bool[InGamePresenter.MAX_X, InGamePresenter.MAX_Z];
            ResetSettableCell();
        }

        /// <summary>
        /// 指定したマスを置くことができるマスとして設定する
        /// 指定したマスが置けるマスである時にその座標を引数としてこのメソッドを呼び出す
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public void SetSettableCell(int x, int z)
        {
            if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
            {
                Debugger.Log($"無効な座標: ({x}, {z})");
                return;
            }
            _settableCellList[x, z] = true;
        }

        /// <summary>
        /// 全てのマスを置くことができないマスとして設定する
        /// 毎ターンこのメソッドを呼ぶことで初期化する
        /// </summary>
        public void ResetSettableCell()
        {
            for (int i = 0; i < InGamePresenter.MAX_X; i++)
            {
                for (int j = 0; j < InGamePresenter.MAX_Z; j++)
                {
                    _settableCellList[i, j] = false;
                }
            }
            _settableCellList[2, 2] = true;
            _settableCellList[2, 3] = true;
            _settableCellList[3, 2] = true;
            _settableCellList[3, 3] = true;

            _settableCellList[0, 5] = true;
            _settableCellList[5, 0] = true;
        }

        public bool[,] GetSettableCellList()
        {
            return _settableCellList;
        }
    }
}