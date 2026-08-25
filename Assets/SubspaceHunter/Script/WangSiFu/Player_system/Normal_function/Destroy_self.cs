/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 玩家通用工具 / Player utility
 * 功能 / Purpose: 提供玩家对象或临时对象的通用生命周期工具。
 * English: Provides common lifecycle utilities for player-related or temporary objects.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy_self : MonoBehaviour
{
    public float time=10f;
   
    // Start is called before the first frame update
    void Start()
    {
      Destroy_self_delay();
    }

    private void Destroy_self_delay()
    {
        Destroy(transform.gameObject,time);
    }

}
