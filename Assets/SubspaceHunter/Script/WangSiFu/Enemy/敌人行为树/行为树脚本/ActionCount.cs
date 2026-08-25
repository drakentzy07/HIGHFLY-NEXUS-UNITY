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

public class ActionCount : EnemyAIConditional
{
    public enum CountType{
        闪避,
        近距离行动
    }
    public CommandExecutor commandexecutor;
    public CountType count类型 = CountType.闪避;
  
    // Start is called before the first frame update
    public override void OnStart()
    {
        commandexecutor = this.GetComponent<CommandExecutor>();
    }

    public override TaskStatus OnUpdate()
    {
        
         if(count类型==CountType.闪避)
        return commandexecutor.Avoidence_CD <= 0 ? TaskStatus.Success : TaskStatus.Failure;
        else if(count类型==CountType.近距离行动)
        return commandexecutor.NearAction_CD <= 0 ? TaskStatus.Success : TaskStatus.Failure;

        return TaskStatus.Failure;
    }

    public override void OnEnd()
    {
        
    }
}
