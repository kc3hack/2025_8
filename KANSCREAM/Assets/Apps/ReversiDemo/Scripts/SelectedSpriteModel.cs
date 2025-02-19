using UnityEngine;
using R3;
public class SelectedSpriteModel
{
    private readonly ReactiveProperty<int> _selectedSpritePosX;//指定しているマスのX座標
    public ReadOnlyReactiveProperty<int> SelectedSpritePosX => _selectedSpritePosX;
    private readonly ReactiveProperty<int> _selectedSpritePosY;//指定しているマスのY座標
    public ReadOnlyReactiveProperty<int> SelectedSpritePosY => _selectedSpritePosY;
    
    public SelectedSpriteModel()
    {
        //選択中のフィールドを示すオブジェクトの初期位置を設定
        _selectedSpritePosX = new ReactiveProperty<int>(0);
        _selectedSpritePosY = new ReactiveProperty<int>(0);
    }

    /// <summary>
    /// 選択中のフィールドを移動
    /// </summary>
    /// <param name="direction">0:上,1:下,2:左,3:右,</param>
    public void Move(int direction)
    {
        switch (direction)
        {
            case 0:
                MoveUp();
                break;
            case 1:
                MoveDown();
                break;
            case 2:
                MoveLeft();
                break;
            case 3:
                MoveRight();
                break;
        }
    }

    public void MoveUp()
    {
        if (_selectedSpritePosY.Value < Const.BOARD_SIZE_Y)
        {
            this._selectedSpritePosY.Value++;
        }
    }

    public void MoveDown()
    {
        if (0 < _selectedSpritePosY.Value)
        {
            this._selectedSpritePosY.Value--;
        }
    }

    public void MoveLeft()
    {
        if (0 < _selectedSpritePosX.Value)
        {
            this._selectedSpritePosX.Value--;
        }
    }

    public void MoveRight()
    {
        if (_selectedSpritePosX.Value < Const.BOARD_SIZE_X)
        {
            this._selectedSpritePosX.Value++;
        }
    }
}