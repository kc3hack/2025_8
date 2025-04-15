namespace refactor
{
    public class SettableCellList
    {
        private bool[,] _settableCellList;
        public SettableCellList()
        {
            _settableCellList = new bool[InGamePresenter.MAX_X, InGamePresenter.MAX_Z];

            ResetSettableCell();
        }

        public void SetSettableCell(int x, int z)
        {
            if (x < 0 || x >= InGamePresenter.MAX_X || z < 0 || z >= InGamePresenter.MAX_Z)
            {
                Debugger.Log($"無効な座標: ({x}, {z})");
                return;
            }
            _settableCellList[x, z] = true;
        }

        public void ResetSettableCell()
        {
            for (int i = 0; i < InGamePresenter.MAX_X; i++)
            {
                for (int j = 0; j < InGamePresenter.MAX_Z; j++)
                {
                    _settableCellList[i, j] = false;
                }
            }
        }

        public bool[,] GetSettableCellList()
        {
            return _settableCellList;
        }
    }
}