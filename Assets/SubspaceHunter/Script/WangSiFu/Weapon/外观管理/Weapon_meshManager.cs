/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 武器外观管理 / Weapon mesh management
 * 功能 / Purpose: 切换或维护武器模型、外观和显示状态。
 * English: Switches or maintains weapon meshes, appearance, and visibility state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_meshManager : MonoBehaviour
{
    public GameObject[] weapon_mesh;  
    
    public void hide_weapon()
    {
        foreach (var item in weapon_mesh)
        {
            item.SetActive(false);
        }
    }

    public void Show_weapon()
    {
        foreach (var item in weapon_mesh)
        {
            item.SetActive(true);
        }
    }

}
