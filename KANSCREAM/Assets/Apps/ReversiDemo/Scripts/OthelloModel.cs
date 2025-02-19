using System;
using System.Collections.Generic;
using System.Threading;
using R3;
using UnityEngine;
public class OthelloModel
{
    private bool _kantoCheckFlag = true;
    private bool _kansaiCheckFlag = true;
    private bool _turnCheck = false;

    private List<(int, int)> _canSettingSpriteList = new List<(int, int)>();
    private StateManager _stateManager;
    private SelectedSpriteModel _selectedSpriteModel;

    public OthelloModel()
    {
        _stateManager = new StateManager();
        _selectedSpriteModel = new SelectedSpriteModel();
        Initialize();
    }
    public void Initialize()
    {
        Awake();
        //初期配置  関東:黒  関西:白
        _stateManager.SetState(2, 2, StateManager.SpriteState.KANTO);
        _stateManager.SetState(3, 3, StateManager.SpriteState.KANTO);
        _stateManager.SetState(2, 3, StateManager.SpriteState.KANSAI);
        _stateManager.SetState(3, 2, StateManager.SpriteState.KANSAI);
    }

    /// <summary>
    /// 盤の初期化
    /// 全ての盤にNONEとKANSAIとKANTOを設定
    /// KANSAIとKANTOは非表示設定
    /// </summary>
    private void Awake()
    {
        //盤の初期化
        for (int y = 0; y < Const.BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < Const.BOARD_SIZE_X; x++)
            {
                _stateManager.Initialize(x, y);
            }
        }
        _stateManager.SetKANTO(2, 2);
        _stateManager.SetKANTO(3, 3);
        _stateManager.SetKANSAI(2, 3);
        _stateManager.SetKANSAI(3, 2);
    }

    public void UpdateSelectedFieldPosition()
    {
            _turnCheck = false;
            //全方向に対してコマを置けるかどうかの判定
            for (int i = 0; i < 8; i++)
            {
                if (TurnCheck(i))
                {
                    _turnCheck = true;
                }
            }

            if (_turnCheck && _stateManager.GetState(_selectedSpriteModel.SelectedSpritePosX.Value, _selectedSpriteModel.SelectedSpritePosY.Value) == _stateManager.GetSpriteState(0))
            {
                foreach (var info in _canSettingSpriteList)
                {
                    var posX = info.Item1;
                    var posY = info.Item2;
                    _stateManager.SetState(posX, posY, _stateManager.GetTurn());
                    if (_stateManager.GetTurn() == _stateManager.GetSpriteState(1))
                    {
                        _stateManager.SetKANTO(posX, posY);
                        _stateManager.SetKANSAINONE(posX, posY);
                    }
                    else if (_stateManager.GetTurn() == _stateManager.GetSpriteState(2))
                    {
                        _stateManager.SetKANSAI(posX, posY);
                        _stateManager.SetKANTONONE(posX, posY);
                    }
                }

                _stateManager.SetState(_selectedSpriteModel.SelectedSpritePosX.Value, _selectedSpriteModel.SelectedSpritePosY.Value, _stateManager.GetTurn());

                if (_stateManager.GetTurn() == _stateManager.GetSpriteState(1))
                {
                    _stateManager.SetKANTO(_selectedSpriteModel.SelectedSpritePosX.Value, _selectedSpriteModel.SelectedSpritePosY.Value);
                }
                else if (_stateManager.GetTurn() == _stateManager.GetSpriteState(2))
                {
                    _stateManager.SetKANSAI(_selectedSpriteModel.SelectedSpritePosX.Value, _selectedSpriteModel.SelectedSpritePosY.Value);
                }
                _stateManager.ChangeTurn();
                Thread.Sleep(100);
            CheckCanSettingStone();
            _canSettingSpriteList = new List<(int, int)>();//リストの初期化
        }
    }

    private void CalcTotalStoneNum()
    {
        var kantoStoneNum = 0;
        var kansaiStoneNum = 0;
        for (int y = 0; y < Const.BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < Const.BOARD_SIZE_X; x++)
            {
                if (_stateManager.GetState(x, y) == _stateManager.GetSpriteState(1))
                {
                    kantoStoneNum++;
                }
                else if (_stateManager.GetState(x, y) == _stateManager.GetSpriteState(2))
                {
                    kansaiStoneNum++;
                }
            }
        }

        // Debug.Log("関東の石の数: " + kantoStoneNum);
        // Debug.Log("関西の石の数: " + kansaiStoneNum);

        if (kantoStoneNum + kansaiStoneNum == Const.BOARD_SIZE_XY || (!_kantoCheckFlag && !_kansaiCheckFlag))
        {
            if (kantoStoneNum > kansaiStoneNum)
            {
                Debug.Log("関東の勝ち");
            }
            else if (kantoStoneNum < kansaiStoneNum)
            {
                Debug.Log("関西の勝ち");
            }
            else
            {
                Debug.Log("引き分け");
            }
        }
    }

    /// <summary>
    /// 置ける場所があるかどうかを判定
    /// </summary>
    private void CheckCanSettingStone()
    {
        for (int y = 0; y < Const.BOARD_SIZE_Y; y++)
        {
            for (int x = 0; x < Const.BOARD_SIZE_Y; x++)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (TurnCheck(i, x, y) && _stateManager.GetState(x, y) == _stateManager.GetSpriteState(0))
                    {
                        _canSettingSpriteList.Add((x, y));
                        break;
                    }
                    {
                        if (_stateManager.GetTurn() == _stateManager.GetSpriteState(1))
                        {
                            _kantoCheckFlag = true;
                            _turnCheck = true;
                            if (!_kansaiCheckFlag)
                            {
                                _kansaiCheckFlag = true;
                            }
                            break;
                        }
                        else if (_stateManager.GetTurn() == _stateManager.GetSpriteState(2))
                        {
                            _kansaiCheckFlag = true;
                            _turnCheck = true;
                            if (!_kantoCheckFlag)
                            {
                                _kantoCheckFlag = true;
                            }
                            break;
                        }
                    }
                }
            }
            //一箇所も置ける場所がない場合
            if (!_turnCheck)
            {
                if (_stateManager.GetTurn() == _stateManager.GetSpriteState(1))
                {
                    _kantoCheckFlag = false;
                }
                else if (_stateManager.GetTurn() == _stateManager.GetSpriteState(2))
                {
                    _kansaiCheckFlag = false;
                }
                _stateManager.ChangeTurn();
            }
            CalcTotalStoneNum();
        }
    }

    /// <summary>
    /// コマを置けるかどうかの判定
    /// </summary>
    /// <returns></returns>
    private bool TurnCheck(int direction)
    {
        //次回ここから
        var posX = _selectedSpriteModel.SelectedSpritePosX.Value;//選択中のフィールドの移動範囲調整
        var posY = _selectedSpriteModel.SelectedSpritePosY.Value;//選択中のフィールドの移動範囲調整

        return CheckCanSettingStone(direction, posX, posY);
    }

    /// <summary>
    /// コマを置けるかどうかの判定
    /// </summary>
    /// <returns></returns>
    private bool TurnCheck(int direction, int posX, int posY)
    {
        return CheckCanSettingStone(direction, posX, posY);
    }

    private bool CheckCanSettingStone(int direction, int posX, int posY)
    {
        var opponentPlayerTurn = _stateManager.GetTurn() == _stateManager.GetSpriteState(1) ? _stateManager.GetSpriteState(2) : _stateManager.GetSpriteState(1);

        var opponentInfoList = new List<(int, int)>();
        var local_turnCheck = false;
        // int i = 0;

        while (0 <= posX && posX <= Const.BOARD_SIZE_X && 0 <= posY && posY <= Const.BOARD_SIZE_Y)
        {
            // i++;
            switch (direction)
            {
                case 0://左
                    if (posX == 0) { return local_turnCheck; }
                    posX--;
                    // Debug.Log(i+"番目の左のコマ: " + _FieldState[posX, posY]);
                    break;
                case 1://右
                    if (posX == Const.BOARD_SIZE_X - 1) { return local_turnCheck; }
                    posX++;
                    // Debug.Log(i+"番目の右のコマ: " + _FieldState[posX, posY]);
                    break;
                case 2://下
                    if (posY == 0) { return local_turnCheck; }
                    posY--;
                    // Debug.Log(i+"番目の下のコマ: " + _FieldState[posX, posY]);
                    break;
                case 3://上
                    if (posY == Const.BOARD_SIZE_Y - 1) { return local_turnCheck; }
                    posY++;
                    // Debug.Log(i+"番目の上のコマ: " + _FieldState[posX, posY]);
                    break;
                case 4://右上
                    if (posX == Const.BOARD_SIZE_X - 1 || posY == Const.BOARD_SIZE_Y - 1) { return local_turnCheck; }
                    posX++;
                    posY++;
                    // Debug.Log(i+"番目の右上のコマ: " + _FieldState[posX, posY]);
                    break;
                case 5://左下
                    if (posX == 0 || posY == 0) { return local_turnCheck; }
                    posX--;
                    posY--;
                    // Debug.Log(i+"番目の左下のコマ: " + _FieldState[posX, posY]);
                    break;
                case 6://左上
                    if (posX == 0 || posY == Const.BOARD_SIZE_Y - 1) { return local_turnCheck; }
                    posX--;
                    posY++;
                    // Debug.Log(i+"番目の左上のコマ: " + _FieldState[posX, posY]);
                    break;
                case 7://右下
                    if (posX == Const.BOARD_SIZE_X - 1 || posY == 0) { return local_turnCheck; }
                    posX++;
                    posY--;
                    // Debug.Log(i+"番目の右下のコマ: " + _FieldState[posX, posY]);
                    break;
            }

            //指定した方向に相手のコマがあるときその情報をリストに追加
            if (_stateManager.GetState(posX, posY) == opponentPlayerTurn)
            {
                opponentInfoList.Add((posX, posY));
            }

            //1回目のループで左のコマが自分のコマまたは空の場合は終了
            if (opponentInfoList.Count == 0 && (_stateManager.GetState(posX, posY) == _stateManager.GetTurn() || _stateManager.GetState(posX, posY) == _stateManager.GetSpriteState(0)))
            {
                local_turnCheck = false;
                break;
            }

            //2つ以上隣のコマが自分のコマの場合は置ける
            if (opponentInfoList.Count > 0 && (_stateManager.GetState(posX, posY) == _stateManager.GetTurn()))
            {
                local_turnCheck = true;
                foreach (var info in opponentInfoList)
                {
                    _canSettingSpriteList.Add(info);
                }
                break;
            }
        }
        // Debug.Log("242:_turnCheck: " + local_turnCheck);
        return local_turnCheck;
    }
}