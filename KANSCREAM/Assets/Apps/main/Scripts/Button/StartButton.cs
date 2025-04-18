using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    /// <summary>
    /// リセットボタンがクリックされたときの処理
    /// </summary>
    public void OnClick()
    {
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}