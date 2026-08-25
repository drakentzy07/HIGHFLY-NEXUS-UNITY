/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 功能道具 / Functional item
 * 功能 / Purpose: 实现恢复、增益或其他可交互道具效果。
 * English: Implements recovery, buffs, or other interactable item effects.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using System;


public class Recover_hp : MonoBehaviour
{
    // Start is called before the first frame update
 
   public Player_Manager Player_controller;
   public float Recover_hpAmount=20;
   public AudioSource BGM;
   public GameObject Model;
   public SaoDeathCtr saoDeathCtr;
   
  
  
   
   

GameObject sourceGo;
   

      /*  protected override void CheckInput(HVRController controller)
        {
            var activated = GetActivated(controller);

            if (activated)
            {
                Deal_recover();
            }
        }*/

   private void Start() {
      // Player_controller=GameObject.FindWithTag("Player").GetComponent<Player_Manager>();
   }

   /*  protected virtual bool GetActivated(HVRController controller)
        {
            var activated = false;

            if (controller.ControllerType == HVRControllerType.WMR)  
            {
                activated = controller.Side == HVRHandSide.Right ? controller.TrackPadLeft.JustActivated : controller.TrackPadRight.JustActivated;
            }
            else if (controller.ControllerType == HVRControllerType.Vive)
            {
                activated = HVRInputManager.Instance.RightController.TrackPadDown.JustActivated;
            }
            else
            {
                activated = controller.PrimaryButtonState.JustActivated;
            }

            return activated;
        }*/

        public void Deal_recover()
        {
             
                BGM.Play();
                Deal_brokenEffect();
                Player_controller.Add_Hp(Recover_hpAmount);
                DestorySelf();
        }

        private void DestorySelf()
        {
            Destroy(transform.gameObject,3f);
        }
        private void Deal_brokenEffect()
        {
            Model.SetActive(false);
                saoDeathCtr.transform.gameObject.SetActive(true);
                saoDeathCtr.deathTime=0.2f;
                saoDeathCtr.Death();
        }
  


         

       





   
}
