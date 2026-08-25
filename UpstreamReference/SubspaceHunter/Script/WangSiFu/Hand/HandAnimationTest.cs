/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 手部交互 / Hand interaction
 * 功能 / Purpose: 处理手部动画、抓取状态和持有物品表现。
 * English: Handles hand animation, grab state, and held-item presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAnimationTest : MonoBehaviour
{
   
    public bool is_autoAcitvate;
    public string[] 遮罩布尔名称;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
           // Activate_maskAnime();
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.E))
        {
            GetComponent<Animator>().SetTrigger("left");
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            GetComponent<Animator>().SetTrigger("right");
        }
    }

    public void Activate_maskAnime()
    {
        foreach (var item in 遮罩布尔名称)
        {
            GetComponent<Animator>().SetBool(item ,true);
        }
         
    }
     public void DeActivate_maskAnime()
    {
         foreach (var item in 遮罩布尔名称)
        {
            GetComponent<Animator>().SetBool(item ,false);
        }
    }
}
