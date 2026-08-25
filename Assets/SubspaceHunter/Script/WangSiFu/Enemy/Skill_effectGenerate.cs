/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_effectGenerate : MonoBehaviour
{
    public GameObject effect;
    public float Destroy_time;
    public Transform Ger_Trans;
    public GameObject Dash;

    public void Gernerate_effect()
    {
         Enemy_Animator_Event enemy_Animator_Event =GetComponent<Enemy_Animator_Event>();
        
         /*if(null!=enemy_Animator_Event&&false!=enemy_Animator_Event.box_Detect.enable_boxDetect_NohitBack)
         {
                GameObject temp=Instantiate(effect,Ger_Trans.position,Ger_Trans.rotation);
                Destroy(temp,Destroy_time);
         }*/
         bool result=false;
         if(null!=enemy_Animator_Event.physic_weaponDetect1) result=enemy_Animator_Event.physic_weaponDetect1.enable_boxDetect_NohitBack;
         else if(null!=enemy_Animator_Event.box_Detect)
         {
                     result=enemy_Animator_Event.box_Detect.enable_boxDetect_NohitBack;
         }
         if(null!=enemy_Animator_Event&&false!=result)
         {
                GameObject temp=Instantiate(effect,Ger_Trans.position,Ger_Trans.rotation);
                Destroy(temp,Destroy_time);
         }
         
    }

    public void Gernerate_Dash()
    {
         Enemy_Animator_Event enemy_Animator_Event =GetComponent<Enemy_Animator_Event>();
        
         /*if(null!=enemy_Animator_Event&&false!=enemy_Animator_Event.box_Detect.enable_boxDetect_NohitBack)
         {
                GameObject temp=Instantiate(effect,Ger_Trans.position,Ger_Trans.rotation);
                Destroy(temp,Destroy_time);
         }*/
         bool result=false;
         if(null!=enemy_Animator_Event.physic_weaponDetect1)
         {
              if(enemy_Animator_Event.physic_weaponDetect1.enable_boxDetect||enemy_Animator_Event.physic_weaponDetect1.enable_boxDetect_NohitBack)
                     result =true;
              else
                     result=false;
         } 
         else if(null!=enemy_Animator_Event.box_Detect)
         {
                     if(enemy_Animator_Event.box_Detect.enable_boxDetect||enemy_Animator_Event.box_Detect.enable_boxDetect_NohitBack)
                      result= true;
                      else result =false;
                   //  result=enemy_Animator_Event.box_Detect.enable_boxDetect_NohitBack;
         }

         if(null!=enemy_Animator_Event&&false!=result)
         {
                GameObject temp=Instantiate(Dash,Ger_Trans.position,Ger_Trans.rotation);
                Destroy(temp,Destroy_time);
         }
         
    }


}
