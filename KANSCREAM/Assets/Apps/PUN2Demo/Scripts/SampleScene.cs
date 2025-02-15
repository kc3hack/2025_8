 using Photon.Pun;
 using Photon.Realtime;
 using UnityEngine;

 public class SampleScene : MonoBehaviourPunCallbacks
 {
     private void Start() {
         // プレイヤー自身の名前を"Player"に設定する
         PhotonNetwork.NickName = "Player";

         PhotonNetwork.ConnectUsingSettings();
     }

     public override void OnConnectedToMaster() {
         PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions(), TypedLobby.Default);
     }

     public override void OnJoinedRoom() {
         var position = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
         PhotonNetwork.Instantiate("Avatar", position, Quaternion.identity);
     }
 }