using UnityEngine;
// using DG.Tweening;
// using Cysharp.Threading.Tasks;
// using System.Threading;


public class KansaiStoneObj : MonoBehaviour
{
    [SerializeField] private AudioSource sound;
    void Start()
    {
        sound = GetComponent<AudioSource>();
        this.transform.rotation = Quaternion.Euler(-90, 180, 0);
    }
    public void SetState(OthelloSystem.SpriteState state)
    {
        var isActived = state != OthelloSystem.SpriteState.NONE && state != OthelloSystem.SpriteState.KANTO;
        gameObject.SetActive(isActived);
    }
}