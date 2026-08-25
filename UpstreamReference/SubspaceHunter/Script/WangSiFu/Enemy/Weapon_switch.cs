/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_switch : MonoBehaviour
{
    public GameObject Weapon_inPack;
    public GameObject Weapon_inHand;
   
   public void Get_weapon_inHand()
   {
        Weapon_inPack.SetActive(false);
        Weapon_inHand.SetActive(true);
   }

   public void rePack_weapon()
   {
        Weapon_inPack.SetActive(true);
        Weapon_inHand.SetActive(false);
   }

}
