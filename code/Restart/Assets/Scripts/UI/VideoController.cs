using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private AudioSource backgroundMusic;

    private GameController gameController;

    private void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public void PlayVideo()
    {
        Debug.Log("play");
        videoPlayer.Play();
        backgroundMusic.Pause();
    }

    public void InterruptVideo()
    {
        videoPlayer.Stop();
        backgroundMusic.time = 18.5f;
        backgroundMusic.Play();
    }

    public void ShowSkipButton()
    {
        skipButton.SetActive(true);
    }

    public void HideSkipButton()
    {
        StartCoroutine(Deactivate(2));
    }
    
    private void Update()
    {
        //if (!videoPlayer.isPlaying) return;
        long playerCurrentFrame = videoPlayer.GetComponent<VideoPlayer>().frame;
        long playerFrameCount = Convert.ToInt64(videoPlayer.GetComponent<VideoPlayer>().frameCount);
        if (playerCurrentFrame >= playerFrameCount-1)
        {
            //if video is done playing, show level selection
            gameController.OpenSelection();
        }
    }

    private IEnumerator Deactivate(float duration) {
        yield return new WaitForSecondsRealtime(duration);
        skipButton.SetActive(false);
    }
}
