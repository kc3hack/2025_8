// using Photon.Pun;
// using UnityEngine;

// public class Synchronize : MonoBehaviourPun
// {
//     private BoardManager _boardManager;

//     private void Start()
//     {
//         // BoardManager の参照を取得
//         _boardManager = FindObjectOfType<BoardManager>();
//         if (_boardManager == null)
//         {
//             Debug.LogError("BoardManager が見つかりません。");
//         }
//     }

//     /// <summary>
//     /// コマを置く処理を同期
//     /// </summary>
//     /// <param name="x">X座標</param>
//     /// <param name="z">Z座標</param>
//     public void SyncPlacePiece(int x, int z)
//     {
//         if (_boardManager == null)
//         {
//             Debug.LogError("BoardManager が null です。");
//             return;
//         }

//         if (PhotonNetwork.IsConnected)
//         {
//             if (photonView == null || photonView.ViewID == 0)
//             {
//                 Debug.LogError("PhotonView が正しく設定されていません。ViewID: " + (photonView != null ? photonView.ViewID.ToString() : "null"));
//                 return;
//             }

//             photonView.RPC("RPCPlacePiece", RpcTarget.All, x, z);
//         }
//         else
//         {
//             // オフラインの場合はローカルで処理
//             _boardManager.SetUpPiece(x, z);
//         }
//     }

//     /// <summary>
//     /// RPCでコマを置く処理を実行
//     /// </summary>
//     /// <param name="x">X座標</param>
//     /// <param name="z">Z座標</param>
//     [PunRPC]
//     private void RPCPlacePiece(int x, int z)
//     {
//         _boardManager.SetUpPiece(x, z);
//     }

//     /// <summary>
//     /// ターン変更を同期
//     /// </summary>
//     public void SyncTurnChange()
//     {
//         if (PhotonNetwork.IsConnected)
//         {
//             photonView.RPC("RPCTurnChange", RpcTarget.All);
//         }
//         else
//         {
//             // オフラインの場合はローカルで処理
//             _boardManager.TurnChange();
//         }
//     }

//     /// <summary>
//     /// RPCでターン変更を実行
//     /// </summary>
//     [PunRPC]
//     private void RPCTurnChange()
//     {
//         _boardManager.TurnChange();
//     }
// }