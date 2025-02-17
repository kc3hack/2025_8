using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;

public class OthelloSystem : MonoBehaviourPun
{
    public GameObject KantoStone;
    public GameObject KansaiStone;
    public GameObject SelectedFieldCube;
    const int FIELD_SIZE_X = 6;
    const int FIELD_SIZE_Y = 6;
    private int SelectedFieldCubePosX;
    private int SelectedFieldCubePosY;
    private SpriteState _PlayerTurn = SpriteState.KANTO;
    public enum SpriteState
    {
        NONE,
        KANTO,
        KANSAI,
    }

    private SpriteState[,] _FieldState = new SpriteState[FIELD_SIZE_X, FIELD_SIZE_Y];
    private KantoStoneObj[,] _KantoStoneObj = new KantoStoneObj[FIELD_SIZE_X, FIELD_SIZE_Y];
    private KansaiStoneObj[,] _KansaiStoneObj = new KansaiStoneObj[FIELD_SIZE_X, FIELD_SIZE_Y];

    private PunTurnManager turnManager;
    private Color originalColor; // 元の色を保存する変数
    private bool isKantoPlayer;

    void Start()
    {
        SelectedFieldCubePosX = (int)SelectedFieldCube.transform.position.x;
        SelectedFieldCubePosY = (int)SelectedFieldCube.transform.position.z;

        for (int y = 0; y < FIELD_SIZE_Y; y++)
        {
            for (int x = 0; x < FIELD_SIZE_X; x++)
            {
                var kanto = Instantiate(KantoStone, new Vector3(-2.5f + x, 0.02f, -2.5f + y), Quaternion.Euler(0, 0, 0));
                var kansai = Instantiate(KansaiStone, new Vector3(-2.5f + x, 0.02f, -2.5f + y), Quaternion.Euler(0, 0, 0));
                _FieldState[x, y] = SpriteState.NONE;
                _KantoStoneObj[x, y] = kanto.GetComponent<KantoStoneObj>();
                _KansaiStoneObj[x, y] = kansai.GetComponent<KansaiStoneObj>();
                _KantoStoneObj[x, y].SetState(SpriteState.NONE);
                _KansaiStoneObj[x, y].SetState(SpriteState.NONE);
            }
        }
        _KantoStoneObj[2, 2].SetState(SpriteState.KANTO);
        _KansaiStoneObj[3, 2].SetState(SpriteState.KANSAI);
        _KantoStoneObj[3, 3].SetState(SpriteState.KANTO);
        _KansaiStoneObj[2, 3].SetState(SpriteState.KANSAI);

        turnManager = FindObjectOfType<PunTurnManager>();

        if (PhotonNetwork.IsMasterClient)
        {
            turnManager.BeginTurn(); // ゲーム開始時にターンを開始
            isKantoPlayer = true; // マスタークライアントはKantoプレイヤー
        }
        else
        {
            isKantoPlayer = false; // 2人目のプレイヤーはKansaiプレイヤー
        }

        // 元の色を保存
        originalColor = SelectedFieldCube.GetComponent<Renderer>().material.color;
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            if ((turnManager.Turn % 2 == 1 && isKantoPlayer) || (turnManager.Turn % 2 == 0 && !isKantoPlayer))
            {
                UpdateSelectedFieldPosition();
            }
        }
    }

    private void UpdateSelectedFieldPosition()
    {
        var position = SelectedFieldCube.transform.position;

        // 選択フィールドの色を元に戻す
        SelectedFieldCube.GetComponent<Renderer>().material.color = originalColor;

        if (Input.GetKeyDown(KeyCode.UpArrow) && SelectedFieldCubePosY < FIELD_SIZE_Y - 3)
        {
            SelectedFieldCubePosY++;
            SelectedFieldCube.transform.position = new Vector3(position.x, position.y, position.z + 1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && SelectedFieldCubePosY > -2)
        {
            SelectedFieldCubePosY--;
            SelectedFieldCube.transform.position = new Vector3(position.x, position.y, position.z - 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && SelectedFieldCubePosX > -2)
        {
            SelectedFieldCubePosX--;
            SelectedFieldCube.transform.position = new Vector3(position.x - 1, position.y, position.z);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && SelectedFieldCubePosX < FIELD_SIZE_X - 3)
        {
            SelectedFieldCubePosX++;
            SelectedFieldCube.transform.position = new Vector3(position.x + 1, position.y, position.z);
        }

        // 選択フィールドの色を変更
        SelectedFieldCube.GetComponent<Renderer>().material.color = Color.yellow;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (_FieldState[SelectedFieldCubePosX + 2, SelectedFieldCubePosY + 2] == SpriteState.NONE)
            {
                photonView.RPC("PlaceStone", RpcTarget.All, SelectedFieldCubePosX, SelectedFieldCubePosY, isKantoPlayer ? SpriteState.KANTO : SpriteState.KANSAI);
            }
        }
    }

    [PunRPC]
    private void PlaceStone(int x, int y, SpriteState playerTurn)
    {
        _FieldState[x + 2, y + 2] = playerTurn;
        if (playerTurn == SpriteState.KANTO)
        {
            _KantoStoneObj[x + 2, y + 2].SetState(SpriteState.KANTO);
        }
        else
        {
            _KansaiStoneObj[x + 2, y + 2].SetState(SpriteState.KANSAI);
        }

        // ターンの変更
        if (turnManager != null)
        {
            turnManager.BeginTurn(); // 次のターンを開始
        }

        // 盤が埋まったかどうかをチェック
        CheckGameEnd();
    }

    private void CheckGameEnd()
    {
        for (int y = 0; y < FIELD_SIZE_Y; y++)
        {
            for (int x = 0; x < FIELD_SIZE_X; x++)
            {
                if (_FieldState[x, y] == SpriteState.NONE)
                {
                    return; // 空のマスがある場合、ゲームは続行
                }
            }
        }

        // 盤が埋まった場合、ゲーム終了
        Debug.Log("Game Over");
        // ゲーム終了処理をここに追加
    }
}