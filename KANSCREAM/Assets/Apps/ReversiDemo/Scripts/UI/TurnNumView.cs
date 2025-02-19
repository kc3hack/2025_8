using UnityEngine;
using UnityEngine.UI;

public class TurnNumView : MonoBehaviour
{
    [SerializeField] private Text _text;
    private OthelloSystem othelloSystem;

    void Start()
    {
        othelloSystem = FindObjectOfType<OthelloSystem>();
    }


    public void SetTurnNum(int turnNum)
    {
        _text.text = $"{turnNum}";
    }
}