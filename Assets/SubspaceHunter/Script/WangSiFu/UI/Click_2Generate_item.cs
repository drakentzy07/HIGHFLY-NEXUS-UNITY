/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 交互 UI 系统 / Interactive UI system
 * 功能 / Purpose: 处理菜单点击、提示框、血条、物品菜单、UI 定位和玩家 HUD。
 * English: Handles menu clicks, hint boxes, HP bars, item menus, UI positioning, and player HUD.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_2Generate_item : MonoBehaviour
{
    //仅仅是暂时用来测试召唤的demo
    public GameObject item;

    public  void Generate_item()
    {
            GameObject point=GameObject.FindWithTag("Item_generate_point");
            Instantiate(item,point.transform.position,Quaternion.identity);
    }
}
