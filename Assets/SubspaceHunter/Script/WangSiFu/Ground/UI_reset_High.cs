/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 地面与高度校准 / Ground and height calibration
 * 功能 / Purpose: 处理玩家高度重置、地面基准和 UI 触发的校准逻辑。
 * English: Handles player height reset, ground reference, and UI-triggered calibration logic.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_reset_High : MonoBehaviour
{
    public GameObject spec_prefab;
     [TextArea]
    public string Hint_message;
    public bool is_follow_controller;

     public void Reset_groundHight()
    {
            Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
            pop_HintBox_Temp.Activate_Std_messageBox();
    
           MessageBox_controller mess =pop_HintBox_Temp.generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
            if(mess==null) print("messcontroller为空");
            else {
                if(is_follow_controller)
                   mess.Deal_Confirm_mission+=Reset_hight_withController;
                else        
                   mess.Deal_Confirm_mission+=Reset_hight;
            }
    }

    

     public void Reset_hight()
    {
        GameObject ground=GameObject.FindWithTag("Ground");
            if(null!=ground)
            {
                ground.GetComponent<Reset_high>().Start_resetHigh();
            
                Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
                pop_HintBox_Temp.Ger_spec_messBox(spec_prefab,Hint_message);
        
                MessageBox_controller mess =pop_HintBox_Temp.generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
                if(mess==null) print("messcontroller为空");
                else mess.Deal_Confirm_mission+=Reset_complete;
            }
    }

      public void Reset_hight_withController()
    {
        GameObject ground=GameObject.FindWithTag("Ground");
        if(null!=ground)
        {
            ground.GetComponent<Reset_high>().Start_resetHigh_controller();
           
            Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
            pop_HintBox_Temp.Ger_spec_messBox(spec_prefab,Hint_message);
    
            MessageBox_controller mess =pop_HintBox_Temp.generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
            if(mess==null) print("messcontroller为空");
            else mess.Deal_Confirm_mission+=Reset_complete;
        }
    }

    public void Reset_complete()
    {
         GameObject ground=GameObject.FindWithTag("Ground");
        if(null!=ground)
        {
            ground.GetComponent<Reset_high>().End_ResetHigh();
        }
    }
}
