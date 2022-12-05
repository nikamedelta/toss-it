using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SetVolume : MonoBehaviour
{
    public AudioMixer mixer;

    public Slider master;
    public Slider background;
    public Slider player;
    public Slider world;
    public Slider ui;

    private void Start()
    {
        // set all slider according to their playerpref variables (or default)
        master.value = PlayerPrefs.GetFloat("Volume_All", 0.75f);
        background.value = PlayerPrefs.GetFloat("Volume_Background", 0.75f);
        player.value = PlayerPrefs.GetFloat("Volume_Player", 0.75f);
        world.value = PlayerPrefs.GetFloat("Volume_World", 0.75f);
        ui.value = PlayerPrefs.GetFloat("Volume_UI", 0.75f);
    }

    private void SetLevel(float sliderValue, String variable)
    {
        mixer.SetFloat(variable, Mathf.Log10(sliderValue)*20);
        PlayerPrefs.SetFloat(variable, sliderValue);
    }

    public void SetMaster(float sliderValue)
    {
        SetLevel(sliderValue, "Volume_All");
    }
    public void SetBackground(float sliderValue)
    {
        SetLevel(sliderValue, "Volume_Background");
    }
    public void SetPlayer(float sliderValue)
    {
        SetLevel(sliderValue, "Volume_Player");
    }
    public void SetUI(float sliderValue)
    {
        SetLevel(sliderValue, "Volume_UI");
    }
    public void SetWorld(float sliderValue)
    {
        SetLevel(sliderValue, "Volume_World");
    }
    
}
