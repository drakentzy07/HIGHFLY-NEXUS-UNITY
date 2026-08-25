/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 语音交互与生成演示 / Voice interaction and generation demo
 * 功能 / Purpose: 处理语音录制、语音驱动生成和演示物体反馈。
 * English: Handles voice recording, voice-driven generation, and demo object feedback.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechManager : MonoBehaviour
{
   public GameObject[] English_speech;
   public GameObject[] Chinese_sppech;
   public AudioSource speech_audio;
   public GameObject effect;
   public Animator animator;
   public AudioSource End_sounds;

   public GameObject father;
   public Ger_plane ger_Plane;

   private int index=0;

   public void void_speechSound()
   {
        effect.SetActive(true);
        speech_audio.Play();
   }

   public void play_speech()
   {
        
      English_speech[index].SetActive(true);
      Chinese_sppech[index].SetActive(true);
      if(index!=0) {
        English_speech[index-1].SetActive(false);
      Chinese_sppech[index-1].SetActive(false);
      }
      index++;
      

      
   }

   public void play_speech_close()
   {
       
      
        English_speech[index-1].SetActive(false);
      Chinese_sppech[index-1].SetActive(false);
      
      

      
   }
   

   public void Deal_end()
   {
        English_speech[English_speech.Length-1].SetActive(false);
      Chinese_sppech[English_speech.Length-1].SetActive(false);
      effect.SetActive(false);
      animator.enabled=false;
      transform.gameObject.SetActive(false);
      ger_Plane.Ger_plane_start();
      father.GetComponent<Rig_hit>().enabled=true;
      father.AddComponent<Rigidbody>();
      father.GetComponent<Rigidbody>().useGravity=true;


      //End_sounds.Play();
   }
    
   
}
