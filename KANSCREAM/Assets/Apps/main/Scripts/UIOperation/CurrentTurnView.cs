using UnityEngine;

namespace refactor
{
    public class CurrentTurnView : MonoBehaviour
    {
        [SerializeField] GameObject _kansaiTurnObj; // フィールド名を変更
        [SerializeField] GameObject _kantoTurnObj; // フィールド名を変更

        private BoardManager _boardManager;

        void Start()
        {
            _boardManager = FindObjectOfType<BoardManager>();
            if (_boardManager == null)
            {
                Debug.LogError("BoardManager component is missing from this GameObject");
            }
        }

        void Update()
        {
            if (_boardManager != null)
            {
                if (_boardManager.GetTurnState() == BoardManager.CellState.KANTO)
                {
                    _kantoTurnObj.SetActive(true);
                    _kansaiTurnObj.SetActive(false);
                }
                else
                {
                    _kantoTurnObj.SetActive(false);
                    _kansaiTurnObj.SetActive(true);
                }
            }
        }
    }
}