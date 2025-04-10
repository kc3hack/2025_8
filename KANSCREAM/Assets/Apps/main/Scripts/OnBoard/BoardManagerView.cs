using UnityEngine;
using UnityEngine.UI;

namespace refactor
{
    public class BoardManagerView : MonoBehaviour
    {
        [SerializeField] private GameObject _kantoPiece;
        [SerializeField] private GameObject _kansaiPiece;
        [SerializeField] private GameObject _kantoParent;
        [SerializeField] private GameObject _kansaiParent;

        public void Initialize()
        {
            for (int x = 0; x < InGamePresenter.MAX_X; x++)
            {
                for (int z = 0; z < InGamePresenter.MAX_Z; z++)
                {
                    var kantoPiece = Instantiate(_kantoPiece, _kantoParent.transform);
                    kantoPiece.transform.localPosition = new Vector3(x, 0.015f, z);
                    kantoPiece.SetActive(false);

                    var kansaiPiece = Instantiate(_kansaiPiece, _kansaiParent.transform);
                    kansaiPiece.transform.localPosition = new Vector3(x, 0.015f, z);
                    kansaiPiece.SetActive(false);
                }
            }
        }
        public void Show(int x, int z, BoardManagerModel.CellState cellState)
        {
            if (cellState == BoardManagerModel.CellState.KANTO)
            {
                _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject.SetActive(true);
            }
            else if (cellState == BoardManagerModel.CellState.KANSAI)
            {
                _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject.SetActive(true);
            }
        }

        public void Hide(int x, int z, BoardManagerModel.CellState cellState)
        {
            if (cellState == BoardManagerModel.CellState.KANTO)
            {
                _kantoParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject.SetActive(false);
            }
            else if (cellState == BoardManagerModel.CellState.KANSAI)
            {
                _kansaiParent.transform.GetChild(x * InGamePresenter.MAX_Z + z).gameObject.SetActive(false);
            }
        }
    }
}