/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 音频系统 / Audio system
 * 功能 / Purpose: 控制背景音乐、动画音效、脚步声、挥砍声和系统音量设置。
 * English: Controls background music, animation sounds, footsteps, swing sounds, and system volume settings.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class woosh_fater : MonoBehaviour
{
    public woosh_controller 武器声音控制;
    public woosh_controller 脚步声音控制;

   public void play_WeaponWoosh()
   {
        武器声音控制.play_WeaponWoosh();
       
   }

   public void play_FootSounds()
   {
     if(null!=脚步声音控制)
        脚步声音控制.play_WeaponWoosh();
       
   }
    
}
