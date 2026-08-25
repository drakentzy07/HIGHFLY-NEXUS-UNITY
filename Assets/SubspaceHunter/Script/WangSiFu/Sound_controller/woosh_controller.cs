/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 音频系统 / Audio system
 * 功能 / Purpose: 控制背景音乐、动画音效、脚步声、挥砍声和系统音量设置。
 * English: Controls background music, animation sounds, footsteps, swing sounds, and system volume settings.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class woosh_controller : MonoBehaviour
{
   public bool is_startPlay;
    public AudioClip[] weapon_woosh;
    public GameObject Temp_Ger_sounds;
    private bool is_interval_new;
    public float OneShot_interval=0.2f;
    private bool One_shot;
    public void play_WeaponWoosh()
   {
          AudioSource audio = GetComponent<AudioSource>();
         //  if(audio.isPlaying || audio==null) return;
           int index=Random.Range(0,weapon_woosh.Length);
           audio.clip=weapon_woosh[index];
           audio.Play();
       
   }

   private void Reset_oneShot()
   {
      One_shot=false;
   }

   public void Play_oneShot()
   {
      if(One_shot)return ;
      One_shot=true;
      Invoke("Reset_oneShot",OneShot_interval);
      AudioSource audio = GetComponent<AudioSource>();
      int index=Random.Range(0,weapon_woosh.Length);
      audio.clip=weapon_woosh[index];
       audio.Play();
   }

   private void reset_interval()
   {
      is_interval_new=false;
   }

   public void play_NotCorruptSounds()
   {
      if(is_interval_new) return ;
      is_interval_new=true;
      Invoke("reset_interval",0.02f);
      int index=Random.Range(0,weapon_woosh.Length);
      GameObject tempSound=Instantiate(Temp_Ger_sounds,transform.position,transform.rotation);
      tempSound.GetComponent<AudioSource>().clip=weapon_woosh[index];
      tempSound.GetComponent<AudioSource>().Play();
      Destroy(tempSound,weapon_woosh[index].length);
   }

   private void Start() {
      if(is_startPlay)
      {
            play_WeaponWoosh();
      }
   }
}
