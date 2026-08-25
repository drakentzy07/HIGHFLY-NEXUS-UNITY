/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie_attacl : MonoBehaviour
{
    public float minus_hp_amount =10f;
    private void OnTriggerEnter(Collider other) {
        print("碰到的其它物体是"+other.gameObject.name);
        if(other.tag=="Player_body")
        {
            print("碰到玩家");//打到玩家扣血
            other.transform.GetComponent<Player_managerV2>().Minus_Hp(minus_hp_amount,"拳击");
            transform.gameObject.SetActive(false);
        }
    }

}
