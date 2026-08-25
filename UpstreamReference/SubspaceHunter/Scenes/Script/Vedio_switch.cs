/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 公开 Demo 场景桥接脚本 / Public demo scene bridge script
 * 功能 / Purpose: 用于在公开演示场景中连接按钮、视频、语音展示和 AR/VR 场景对象激活逻辑。
 * English: Connects buttons, video, voice display, and AR/VR scene object activation logic in public demo scenes.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vedio_switch : MonoBehaviour
{
        public GameObject[] child_vedio;
        public int index=0;

        private void Update() {
            if(OVRInput.GetDown(OVRInput.Button.Two))
            {
                    switch_2Next();
            }
        }
        public void switch_2Next()
        {
             if(++index<child_vedio.Length)
             {
                 child_vedio[index-1].SetActive(false);
                 child_vedio[index].SetActive(true);
             }
             else
             {
                 Destroy(transform.gameObject);
             }
        }
}
