/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 语音交互与生成演示 / Voice interaction and generation demo
 * 功能 / Purpose: 处理语音录制、语音驱动生成和演示物体反馈。
 * English: Handles voice recording, voice-driven generation, and demo object feedback.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_gerEffect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("disable_self",1.6f); 
    }

    private void disable_self()
    {
        transform.gameObject.SetActive(false);
    }

    
}
