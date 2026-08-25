/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 音频系统 / Audio system
 * 功能 / Purpose: 控制背景音乐、动画音效、脚步声、挥砍声和系统音量设置。
 * English: Controls background music, animation sounds, footsteps, swing sounds, and system volume settings.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounds_with_animation : MonoBehaviour
{
    public AudioClip[] idle_sounds;
   public AudioClip[] anotice_sounds;
  

   public void play_idle_sounds()
   {
       int index=Random.Range(0,idle_sounds.Length*2);
       if(index<idle_sounds.Length)
       {
           GetComponent<AudioSource>().clip=idle_sounds[index];
           GetComponent<AudioSource>().Play();
       }

   }

  

   public void play_anotice_sounds()
   {
       int index=Random.Range(0,anotice_sounds.Length*2);
       if(index<anotice_sounds.Length)
       {
           GetComponent<AudioSource>().clip=anotice_sounds[index];
           GetComponent<AudioSource>().Play();
       }

   }
}
