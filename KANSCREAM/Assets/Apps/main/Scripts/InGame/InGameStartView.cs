using UnityEngine;
using UnityEngine.UI;

namespace refactor
{
    public class InGameStartView : MonoBehaviour
    {
        [SerializeField] private GameObject _startView;

        public void Initialize()
        {
            _startView.SetActive(true);
        }

        public void Hide()
        {
            _startView.SetActive(false);
        }

        public void Show()
        {
            _startView.SetActive(true);
        }
    }
}