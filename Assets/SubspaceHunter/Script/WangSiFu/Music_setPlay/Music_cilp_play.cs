/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 音频系统 / Audio system
 * 功能 / Purpose: 控制背景音乐、动画音效、脚步声、挥砍声和系统音量设置。
 * English: Controls background music, animation sounds, footsteps, swing sounds, and system volume settings.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music_cilp_play : MonoBehaviour
{
    // Start is called before the first frame update
   public AudioClip[] clip;

   public void Play_standardOpen_music()
   {
       AudioSource audios=GetComponent<AudioSource>();
       audios.clip=clip[0];
       audios.Play();
   }

   public void Play_standardClose_music()
   {
         AudioSource audios=GameObject.FindWithTag("Global_Music_play").GetComponent<AudioSource>();
       audios.clip=clip[1];
       audios.Play();
   }

   public void Play_Close_music_self()
   {
         AudioSource audios=GetComponent<AudioSource>();
       audios.clip=clip[1];
       audios.Play();
   }

}
