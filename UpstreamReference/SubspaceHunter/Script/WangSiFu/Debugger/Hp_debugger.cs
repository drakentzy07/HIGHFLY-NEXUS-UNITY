/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 调试工具 / Debug utility
 * 功能 / Purpose: 提供血量、状态或运行时参数的调试入口。
 * English: Provides debug entry points for HP, state, or runtime parameters.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hp_debugger : MonoBehaviour
{
    // Start is called before the first frame update\
   // public HVRController controller;
    public Player_Manager player_Manager;
  //  public HVRController LeftController => HVRInputManager.Instance.LeftController;
    

       

     /*   private bool checkinput()
        {
           if (LeftController.ControllerType == HVRControllerType.Vive)
            {
                return LeftController.TrackPadUp.JustActivated;
            }

            if (LeftController.ControllerType == HVRControllerType.WMR)
            {
                return LeftController.TrackPadDown.JustActivated;
            }

            return LeftController.PrimaryButtonState.JustActivated;
        }

        private void Update() {
            if(checkinput())
            {
                player_Manager.Minus_Hp(10f);
            }
        }*/

        
}
