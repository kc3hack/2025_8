using UnityEngine;
using UnityEngine.UI;
public class SelectedSpriteView : MonoBehaviour
{
    public GameObject SelectedFieldCube;//選択中のフィールドを示すオブジェクト
    void Start()
    {
        //選択中のフィールドを示すオブジェクトの初期位置を設定
        SelectedFieldCube.transform.position = new Vector3(0, 1, 0);
    }
}