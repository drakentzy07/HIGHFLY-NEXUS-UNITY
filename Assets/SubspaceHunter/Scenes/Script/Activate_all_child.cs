/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 公开 Demo 场景桥接脚本 / Public demo scene bridge script
 * 功能 / Purpose: 用于在公开演示场景中连接按钮、视频、语音展示和 AR/VR 场景对象激活逻辑。
 * English: Connects buttons, video, voice display, and AR/VR scene object activation logic in public demo scenes.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activate_all_child : MonoBehaviour
{ 
    public GameObject[] Child;

    public  void Activate_all_childs()
    {
        for(int i=0;i<Child.Length;i++)
        {
            Child[i].SetActive(true);
        }
    }
}
