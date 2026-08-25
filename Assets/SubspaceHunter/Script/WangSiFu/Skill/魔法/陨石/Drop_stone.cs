/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 陨石魔法 / Meteor magic
 * 功能 / Purpose: 控制陨石类魔法的生成、下落和命中表现。
 * English: Controls meteor magic spawning, falling, and impact presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop_stone : MonoBehaviour
{
    public Transform fireStone_gerPoint;
    public GameObject fireStone;
    
    public void Ger_fireStone_attack()
    {
         Instantiate(fireStone,fireStone_gerPoint.position,fireStone_gerPoint.rotation);
    }
}
