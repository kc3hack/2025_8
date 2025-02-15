using UnityEngine;
public class  OthelloSystem : MonoBehaviour
{
    public GameObject OthelloSprite;//オセロ駒オブジェクト
    const int FIELD_SIZE_X = 6;
    const int FIELD_SIZE_Y = 6;

    public enum SpriteState
    {
        NONE,
        KANTO,
        KANSAI,
    }

    private SpriteState[,] _FieldState = new SpriteState[FIELD_SIZE_X, FIELD_SIZE_Y];
    private SpriteScript[,] _FieldSpriteScript = new SpriteScript[FIELD_SIZE_X, FIELD_SIZE_Y];

    void Start()
    {
        for(int y = 0; y < FIELD_SIZE_Y; y++)
        {
            for(int x = 0; x < FIELD_SIZE_X; x++)
            {
                var sprite = Instantiate(OthelloSprite
                                        , new Vector3(-2.5f+x, 0.02f, -2.5f+y)
                                        , Quaternion.Euler(0, 0, 0));
                _FieldState[x, y] = SpriteState.NONE;
                _FieldSpriteScript[x, y] = sprite.GetComponent<SpriteScript>();
            }
        }
        _FieldSpriteScript[0, 0].SetState(SpriteState.KANSAI);
        _FieldSpriteScript[3, 4].SetState(SpriteState.KANTO);
        _FieldSpriteScript[4, 3].SetState(SpriteState.KANTO);
        _FieldSpriteScript[4, 4].SetState(SpriteState.KANSAI);
    }

    void Update()
    {

    }
}