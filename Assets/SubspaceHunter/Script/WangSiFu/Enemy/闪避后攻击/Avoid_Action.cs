/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人闪避反击 / Enemy dodge counterattack
 * 功能 / Purpose: 控制敌人闪避后的追击、反击或动作衔接。
 * English: Controls enemy follow-up, counterattack, or action chaining after dodging.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avoid_Action : MonoBehaviour
{
   public int[] forwardAction;
   public int[] leftAction;
   public int[] rightAction;

   public int Get_forwardAction()
   {
        if(forwardAction.Length==0) return -1;
         return forwardAction[Random.Range(0,forwardAction.Length)];
   }
   public int Get_leftAction()
   {
        if(leftAction.Length==0) return -1;
         return leftAction[Random.Range(0,leftAction.Length)];
   }
   public int Get_rightAction()
   {
        if(rightAction.Length==0) return -1;
         return rightAction[Random.Range(0,rightAction.Length)];
   }


}
