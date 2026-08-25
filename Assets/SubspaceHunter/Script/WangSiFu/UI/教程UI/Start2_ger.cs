/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 教程 UI / Tutorial UI
 * 功能 / Purpose: 控制教程界面生成、步骤推进和提示显示。
 * English: Controls tutorial UI generation, step progression, and hint display.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Start2_ger : MonoBehaviour
{
    public GameObject player_ui;
    
    private void OnEnable() {
        if(null!=player_ui) player_ui.SetActive(true);
    }

}
