using UnityEngine;
using UnityEngine.UI;

namespace refactor
{
    public class RestartButton : MonoBehaviour
    {
        [SerializeField] private GameObject _startPopUp;

        /// <summary>
        /// リセットボタンがクリックされたときの処理
        /// </summary>
        public void OnClick()
        {
            _startPopUp.SetActive(true);
        }
    }
}