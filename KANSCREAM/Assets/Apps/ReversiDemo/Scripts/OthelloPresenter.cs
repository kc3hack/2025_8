using UnityEngine;
using R3;
public class OthelloPresenter : MonoBehaviour
{
    private OthelloModel _model;
    private OthelloView _view;

    void Start()
    {
        _model = new OthelloModel();
        _view = FindObjectOfType<OthelloView>();
        _model.Initialize();
        Bind();
    }

    private void Update()
    {
        UpdateSelectedFieldPosition();
    }

    private void UpdateSelectedFieldPosition()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            _model.UpdateSelectedFieldPosition();
        }
    }

    private void Bind()
    {
        _model.TurnNum.Subscribe(turnNum =>
        {
            _view.TurnNumView.SetTurnNum(turnNum);
        });
    }
}