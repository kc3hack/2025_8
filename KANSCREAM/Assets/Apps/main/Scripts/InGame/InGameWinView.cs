using UnityEngine;
using UnityEngine.UI;

namespace refactor
{
    public class InGameWinView : MonoBehaviour
    {
        [SerializeField] private GameObject _winView;

        public void Initialize()
        {
            _winView.SetActive(true);
        }

        public void Hide()
        {
            _winView.SetActive(false);
        }

        public void Show()
        {
            _winView.SetActive(true);
        }
    }
}