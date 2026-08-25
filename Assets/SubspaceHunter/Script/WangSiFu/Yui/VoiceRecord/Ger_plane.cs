/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 语音交互与生成演示 / Voice interaction and generation demo
 * 功能 / Purpose: 处理语音录制、语音驱动生成和演示物体反馈。
 * English: Handles voice recording, voice-driven generation, and demo object feedback.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ger_plane : MonoBehaviour
{
    public GameObject plane;
    public void Ger_plane_start()
    {
        GameObject temp = Instantiate(plane,transform.position,transform.rotation);
        Destroy(temp,10f);
    }
}
