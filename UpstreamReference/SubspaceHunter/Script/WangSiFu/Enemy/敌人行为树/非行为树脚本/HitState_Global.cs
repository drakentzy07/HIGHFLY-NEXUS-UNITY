/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人受击状态辅助 / Enemy hit-state helper
 * 功能 / Purpose: 维护敌人受击、硬直、打断和全局命中状态。
 * English: Maintains enemy hit reactions, stagger, interrupts, and global hit state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitState_Global : MonoBehaviour{
    public enum Hit_type
    {
        击中,
        击退,
        击飞,
        击晕,
        击倒,
        无
    }
   
}
