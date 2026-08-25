/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 音频系统 / Audio system
 * 功能 / Purpose: 控制背景音乐、动画音效、脚步声、挥砍声和系统音量设置。
 * English: Controls background music, animation sounds, footsteps, swing sounds, and system volume settings.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bgm_manager : MonoBehaviour
{
   public AudioSource start_bgm;
   public AudioSource Loop;
   public AudioSource End;

    private void Start() {
        start_bgm.Play();
        Invoke("paly_loop",4.5f);
    }
    
    private void paly_loop()
    {
        Loop.Play();
    }


   public void Play_End()
   {
       Loop.Stop();
       End.Play();
   }

   private void Update() {
       if(Input.GetKeyDown(KeyCode.E))
       {
           Play_End();
       }
   }
}
