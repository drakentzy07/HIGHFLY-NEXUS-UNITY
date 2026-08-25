/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 交互 UI 系统 / Interactive UI system
 * 功能 / Purpose: 处理菜单点击、提示框、血条、物品菜单、UI 定位和玩家 HUD。
 * English: Handles menu clicks, hint boxes, HP bars, item menus, UI positioning, and player HUD.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MessageBox_controller : MonoBehaviour
{
    public TextMeshProUGUI information;
    public delegate void Mission_function();

    public Mission_function Deal_Confirm_mission;   //委托函数，将需要弹窗确认后激活的函数委托给它，点击确认后执行
    public float Close_time=7f;
    public bool is_notAuto_close;
    public bool is_Auto_End2Jump;
    public bool is_end_AutoDeal;
    public GameObject UI_father;
    public GameObject[] UI_Close_Set;
    public Destory_self destory_Self;

    change_ready change_Ready;
    // Start is called before the first frame update
   
   private void Start() {
       if(!is_notAuto_close)
       Invoke("Close_ui",Close_time);
   }

   public void Deal_Mission()
   {    
       print("执行 deal mission");

       if(null!=Deal_Confirm_mission)
        Deal_Confirm_mission ();
        else
         print("Deal mission 为空");

      
        
        GetComponent<Animator>().SetTrigger("Close_box");
       
   }


   public void Close_ui()
   {
        if(is_end_AutoDeal)   Deal_Confirm_mission();
       GetComponent<Animator>().SetTrigger("Close_box");
       if(is_Auto_End2Jump) Skip_tour();
       
       
   }

   public void Destroy_self()
   {
       Destroy(transform.parent.gameObject);
   }


   public void Skip_tour()
    {
       
      //  Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
      //  pop_HintBox_Temp.Activate_Std_messageBox();

         change_Ready=GameObject.FindWithTag("Transition").GetComponent<change_ready>();
         Scene scene = SceneManager.GetActiveScene ();
         
         change_Ready.Next_scenceName="VR_BattleRoom";
         change_Ready.HideCurrentScene();
        
    }

   public void DeActivate_UI()
   {
       if(null!=UI_father)
       UI_father.SetActive(false);
        if(UI_Close_Set.Length!=0)
        {
            foreach (var item in UI_Close_Set)
            {
                item.SetActive(false);
            }
        }

          if(null!=destory_Self)
       destory_Self.start_2waitDestroy();
       
   }


}
