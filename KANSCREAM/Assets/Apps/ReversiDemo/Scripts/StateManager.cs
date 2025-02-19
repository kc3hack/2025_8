using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// オセロの状態を管理するクラス
/// </summary>
public class StateManager : MonoBehaviour
{
    public enum SpriteState
    {
        NONE,
        KANTO,
        KANSAI
    }

    private SpriteState[,] _spriteState = new SpriteState[Const.BOARD_SIZE_X, Const.BOARD_SIZE_Y];
    private KantoState[,] _kantoState = new KantoState[Const.BOARD_SIZE_X, Const.BOARD_SIZE_Y];
    private KansaiState[,] _kansaiState = new KansaiState[Const.BOARD_SIZE_X, Const.BOARD_SIZE_Y];
    private SpriteState _platerTurn = SpriteState.KANTO;

    public void Initialize(int x, int y)
    {
        _spriteState[x, y] = SpriteState.NONE;
        _kantoState[x, y].SetState(SpriteState.NONE);
        _kansaiState[x, y].SetState(SpriteState.NONE);
    }


    /// <summary>
    /// オセロの状態を設定する
    /// </summary>
    /// <param name="x">置きたいマスのx座標</param>
    /// <param name="y">置きたいマスのy座標</param>
    /// <param name="state">置きたいコマ
    /// </param>
    public void SetNONE(int x, int y)
    {
        _spriteState[x, y] = SpriteState.NONE;
    }

    public void SetKANTO(int x, int y)
    {
        _spriteState[x, y] = SpriteState.KANTO;
        _kantoState[x, y].SetState(SpriteState.KANTO);
    }

    public void SetKANSAI(int x, int y)
    {
        _spriteState[x, y] = SpriteState.KANSAI;
        _kansaiState[x, y].SetState(SpriteState.KANSAI);
    }

    public void SetKANTONONE(int x, int y)
    {
        _spriteState[x, y] = SpriteState.NONE;
        _kantoState[x, y].SetState(SpriteState.NONE);
    }

    public void SetKANSAINONE(int x, int y)
    {
        _spriteState[x, y] = SpriteState.NONE;
        _kansaiState[x, y].SetState(SpriteState.NONE);
    }

    public void SetState(int x, int y, SpriteState state)
    {
        _spriteState[x, y] = state;
    }

    public SpriteState GetState(int x, int y)
    {
        return _spriteState[x, y];
    }

    public void ChangeTurn()
    {
        _platerTurn = _platerTurn == SpriteState.KANTO ? SpriteState.KANSAI : SpriteState.KANTO;
    }

    public SpriteState GetTurn()
    {
        return _platerTurn;
    }

    public SpriteState GetSpriteState(int num)
    {
        switch (num)
        {
            case 0:
                return SpriteState.NONE;
            case 1:
                return SpriteState.KANTO;
            case 2:
                return SpriteState.KANSAI;
            default:
                return SpriteState.NONE;
        }
    }

    public void Initialize()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                _spriteState[i, j] = SpriteState.NONE;
            }
        }
        _spriteState[2, 2] = SpriteState.KANTO;
        _spriteState[3, 3] = SpriteState.KANTO;
       _spriteState[2, 3] = SpriteState.KANSAI;
        _spriteState[3, 2] = SpriteState.KANSAI;
    }
}