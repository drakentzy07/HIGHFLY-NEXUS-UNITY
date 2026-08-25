/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 控制器魔法输入 / Controller magic input
 * 功能 / Purpose: 把控制器输入映射到魔法选择、准备和释放流程。
 * English: Maps controller input to magic selection, preparation, and release flow.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Magic_controller : MonoBehaviour
{
   public GameObject Magic_choice;
   public Transform Left_gerPoint;
   public Transform Right_gerPoint;
   public Transform Fater_point;

    

   private GameObject Magic_choiceInstanceL;
   private GameObject Magic_choiceInstanceR;

  // public GameObject Magic_aim_R;
   //public GameObject Magic_aim_L;

    private Player_magicController player_MagicController_L;
    private Player_magicController player_MagicController_R;

    public GameObject Magic_barSet_R;
    public GameObject Magic_barSet_L;

    void Start()
    {
        player_MagicController_L = GameObject.FindWithTag("Magic_leftHand").GetComponent<Player_magicController>();
        player_MagicController_R = GameObject.FindWithTag("Magic_rightHand").GetComponent<Player_magicController>();
    
    }

   private bool CheckLeftHand_hold()
   {
      if(!player_MagicController_L.is_magic_weapon())
      {
         GameObject res = GameObject.FindWithTag("Weapon_inHand");
        //print("进入魔法9，查找物体为"+res==null? false : true);
         return res==null? false : true;
      }else
         return false;
       
   }

    private bool CheckRightHand_hold()
   {
        if(!player_MagicController_R.is_magic_weapon())
        {
             GameObject res = GameObject.FindWithTag("Weapon_inRightHand");
           // print("进入魔法10，查找物体为"+res==null? false : true);
            return res==null? false : true;
        }else
         return false;
       
   }

   private void Update() {  
        if(OVRInput.GetDown(OVRInput.Button.Two,OVRInput.Controller.RTouch)&&!CheckRightHand_hold())
        {
            print("进入魔法1");
            if(null==Magic_choiceInstanceR)
            {
                print("进入魔法2");
                Magic_choiceInstanceR = Instantiate(Magic_choice,Right_gerPoint.position,Right_gerPoint.rotation);
                Magic_choiceInstanceR.transform.SetParent(Fater_point);
                Magic_choiceInstanceR.transform.LookAt(GameObject.FindWithTag("MainCamera").transform);

            }else
            {
                     print("进入魔法3");
                    if(null!= Magic_choiceInstanceR)
                    {
                        print("进入魔法4");
                        Destroy(Magic_choiceInstanceR);
                    }
            }
            
        }
        
      /*  else
        {
            print("进入魔法3");
            if(null!= Magic_choiceInstanceR)
            {
                print("进入魔法4");
                Destroy(Magic_choiceInstanceR);
            }
        }*/

         if(OVRInput.GetDown(OVRInput.Button.Two,OVRInput.Controller.LTouch)&&!CheckLeftHand_hold())
        {
            print("进入魔法5");
             if(null==Magic_choiceInstanceL)
             {
                print("进入魔法6");
                 Magic_choiceInstanceL = Instantiate(Magic_choice,Left_gerPoint.position,Left_gerPoint.rotation);
                Magic_choiceInstanceL.transform.SetParent(Fater_point);
                Magic_choiceInstanceL.transform.LookAt(GameObject.FindWithTag("MainCamera").transform);
             }else
             {
                print("进入魔法7");
                if(null!= Magic_choiceInstanceL)
                {
                    print("进入魔法8");
                    Destroy(Magic_choiceInstanceL);
                }
             }
           
        }
        /*else
        {
            print("进入魔法7");
             if(null!= Magic_choiceInstanceL)
            {
                print("进入魔法8");
                Destroy(Magic_choiceInstanceL);
            }
        }*/

        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger,OVRInput.Controller.LTouch)&&!CheckLeftHand_hold()&&player_MagicController_L.magic_ready())
        {
                 Magic_barSet_L.SetActive(true);
                Magic_barSet_L.GetComponent<Magic_SkillBar_controller>().prepare(player_MagicController_L.Get_Cur_prepareTime());
        }
        if(OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger,OVRInput.Controller.LTouch)&&!CheckLeftHand_hold())
        {
                print("进入关闭按钮");
                  Magic_barSet_L.SetActive(false);
                if(Magic_barSet_L.GetComponent<Magic_SkillBar_controller>().is_skillReady)
                {
                    print("进入关闭按钮1");
                    player_MagicController_L.Deal_magic_attack();
                    player_MagicController_L.Deactivate_magic_ball();
                    Magic_barSet_L.GetComponent<Magic_SkillBar_controller>().Reset_bar_status();
                }else
                {
                    print("进入关闭按钮2");
                    player_MagicController_L.Deactivate_magic_ball();
                    Magic_barSet_L.GetComponent<Magic_SkillBar_controller>().Pause_bar();
                }
        }

        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger,OVRInput.Controller.RTouch)&&!CheckRightHand_hold()&&player_MagicController_R.magic_ready())
        {
               // Magic_aim_R.SetActive(true);
                Magic_barSet_R.SetActive(true);
                Magic_barSet_R.GetComponent<Magic_SkillBar_controller>().prepare(player_MagicController_R.Get_Cur_prepareTime());
                // if(Magic_barSet_R.GetComponent<Magic_SkillBar_controller>().is_skillReady)
                
               
        }
        if(OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger,OVRInput.Controller.RTouch)&&!CheckRightHand_hold())
        {
                //Magic_aim_R.SetActive(false);
                Magic_barSet_R.SetActive(false);
                if(Magic_barSet_R.GetComponent<Magic_SkillBar_controller>().is_skillReady)
                {
                    player_MagicController_R.Deal_magic_attack();
                    player_MagicController_R.Deactivate_magic_ball();
                    Magic_barSet_R.GetComponent<Magic_SkillBar_controller>().Reset_bar_status();
                }else
                {
                    player_MagicController_R.Deactivate_magic_ball();
                    Magic_barSet_R.GetComponent<Magic_SkillBar_controller>().Pause_bar();
                }
                    
        }

   }
}
