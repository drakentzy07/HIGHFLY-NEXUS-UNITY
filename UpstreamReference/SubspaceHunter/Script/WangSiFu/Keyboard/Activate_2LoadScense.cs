/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: VR 键盘与输入 / VR keyboard and input
 * 功能 / Purpose: 管理 VR 场景中的虚拟键盘、按键输入和场景加载触发。
 * English: Manages virtual keyboard, key input, and scene-load triggers in VR scenes.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Activate_2LoadScense : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("First_Town 1");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
