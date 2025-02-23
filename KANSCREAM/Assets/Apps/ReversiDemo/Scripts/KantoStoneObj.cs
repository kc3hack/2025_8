using UnityEngine;
// using DG.Tweening;
// using Cysharp.Threading.Tasks;
// using System.Threading;

public class KantoStoneObj : MonoBehaviour
{
    [SerializeField] private AudioSource sound;
    // private Sequence sequence;
    void Start()
    {
        sound = GetComponent<AudioSource>();
        this.transform.rotation = Quaternion.Euler(0, 180, 0);
    }
    public void SetState(OthelloSystem.SpriteState state)
    {
        var isActived = state != OthelloSystem.SpriteState.NONE && state != OthelloSystem.SpriteState.KANSAI;
        gameObject.SetActive(isActived);
    }
}