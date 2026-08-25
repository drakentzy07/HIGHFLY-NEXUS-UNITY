/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 信息流 UI / Info-stream UI
 * 功能 / Purpose: 生成信息按钮并展示流式提示或消息单元。
 * English: Generates info buttons and displays stream-style hints or message units.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ger_infoButton : MonoBehaviour
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
    [Header("暂时用于中/英控制")]
    public GameObject 中文界面;
    public GameObject 英文界面;

    public GameObject Not_infoPage;

    


    public void Ger_infoPage_notInput()
    {
       
        Click_2POP_hintBox pop_HintBox_Temp=GetComponent<Click_2POP_hintBox>();
        switch(语言种类)
        {
            case language_type.中文:
             pop_HintBox_Temp.Activate_Std_Specifi(中文界面);
            break;
            case language_type.英文:
             pop_HintBox_Temp.Activate_Std_Specifi(英文界面);
            break;
        }

    }


}
