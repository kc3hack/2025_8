using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SpaceVideoPlayer : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    void Start()
    {
        gameObject.SetActive(false);  // 最初は再生しないようにしたかった。
        videoPlayer.playOnAwake = false;
        videoPlayer = GetComponent<VideoPlayer>();
        gameObject.SetActive(false);  // 最初は再生しないようにしたかった。
        videoPlayer.loopPointReached += OnVideoEnd; // 動画の終了を検知

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameObject.SetActive(true);
            if (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();  // 再生を開始
            }
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        gameObject.SetActive(false);  // 動画が終わったら非アクティブ化
    }

}
