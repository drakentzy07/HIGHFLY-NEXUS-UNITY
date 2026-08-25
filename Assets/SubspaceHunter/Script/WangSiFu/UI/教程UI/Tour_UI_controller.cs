/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 教程 UI / Tutorial UI
 * 功能 / Purpose: 控制教程界面生成、步骤推进和提示显示。
 * English: Controls tutorial UI generation, step progression, and hint display.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tour_UI_controller : MonoBehaviour
{
    public GameObject[] Operation_Tour;
    public GameObject 教程结束;
    public GameObject 操作面板;

    private int Cur_index;

    public void Next_tour()
    {

        Operation_Tour[Cur_index].SetActive(false);
        if(Cur_index+1==Operation_Tour.Length) {教程结束.SetActive(true); 操作面板.SetActive(false); return;}
        Cur_index++;
        Operation_Tour[Cur_index].SetActive(true);
    }

    public void Previous_tour()
    {
        if(Cur_index==0) return;
        Operation_Tour[Cur_index].SetActive(false);
        Cur_index--;
        Operation_Tour[Cur_index].SetActive(true);
    }



    


}
