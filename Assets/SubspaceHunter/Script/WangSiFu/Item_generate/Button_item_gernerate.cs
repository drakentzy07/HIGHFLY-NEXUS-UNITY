/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 道具生成 / Item generation
 * 功能 / Purpose: 根据 UI 或场景事件生成武器、道具或演示物体。
 * English: Generates weapons, items, or demo objects from UI or scene events.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_item_gernerate : MonoBehaviour
{
    public string Confirm_exist_tag="Weapon";
    public GameObject Gernerate_Mother;
    public GameObject Temp_model;
    public GameObject Real_Item;
    public string mother_tag;
    public string enemy_name;
    private Player_UI_Controller player_UI_Controller;

    
    //邮件消息等
    public GameObject  message_speech;

    public void mess_speech_click()
    {
         Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
        pop_HintBox_Temp.Activate_Std_messageBox();
 
       MessageBox_controller mess =pop_HintBox_Temp.generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
        if(mess==null) print("messcontroller为空");
        else mess.Deal_Confirm_mission+=Gernerate_mess_speech;
    }

    //生成武器
    public void Button_click()
    {
       
        Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
        pop_HintBox_Temp.Activate_Std_messageBox();

        
       MessageBox_controller mess =pop_HintBox_Temp.generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
        if(mess==null) print("messcontroller为空");
        else mess.Deal_Confirm_mission+=Gernerate_mother;
    }

    //生成敌人
    public void Button_click2()
    {
        Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
        pop_HintBox_Temp.Activate_Std_messageBox();
 
        MessageBox_controller mess =pop_HintBox_Temp.generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
        if(mess==null) print("messcontroller为空");
        else mess.Deal_Confirm_mission+=Gernerate_ene;
    }

    public void Gernerate_mess_speech()
    {
        Transform targer_trans = GameObject.FindWithTag("Item_generate_point").transform;
        if(!targer_trans) print("找不到玩家视角正前方的UI生成点");
        Transform Camera_trans = GameObject.FindWithTag("MainCamera").transform;
        if(!Camera_trans) print("找不到玩家摄像头位置");
        close_MainSystem_menu();
        GameObject Mess_speech_temp = Instantiate(message_speech,targer_trans.position,Quaternion.identity);
        Vector3 targer_point = new Vector3(Camera_trans.position.x,targer_trans.position.y,Camera_trans.position.z);
        Mess_speech_temp.transform.LookAt(targer_point);
    }

    private void close_MainSystem_menu()
    {
        GameObject[] players=GameObject.FindGameObjectsWithTag("Hand_and_Controller");
        foreach(var player in players)
        {
            Player_UI_Controller  temp = player.GetComponent<Player_UI_Controller>();
            if(null!=temp) 
            {
                player_UI_Controller=temp;
                break;
            }
        }
        if(null!=player_UI_Controller)
        {
            player_UI_Controller.关闭UI();
        }
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.T)) Gernerate_ene();
    }

    public void Gernerate_ene()
    {   
        GameObject AR_initTour = GameObject.FindWithTag("tuto_ui");
        if(null!=AR_initTour) AR_initTour.SetActive(false);
        GameObject.FindWithTag("Enemy_spawn").GetComponent<Battle_call>().Enable_battle();
        GameObject.FindWithTag("Enemy_spawn").GetComponent<Battle_call>().Set_enemeyName(enemy_name);
        close_MainSystem_menu();
        
    }


    public void Button_click3()
    {
        Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
        pop_HintBox_Temp.Activate_Std_messageBox();
 
       MessageBox_controller mess =pop_HintBox_Temp.generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
        if(mess==null) print("messcontroller为空");
        else mess.Deal_Confirm_mission+=Clean_battle;
    }
    AR_scen_controller colle;
    public void Clean_battle()
    {
         GameObject temp111 =   GameObject.FindWithTag("enemy_AR_secen_manger");
        
         if(null!= temp111)
         colle = temp111.GetComponent<AR_scen_controller>();

        if(null != colle) colle.Recovery_AR_defultLight();
        
        GameObject battle_mana=GameObject.FindWithTag("Battle_manager");
        if(null!=battle_mana)
        {
            Destroy(battle_mana);
        }
    }
    


    public void Gernerate_mother()
    {
        Transform mother_point=GameObject.FindWithTag(mother_tag).GetComponent<Transform>();
       // GameObject exist=GameObject.FindWithTag(Confirm_exist_tag);
        GameObject[] exist=GameObject.FindGameObjectsWithTag(Confirm_exist_tag);
        print("找到的武器数量为"+exist.Length);
        if(null!=mother_point&&exist.Length<3)
        {
                
             Transform Player_headTransform = Camera.main.transform;
             Vector3 origin_point=new Vector3(Player_headTransform.position.x, mother_point.position.y,Player_headTransform.position.z);
            Vector3 SYN_direct = mother_point.position - origin_point;
            Vector3 ger_position = new Vector3(mother_point.position.x,Player_headTransform.position.y+1.164f,mother_point.position.z);
            GameObject Goal_item=Instantiate(Gernerate_Mother,ger_position,Quaternion.LookRotation(SYN_direct));
          //  Goal_item.transform.rotation= Quaternion.LookRoatation(SYN_direct);
            
         

            Item_generate item_Generate=Goal_item.GetComponent<Item_generate>();
            if(null!=item_Generate)
            {
                item_Generate.Item_Model=Temp_model;
                item_Generate.Real_item=Real_Item;
            }
        }

    }

   

}
