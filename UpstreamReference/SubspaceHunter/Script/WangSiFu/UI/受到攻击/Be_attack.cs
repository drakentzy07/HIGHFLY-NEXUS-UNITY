/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 受击 UI / Damage UI
 * 功能 / Purpose: 在玩家受到攻击时触发屏幕反馈或提示表现。
 * English: Triggers screen feedback or hints when the player takes damage.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Be_attack : MonoBehaviour
{
    
  // private bool is_wait;
    public float intervalTime=0.2f;
    void OnEnable()
    {

        Invoke("wait2Close",intervalTime);
    }

    private void  wait2Close()
    {
        // is_wait=false;
         transform.gameObject.SetActive(false);
    }
}
