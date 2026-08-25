/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 动画遮罩与动作辅助 / Animation mask and action helper
 * 功能 / Purpose: 封装角色动画层、遮罩或指定动作状态的运行时控制。
 * English: Wraps runtime control for character animation layers, masks, or specific action states.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stand_mask : MonoBehaviour
{
    public bool is_autoAcitvate;
    public string[] 遮罩布尔名称;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
            if(is_autoAcitvate)
            Activate_maskAnime();
    }
    public void Activate_maskAnime()
    {
        foreach (var item in 遮罩布尔名称)
        {
            GetComponent<Animator>().SetBool(item ,true);
        }
         
    }
     public void DeActivate_maskAnime()
    {
         foreach (var item in 遮罩布尔名称)
        {
            GetComponent<Animator>().SetBool(item ,false);
        }
    }
}
