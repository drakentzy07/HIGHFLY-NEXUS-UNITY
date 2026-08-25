/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 九宫格技能测试 / Grid skill test
 * 功能 / Purpose: 用于测试九宫格斩击、随机剑气和技能演示流程。
 * English: Used to test grid-based slashes, random blade waves, and skill demo flow.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class once_slashGer : MonoBehaviour
{
    public GameObject 剑气;
    public Transform 剑气生成点;
    public void Ger_flySlash()
    {
        Instantiate(剑气,剑气生成点.position,剑气生成点.rotation);
    }
}
