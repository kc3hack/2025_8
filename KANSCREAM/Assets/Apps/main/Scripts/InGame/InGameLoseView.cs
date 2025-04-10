using UnityEngine;
using UnityEngine.UI;

namespace refactor
{
    public class InGameLoseView : MonoBehaviour
    {
        [SerializeField] private GameObject _loseView;

        public void Initialize()
        {
            _loseView.SetActive(true);
        }

        public void Hide()
        {
            _loseView.SetActive(false);
        }

        public void Show()
        {
            _loseView.SetActive(true);
        }
    }
}