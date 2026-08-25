/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: VR 键盘与输入 / VR keyboard and input
 * 功能 / Purpose: 管理 VR 场景中的虚拟键盘、按键输入和场景加载触发。
 * English: Manages virtual keyboard, key input, and scene-load triggers in VR scenes.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VR_keyboardManager : MonoBehaviour
{
    public InputField Cur_select_Field;
    public InputField UserName;
    public InputField PassWord;
    public bool is_upper;
    // Start is called before the first frame update
    void Start()
    {
        Cur_select_Field=UserName;
        Cur_select_Field.Select();
    }

    public void Switch_Cur_selectField()
    {
        if(Cur_select_Field==UserName)
           Cur_select_Field=PassWord;
        else
           Cur_select_Field=UserName;

           Cur_select_Field.Select();

    }

    public void OneClick_input(string Alhpa)
    {
        char[] Alhpa_char=Alhpa.ToCharArray();

        if(is_upper==false)
        Cur_select_Field.text+=Alhpa;
        else if(Alhpa_char[0]>='a'&&Alhpa_char[0]<='z')
        {
            string upper=Alhpa.ToUpper();
            Cur_select_Field.text+=upper;
        }else
        Cur_select_Field.text+=Alhpa;
    }

    public void Delete_oneAlhpa()
    {
       string buffer=Cur_select_Field.text;
       Cur_select_Field.text=buffer.Substring(0,buffer.Length-1);

    }

    public void Upper_switch()
    {
        if(is_upper==false)
          is_upper=true;
        else
          is_upper=false;
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
