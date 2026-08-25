/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人出场与武器显隐 / Enemy entrance and weapon visibility
 * 功能 / Purpose: 控制敌人出场阶段、行为树启用时机和武器模型显示。
 * English: Controls enemy entrance timing, behavior-tree activation, and weapon model visibility.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;


public class BehaTree_contor : MonoBehaviour
{
     public BehaviorTree tree;
     public AudioClip 出场嘶吼;
     public GameObject[] 出场特效;

     public void Monster_Roar()
     {
          GetComponent<AudioSource>().clip=出场嘶吼;
           GetComponent<AudioSource>().Play();
     }

     public void play_Enemy_effect()
     {
         foreach(var effect in 出场特效)
         {
             effect.GetComponent<ParticleSystem>().Play();
         }
     }
    
     public void enable_treeDesigner()
     {
         tree.enabled=true;
     }
    
}
