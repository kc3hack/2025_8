using TMPro;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class TurnText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI turnText; // フィールド名を変更
    private PunTurnManager turnManager;

    void Start()
    {
        turnManager = FindObjectOfType<PunTurnManager>();
        if (turnManager == null)
        {
            Debug.LogError("PunTurnManager component is missing from this GameObject");
        }
    }

    void Update()
    {
        if (turnManager != null)
        {
            turnText.text = "Turn: " + turnManager.Turn; // フィールド名を変更
        }
    }

    public void TurnTextOff()
    {
        turnText.gameObject.SetActive(false);
    }

    public void TurnTextOn()
    {
        turnText.gameObject.SetActive(true);
    }
}