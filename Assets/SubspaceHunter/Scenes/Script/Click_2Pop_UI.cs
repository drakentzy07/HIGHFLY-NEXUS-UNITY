/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 公开 Demo 场景桥接脚本 / Public demo scene bridge script
 * 功能 / Purpose: 用于在公开演示场景中连接按钮、视频、语音展示和 AR/VR 场景对象激活逻辑。
 * English: Connects buttons, video, voice display, and AR/VR scene object activation logic in public demo scenes.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_2Pop_UI : MonoBehaviour
{
     public GameObject Hint_prefab;
      public string Gernerate_point;
   
    
    public void 标准生成()
    {
        //生成UI，并且把信息填入
        GameObject point=GameObject.FindWithTag(Gernerate_point);
        Player_UI_Controller uI_Controller=GameObject.FindWithTag("Player").GetComponent<Player_UI_Controller>();
       GameObject generation_ui = uI_Controller.Web_Generate_UI(Hint_prefab,point);
      
    }

     
}
