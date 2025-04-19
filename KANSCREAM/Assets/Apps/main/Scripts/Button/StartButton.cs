using UnityEngine;
using UnityEngine.UI;

namespace refactor
{
    public class StartButton : MonoBehaviour
    {
        /// <summary>
        /// リセットボタンがクリックされたときの処理
        /// </summary>
        public void OnClick()
        {
            gameObject.transform.parent.gameObject.SetActive(false);
            //GameSystemオブジェクトのInGamePresenterコンポーネントを取得
            GameObject.Find("GameSystem").GetComponent<InGamePresenter>().StartGame();
        }
    }
}