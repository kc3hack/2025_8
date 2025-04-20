// inGamePresenterのturnを取得してきて、それをテキストに設定。

using TMPro;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using refactor;

public class TextManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI turnText; // フィールド名を変更
    private InGamePresenter _inGamePresenter;

    void Start()
    {
        _inGamePresenter = FindObjectOfType<InGamePresenter>();
        if (_inGamePresenter == null)
        {
            Debug.LogError("inGamePresenter component is missing from this GameObject");
        }
    }

    void Update()
    {
        if (_inGamePresenter != null)
        {
            turnText.text = "Turn: " + _inGamePresenter.GetTurn(); // フィールド名を変更
        }
    }
}