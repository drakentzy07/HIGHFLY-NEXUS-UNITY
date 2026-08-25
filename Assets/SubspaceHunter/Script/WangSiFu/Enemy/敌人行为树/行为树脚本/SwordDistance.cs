/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人行为树任务 / Enemy behavior-tree task
 * 功能 / Purpose: 作为 Behavior Designer 行为树节点使用，封装敌人技能冷却、距离判断、中断和计数逻辑。
 * English: Used as Behavior Designer behavior-tree nodes for enemy skill cooldowns, distance checks, interrupts, and counters.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;

public class SwordDistance : Conditional
{
    Player_distance player_Distance;
    public float checkDuration = 1.0f; // 持续检查的时间
    private float timer = 0.0f; // 计时器
    private bool isChecking = false; // 是否正在检查
    public override void OnStart()
    {
       player_Distance = GetComponent<Player_distance>();
       isChecking = true; // 开始检查
        timer = 0.0f; // 重置计时器
    }

    public override TaskStatus OnUpdate()
    {
        
        if (player_Distance == null)
        {
            Debug.LogError("未找到 Player_distance 组件!");
            return TaskStatus.Failure;
        }

        if (isChecking)
        {
            // 更新计时器
            timer += Time.deltaTime;

            // 检查玩家与剑的距离
            if (player_Distance.Check_player_sword_Distace())
            {
                // 返回成功，持续检查期间，返回成功
                return TaskStatus.Success;
            }

            // 检查是否超过一秒
            if (timer >= checkDuration)
            {
                isChecking = false; // 结束检查
                return TaskStatus.Failure; // 一秒钟内未满足条件，返回失败
            }
        }

        return TaskStatus.Running; // 仍在检查中
    }

    public override void OnEnd()
    {
        // 结束时的清理或重置
        isChecking = false;
        timer = 0.0f;
    }
}
