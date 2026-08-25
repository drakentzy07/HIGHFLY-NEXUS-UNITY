/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 场景切换 / Scene transition
 * 功能 / Purpose: 封装加载界面、按钮跳转和进入游戏流程。
 * English: Wraps loading screens, button navigation, and game entry flow.
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enter_game : MonoBehaviour
{
   public TextMeshProUGUI textMeshProUGUI;
   public void start2_switchScene()
    {
      textMeshProUGUI.text+="进入场景跳转函数动画\n";
       Scene_jump._DBInstance().start2_switchScene();
        
    }
}
