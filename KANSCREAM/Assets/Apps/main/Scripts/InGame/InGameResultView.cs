using UnityEngine;
using UnityEngine.UI;

namespace refactor
{
    /// <summary>
    /// ゲーム結果を表示するクラス
    /// ゲームに勝った場合は勝利画面を表示し、負けた場合は敗北画面を表示する
    /// </summary>
    public class InGameResultView : MonoBehaviour
    {
        [SerializeField] private GameObject _winView;
        [SerializeField] private GameObject _loseView;

        public void Initialize()
        {
            _winView.SetActive(false);
            _loseView.SetActive(false);
        }

        /// <summary>
        /// ゲーム結果を非表示にするメソッド
        /// </summary>
        public void Hide()
        {
            _winView.SetActive(false);
            _loseView.SetActive(false);
        }

        /// <summary>
        /// ゲーム結果を表示するメソッド
        /// </summary>
        /// <param name="isWin"></param>
        public void Show(bool isWin)
        {
            if (isWin)
            {
                _winView.SetActive(true);
            }
            else
            {
                _loseView.SetActive(true);
            }
        }
    }
}