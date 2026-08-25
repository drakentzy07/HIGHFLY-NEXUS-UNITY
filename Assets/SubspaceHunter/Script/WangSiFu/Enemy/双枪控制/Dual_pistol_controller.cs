/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人双枪控制 / Enemy dual-pistol control
 * 功能 / Purpose: 管理敌人双枪武器、射击动作和相关特效触发。
 * English: Manages enemy dual-pistol weapons, shooting actions, and related effect triggers.
 */

using System.Collections;
using System.Collections.Generic;
using Knife.Effects.SimpleController;
using UnityEngine;

public class Dual_pistol_controller : MonoBehaviour
{
   public GameObject Gun_one_backpack;
   public GameObject Gun_one_inHand;

   public void Gun_model_switch()
   {
        if(Gun_one_backpack.activeSelf)
        {
            Gun_one_backpack.SetActive(false);
            Gun_one_inHand.SetActive(true);
        }
        else
        {
            Gun_one_backpack.SetActive(true);
            Gun_one_inHand.SetActive(false);
        }
   }

    public void Gun_around_true()
    {
        print("进入转枪");
         transform.GetComponent<Animator>().SetBool("Gun_around",true);
    }

    public void Gun_around_false()
    {
         transform.GetComponent<Animator>().SetBool("Gun_around",false);
    }

}
