using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace refactor
{
    public class RestartButton : MonoBehaviourPun
    {
        [SerializeField] private GameObject _startPopUp;
        [SerializeField] private GameObject _gameSystem;

        /// <summary>
        /// リセットボタンがクリックされたときの処理
        /// </summary>
        public void OnClick()
        {
            //リスタート
            photonView.RPC("ReStart", RpcTarget.All);
        }

        [PunRPC]
        public void ReStart()
        {
            _startPopUp.SetActive(true);
            _gameSystem.GetComponent<InGamePresenter>().Restart();
        }
    }
}