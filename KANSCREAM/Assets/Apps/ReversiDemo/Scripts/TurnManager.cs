using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;

public class TurnManager : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
{
    private PunTurnManager turnManager;
    private bool isKantoPlayer;

    void Start()
    {
        turnManager = GetComponent<PunTurnManager>();
        if (turnManager == null)
        {
            Debug.LogError("PunTurnManager component is missing from this GameObject");
            return;
        }
        turnManager.TurnManagerListener = this;

        if (PhotonNetwork.IsMasterClient)
        {
            turnManager.BeginTurn(); // ゲーム開始時にターンを開始
            isKantoPlayer = true; // マスタークライアントはKantoプレイヤー
        }
        else
        {
            isKantoPlayer = false; // 2人目のプレイヤーはKansaiプレイヤー
        }
    }

    void Update()
    {
        if (turnManager == null)
        {
            return;
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            if ((turnManager.Turn % 2 == 1 && isKantoPlayer) || (turnManager.Turn % 2 == 0 && !isKantoPlayer))
            {
                // プレイヤーのターンの場合、OthelloSystemのUpdateSelectedFieldPositionを呼び出す
                FindObjectOfType<OthelloSystem>().UpdateSelectedFieldPosition();
            }
        }
    }

    public bool IsKantoPlayer()
    {
        return isKantoPlayer;
    }

    public void OnTurnBegins(int turn)
    {
        Debug.Log("Turn " + turn + " begins.");
    }

    public void OnTurnCompleted(int turn)
    {
        Debug.Log("Turn " + turn + " completed.");
    }

    public void OnPlayerMove(Player player, int turn, object move)
    {
        Debug.Log("Player " + player.NickName + " made a move on turn " + turn);
    }

    public void OnPlayerFinished(Player player, int turn, object move)
    {
        Debug.Log("Player " + player.NickName + " finished turn " + turn);
    }

    public void OnTurnTimeEnds(int turn)
    {
        Debug.Log("Turn " + turn + " time ended.");
    }
}