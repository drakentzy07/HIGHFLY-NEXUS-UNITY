/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 交互 UI 系统 / Interactive UI system
 * 功能 / Purpose: 处理菜单点击、提示框、血条、物品菜单、UI 定位和玩家 HUD。
 * English: Handles menu clicks, hint boxes, HP bars, item menus, UI positioning, and player HUD.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_2Open_itemMenu : MonoBehaviour
{
   
    [Header("是否是一个点击后会打开物品栏的按键")]
    public bool is_click2_openItem_menu;//如果true，则除了关闭之前的item外，还会重新打开

    [Header("动态生成的物品菜单栏")]
    public GameObject Item_Menu;
    [Header("菜单生成锚点")]
    private Transform Generate_point;

    private void Start() {
       Generate_point=GameObject.FindWithTag("item_menu_generatePoint").transform;
       if(null==Generate_point) print("没找到item生成点");

        /*  GameObject Menu_right_itemGeneratePoint=GameObject.FindWithTag("test").gameObject;
         transform.SetParent(Menu_right_itemGeneratePoint.transform);
         transform.localScale=new Vector3(1f,1f,1f);*/
    }

    public void Open_item_menu()
    {
        print("进入打开菜单");
      
        //首先,点击了一个菜单之后，检查之前的菜单是否是打开的，如果是打开的
        //那就要先将之前的菜单的子项所有都要销毁，然后把物品栏UI禁用

        GameObject Pre_item_menu=GameObject.FindWithTag("item_setUI");
       

        //1销毁子项接口（等待补全）
              //if(Pre_item_menu)  ;
        //关闭物品栏UI
             if(Pre_item_menu) 
             Destroy(Pre_item_menu);
      


        //第二，检查这次点击的这个按钮 是不是一个会打开物品栏的按钮
        //如果是，则启用物品栏UI，然后动态读取刷新里面的item项
        if(is_click2_openItem_menu)
        {
                GameObject UI =Instantiate(Item_Menu,Generate_point.position,Generate_point.rotation);
            GameObject fahter_point=GameObject.FindWithTag("UI_system");
            UI.transform.SetParent(fahter_point.transform);
            UI.transform.localScale=new Vector3(271,271,271);
        }
           
         
    }
}
