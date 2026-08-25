/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 节日 Boss 演示 / Holiday boss demo
 * 功能 / Purpose: 控制圣诞主题 Boss 生成或跳跃模型演示。
 * English: Controls holiday-themed boss spawning or jumping-model demos.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump_model : MonoBehaviour
{

    public Boss_gerController boss_GerController;
    public ParticleSystem hit;
    private bool once;

    private void OnCollisionEnter(Collision other) {
        if(once==false)
        {
            once=true;
            
            hit.transform.gameObject.SetActive(true);
            boss_GerController.BatlleStart();
            
        }
    }

}
