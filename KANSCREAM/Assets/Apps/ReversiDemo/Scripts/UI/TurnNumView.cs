using UnityEngine;
using UnityEngine.UI;

public class TurnNumView : MonoBehaviour
{
    [SerializeField] private Text _text;
    private StateManager _stateManager;

    void Start()
    {
        _stateManager = FindObjectOfType<StateManager>();
    }


    public void SetTurnNum(int turnNum)
    {
        _text.text = $"{turnNum}";
    }
}