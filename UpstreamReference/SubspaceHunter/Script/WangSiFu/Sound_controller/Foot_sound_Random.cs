/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 音频系统 / Audio system
 * 功能 / Purpose: 控制背景音乐、动画音效、脚步声、挥砍声和系统音量设置。
 * English: Controls background music, animation sounds, footsteps, swing sounds, and system volume settings.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Foot_sound_Random : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioClip[] clip_set;
   public AudioSource Audio;
   private bool is_wait_playSound;
   public bool  play_foot_sounds;
//public HVRGlobalInputs input;
   
   
   
  
    

    public void Random_Player()
    {
        if(0!=clip_set.Length)
        {
            Audio.clip=clip_set[Random.Range(0,clip_set.Length)];
            Audio.Play();
        }
    }


    IEnumerator play_feet_sound()
	{
		is_wait_playSound=true;
	
		Random_Player();
		//AudioSource audio=GetComponent<AudioSource>();
		yield return new WaitForSeconds(1f+Audio.time);
		is_wait_playSound=false;
	}

    IEnumerator play_run_sound()
	{
		is_wait_playSound=true;
	
		Random_Player();
		//AudioSource audio=GetComponent<AudioSource>();
		yield return new WaitForSeconds(0.5f+Audio.time);
		is_wait_playSound=false;
	}

    public void StickMovement()
	{

      /*  if(input.LeftJoystickButtonState.Active)
        {
                if(is_wait_playSound==false)
			 StartCoroutine("play_run_sound");
        }
		else if(input.LeftJoystickAxis.x!=0||input.LeftJoystickAxis.y!=0)
		{
		//	print("播放音乐");
			if(is_wait_playSound==false)
			 StartCoroutine("play_feet_sound");
          
		}*/
		
	}

    private void FixedUpdate() {
       
        if(play_foot_sounds) StickMovement();
    }


}
