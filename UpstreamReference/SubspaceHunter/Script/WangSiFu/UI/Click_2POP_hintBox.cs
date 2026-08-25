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

public class Click_2POP_hintBox : MonoBehaviour
{
    public enum language_type
    {
        中文,
        英文,
        日文,
        俄文,
        法文,
        德文
    }

    public language_type 语言种类=language_type.中文;
    // Start is called before the first frame update
    public GameObject Hint_prefab;
    [TextArea]
    public string 中文提示;
    [TextArea]
    public string 英文提示;
    public GameObject Gernerate_point;
    [HideInInspector]
    public GameObject generation_ui;

    Player_UI_Controller uI_Controller;
    public bool is_Pin_InMainUI;
    

    private void Start() {
        if(is_Pin_InMainUI)
        {
            Gernerate_point=GameObject.FindWithTag("MessBox_gerPoint").gameObject;
        }
    }
    
    public void Activate_Hint()
    {
        //生成UI，并且把信息填入
        uI_Controller=GameObject.FindWithTag("Hand_and_Controller").GetComponent<Player_UI_Controller>();
        generation_ui = uI_Controller.Standard_Generate_UI(Hint_prefab,Gernerate_point);

        switch(语言种类)
        {
            case language_type.中文:
            generation_ui.transform.FindChildRecursive("Information").GetComponent<TextMeshProUGUI>().text=中文提示;
            break;
            case language_type.英文:
            generation_ui.transform.FindChildRecursive("Information").GetComponent<TextMeshProUGUI>().text=英文提示;
            break;
        }
       
    }

    public void Activate_Std_Specifi(GameObject goal_prefab)
    {
         UI_manager uI_Manager=GameObject.FindWithTag("UI_system").GetComponent<UI_manager>();

         uI_Manager.Ger_infoPage(goal_prefab);

    }

     public  void Activate_Std_messageBox()
    {
        //生成UI，并且把信息填入
        // uI_Controller=GameObject.FindWithTag("Player").GetComponent<Player_UI_Controller>();
         GameObject[] playerTags=GameObject.FindGameObjectsWithTag("Hand_and_Controller");
         for(int i=0;i<playerTags.Length;i++)
         {
             uI_Controller=playerTags[i].GetComponent<Player_UI_Controller>();
             if(uI_Controller!=null)
               break;
         }
        generation_ui = uI_Controller.Standard_Generate_UI(Hint_prefab,Gernerate_point);

        switch(语言种类)
        {
            case language_type.中文:
           generation_ui.transform.FindChildRecursive("Information").GetComponent<TextMeshProUGUI>().text=中文提示;
            break;
            case language_type.英文:
            generation_ui.transform.FindChildRecursive("Information").GetComponent<TextMeshProUGUI>().text=英文提示;
            break;
        }
        
       
    }

    public void Activate_UI_plane()
    {
         GameObject[] playerTags=GameObject.FindGameObjectsWithTag("Hand_and_Controller");
         for(int i=0;i<playerTags.Length;i++)
         {
             uI_Controller=playerTags[i].GetComponent<Player_UI_Controller>();
             if(uI_Controller!=null)
               break;
         }
        generation_ui = uI_Controller.Standard_Generate_UI(Hint_prefab,Gernerate_point);
    }

    public void Ger_spec_messBox(GameObject spec_hindPrefab,string spec_string)
    {
         GameObject[] playerTags=GameObject.FindGameObjectsWithTag("Hand_and_Controller");
         for(int i=0;i<playerTags.Length;i++)
         {
             uI_Controller=playerTags[i].GetComponent<Player_UI_Controller>();
             if(uI_Controller!=null)
               break;
         }
        generation_ui = uI_Controller.Standard_Generate_UI(spec_hindPrefab,Gernerate_point);

        
        
       generation_ui.transform.FindChildRecursive("Information").GetComponent<TextMeshProUGUI>().text=spec_string;
    }

}
