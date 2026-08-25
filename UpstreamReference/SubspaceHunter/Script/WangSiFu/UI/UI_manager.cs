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
using UnityEngine.UI;


public class UI_manager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Cur_selected_button;
    public Transform[] infoStream;

     [Header("标题文本显示")]
    public TextMeshProUGUI title;
    [Header("信息栏显示")]
    public TextMeshProUGUI Display_information;
    [Header("图标")]
    public Image icon;
    public GameObject[] 控制器教程视频合集;
    public GameObject[] 手追教程视频合集;

    public GameObject[] 魔法教程视频合集;

    public Sprite Default_icon;
    
    
    
    void Start()
    {
        Invoke("Close_animator",2.3f);
      
    }

    private void Close_animator()
    {
        transform.GetComponent<Animator>().enabled=false;
    }

    

    public int Check_childNum()
    {

        int temp_count=0;
        foreach(var unit in infoStream)
        {
            GameObject temp=unit.GetComponent<info_unit>().My_domin_UI;
            if(null!=temp)
            temp_count++;
        }
        print("已有UI数量"+temp_count);
        return temp_count;
    }

    public Transform find_min_CanUse()
    {
        
        for(int i=0;i<infoStream.Length;i++)
        {
             GameObject temp=infoStream[i].GetComponent<info_unit>().My_domin_UI;
             if(null==temp) return infoStream[i];
        }
        return null;
    }

     public void Ger_infoPage(GameObject page)
    {
        if(Check_childNum()+1>infoStream.Length) return;

        print("信息流生成A");
        Transform ger_point=find_min_CanUse();
        if(null!=ger_point)
        {
            print("信息流生成B");
            GameObject temp_info=Instantiate(page,ger_point.position,ger_point.rotation);
             temp_info.transform.LookAt(GameObject.FindWithTag("MainCamera").transform);
             ger_point.GetComponent<info_unit>().My_domin_UI=temp_info;
        }
        
      
    }

    public void Call_ALLStream2_close()
    {
        foreach(var info in infoStream)
        {
            if(null!=info.GetComponent<info_unit>().My_domin_UI)
            {
                GameObject UI=info.GetComponent<info_unit>().My_domin_UI;
                MessageBox_controller messageBox_Controller=UI.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
                messageBox_Controller.Close_ui();
            }

            
        }
    }


    public void Reset_information_stream()
   {
        title.text="information";
        icon.sprite=Default_icon;
        Display_information.text="";
   }

   public void set_sprite(Sprite sprite)
    {
        icon.sprite=sprite;
    }

    public void set_title(string title_input)
    {
        title.text=title_input;
    }

    public void set_information(string information_input)
    {
        Display_information.text=information_input;
    }


   
}
