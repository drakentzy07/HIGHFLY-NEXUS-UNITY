/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_spawn : MonoBehaviour
{
    public GameObject Enemy_prefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Gernerate_enemy()
    {
        Instantiate(Enemy_prefab,transform.position,transform.rotation);
    }

    public void Delay_ger_Enemey()
    {
        Invoke("Gernerate_enemy",6f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
