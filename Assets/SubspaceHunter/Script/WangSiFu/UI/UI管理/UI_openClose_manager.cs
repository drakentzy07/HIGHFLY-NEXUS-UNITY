/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: UI 打开关闭与渐隐 / UI open/close and fade
 * 功能 / Purpose: 统一控制 UI 面板开关、显隐动画和渐隐状态。
 * English: Centralizes UI panel open/close, visibility animation, and fade state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_openClose_manager : MonoBehaviour
{

    public GameObject[]  Deactivate_set;
    public Destory_self destory_Self;
    public GameObject[]  Display_content;
     public void DeActivate_UI()
   {
      
        if(Deactivate_set.Length!=0)
        {
            foreach (var item in Deactivate_set)
            {
                item.SetActive(false);
            }
        }

          if(null!=destory_Self)
       destory_Self.start_2waitDestroy();
       
   }

   public void Activate_UI()
   {
         if(Display_content.Length!=0)
        {
            foreach (var item in Display_content)
            {
                item.SetActive(true);
            }
        }
   }

   public void play_close_animator()
   {
      GetComponent<Animator>().SetTrigger("Close_box");
   }
}
