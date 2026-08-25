/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 音频系统 / Audio system
 * 功能 / Purpose: 控制背景音乐、动画音效、脚步声、挥砍声和系统音量设置。
 * English: Controls background music, animation sounds, footsteps, swing sounds, and system volume settings.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Sound_unit : MonoBehaviour
{
    public enum settingType
    {
        总音量,
        音乐,
        音效,
        战斗,
        环境,
        脚步,
        系统
    }

    public settingType 设置类型 = settingType.总音量;
    public Slider slider;
    public TextMeshPro textMeshProUGUI;

    public Sound_setting sound_Setting; //玩家可以实时调试声音大小，但是没有保存就恢复原样，所以要记住刚开始初始化的参数。

    public float value()
    {
        return slider.value;
    }
    
    public void set_init_status(float value)
    {
        slider.value = value;
        textMeshProUGUI.text = Convert2_percent(value).ToString();
    }

    private int Convert2_percent(float amount)
    {
        return Convert.ToInt16((1-Math.Abs(amount/slider.minValue))*100);
    }

    public void Add_amount()
    {
         float Once_add =0.8f*10;
         if(slider.value+Once_add<slider.maxValue)
         {
            slider.value+=Once_add;
         }else if(slider.value!=slider.maxValue)
         {
                slider.value=slider.maxValue;
         }
         set_init_status(slider.value);
         set_audio(slider.value);
    }

    public void Minus_amount()
    {
             float Once_add =0.8f*10;
         if(slider.value-Once_add>slider.minValue)
         {
            slider.value-=Once_add;
         }else if(slider.value!=slider.minValue)
         {
                slider.value=slider.minValue;
         }
         set_init_status(slider.value);
         set_audio(slider.value);
    }

    private void set_audio(float amount)
    {
        switch (设置类型)
        {
            
            case settingType.总音量:
             sound_Setting.audioMixer.SetFloat("Master",amount);
            break;
            case settingType.音乐:
             sound_Setting.audioMixer.SetFloat("Music",amount);
            break;
            case settingType.音效:
             sound_Setting.audioMixer.SetFloat("SoundEffect",amount);
            break;
            case settingType.环境:
             sound_Setting.audioMixer.SetFloat("Environment",amount);
            break;
            case settingType.战斗:
             sound_Setting.audioMixer.SetFloat("Battle",amount);
            break;
            case settingType.脚步:
             sound_Setting.audioMixer.SetFloat("Footstep",amount);
            break;
            case settingType.系统:
             sound_Setting.audioMixer.SetFloat("System",amount);
            break;
        }
    }

}
