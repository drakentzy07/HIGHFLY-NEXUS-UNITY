/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人受击状态辅助 / Enemy hit-state helper
 * 功能 / Purpose: 维护敌人受击、硬直、打断和全局命中状态。
 * English: Maintains enemy hit reactions, stagger, interrupts, and global hit state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit_Reaction :  HitState_Global 
{
    
    public Hit_type hit_Type = Hit_type.无;
    public GameObject 击晕特效;

    public void Set_hit_type(Hit_type Type)
    {
        hit_Type = Type;
    }

    public Hit_type GetHit_Type()
    {
        return hit_Type;
    }

    public void Reset()
    {
        hit_Type = Hit_type.无;
        if(击晕特效.activeSelf) 击晕特效.SetActive(false);
    }

    




}
