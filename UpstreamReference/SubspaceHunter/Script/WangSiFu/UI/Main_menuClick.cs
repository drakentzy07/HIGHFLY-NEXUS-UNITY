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


public class Main_menuClick : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("按钮的名字")]
    public string Button_name;
    [Header("按钮的详细信息")]
    [Tooltip("点击按钮时左侧信息栏显示对按钮的说明")]
    [TextArea]
    public string Information;
    public Sprite self_sprite;

    [Header("对应的信息显示组件")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI Display_information;
    public Image icon;


    [Header("点击按钮后应该刷新的子项目预制体")]
    public GameObject[] Sub_Child_item;

    

    [Header("UI菜单动画控制器")]
    public Animator UI_animator;

    [Header("Menu_right预制体")]
    public GameObject Menu_right;
    
     [Header("左菜单")]
    public GameObject Menu_left;

    public GameObject BGM;
    
    void Start()
    {
        
    }

    //递归查找对应名字的子物体
    private Transform FindChildInTransform(Transform parent,string child)
    {
        Transform childTF = parent.Find(child);
        if(childTF != null)
        {
            return childTF;
        }
        for(int i=0;i<parent.childCount;i++){
            childTF=FindChildInTransform(parent.GetChild(i),child);
        }
        return null;
    }


   public void Generate_All_child()
   {

       print("进入子项");
        //左菜单设置
        if(Menu_left.activeSelf==false)
           Menu_left.SetActive(true);


        //清理物品菜单栏
          GameObject Pre_item_menu=GameObject.FindWithTag("item_setUI");
          if(Pre_item_menu)
          Destroy(Pre_item_menu);

        //清除之前的Menu_right
          GameObject Pre_Menu_right=GameObject.FindWithTag("Menu_right");
          if(Pre_Menu_right)
          Destroy(Pre_Menu_right);
        

         //切换左侧信息显示
            title.text=Button_name;
            Display_information.text=Information;
            icon.sprite=self_sprite;

        //生成Menu_right
        GameObject Menu_right_generatePoint=GameObject.FindWithTag("Menu_right_GeneratePoint");
        GameObject Menu_right_instant=Instantiate(Menu_right,Menu_right_generatePoint.transform.position,Menu_right_generatePoint.transform.rotation);
        Menu_right_instant.transform.SetParent(Menu_right_generatePoint.transform);
        Menu_right_instant.transform.localScale=new Vector3(1,1,1);


        //获取子项目生成位置的父节点
        Transform content_transform=Menu_right_instant.GetComponent<Content_point_locate>().content.transform;
     

    
    
       //动态生成子项
           
                 for(int flag=0;flag<Sub_Child_item.Length;flag++)
                {
                    //  print("进入生成子项");
                   GameObject child= Instantiate(Sub_Child_item[flag],content_transform.position,content_transform.rotation);
                 //   if(flag!=1)
                    child.transform.SetParent(content_transform);
                    child.transform.localScale=new Vector3(1f,1f,1f);
                    print("生成的子项名字为:"+child.gameObject.name);
                }
        
       

        //暂时的点击切换效果
        UI_manager pre_selectedUI= UI_animator.transform.GetComponent<UI_manager>();
        Vector3 temp= pre_selectedUI.Cur_selected_button.transform.position;
        pre_selectedUI.Cur_selected_button.transform.position=transform.position;
        transform.position=temp;
        pre_selectedUI.Cur_selected_button=transform.gameObject;
        BGM.transform.Find("Button_switch").GetComponent<AudioSource>().Play();;

            
       
   }

    // Update is called once per frame
    void Update()
    {
        
    }
}
