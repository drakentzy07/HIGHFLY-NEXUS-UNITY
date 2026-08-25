/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 场景切换 / Scene transition
 * 功能 / Purpose: 封装加载界面、按钮跳转和进入游戏流程。
 * English: Wraps loading screens, button navigation, and game entry flow.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_button : MonoBehaviour
{
    // Start is called before the first frame update
 
    public string Scene_name;
    change_ready change_Ready;
   
    private void OnEnable() {
        
    }

    public void Button_click()
    {
       
        Scene Cur_scene=SceneManager.GetActiveScene();
        if(Cur_scene.name==Scene_name)
          return;

 
        Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
        pop_HintBox_Temp.Activate_Std_messageBox();

  
        change_Ready=GameObject.FindWithTag("Transition").GetComponent<change_ready>();
        change_Ready.Next_scenceName=Scene_name;


        
       MessageBox_controller mess =pop_HintBox_Temp.generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
        if(mess==null) print("messcontroller为空");
        else mess.Deal_Confirm_mission+=change_sce;
    }

    public void change_sce()
    {
            print("执行场景跳跃");
            change_Ready.HideCurrentScene();
    }

    public void VR_MR_switch()
    {
       
 
        Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
        pop_HintBox_Temp.Activate_Std_messageBox();

      
         change_Ready=GameObject.FindWithTag("Transition").GetComponent<change_ready>();
         Scene scene = SceneManager.GetActiveScene ();
         if(scene.name!=Scene_name)
         {
            change_Ready.Next_scenceName= Scene_name;
         }else
         {
           // change_Ready.Next_scenceName="VR_BattleRoom";
         }
        
         
         
        
       MessageBox_controller mess =pop_HintBox_Temp.generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
        if(mess==null) print("messcontroller为空");
        else mess.Deal_Confirm_mission+=change_sce;
    }



    // Update is called once per frame
    
}
