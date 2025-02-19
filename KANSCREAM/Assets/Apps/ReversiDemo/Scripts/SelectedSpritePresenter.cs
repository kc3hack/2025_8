using UnityEngine;
using R3;
public class SelectedSpritePresenter : MonoBehaviour
{
    private SelectedSpriteModel _model;
    private SelectedSpriteView _view;

    void Start()
    {
        _model = new SelectedSpriteModel();
        _view = FindObjectOfType<SelectedSpriteView>();
        Bind();
    }

    void Update()
    {
        UpdateSelectedFieldPosition();
    }
    private void UpdateSelectedFieldPosition()
    {
        //選択中のフィールドを移動
        if (Input.GetKeyDown(KeyCode.W))
        {
            _model.Move(0);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            _model.Move(1);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            _model.Move(2);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _model.Move(3);
        }
    }

    private void Bind()
    {
        _model.SelectedSpritePosX.Subscribe(posX =>
        {
            _view.SelectedFieldCube.transform.position = new Vector3(posX, 1, _model.SelectedSpritePosY.Value);
        });
        _model.SelectedSpritePosY.Subscribe(posY =>
        {
            _view.SelectedFieldCube.transform.position = new Vector3(_model.SelectedSpritePosX.Value, 1, posY);
        });
    }
}