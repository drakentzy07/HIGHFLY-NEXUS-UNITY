/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 教程 UI / Tutorial UI
 * 功能 / Purpose: 控制教程界面生成、步骤推进和提示显示。
 * English: Controls tutorial UI generation, step progression, and hint display.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tour_startGer : MonoBehaviour
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
    public Transform Player_headTransform;
    public GameObject 中文教程;
    public GameObject 英文教程;
    public language_type 语言种类=language_type.中文;
    private GameObject Tmp_gameObject;
    public float ger_time=7f;

    private void OnEnable() {
        Invoke("enable_tourUI",ger_time);
       
    }
    
    private void enable_tourUI()
    {
        
        Vector3 Ger_point=new Vector3(transform.position.x,Player_headTransform.position.y,transform.position.z);
        
         switch(语言种类)
        {
            case language_type.中文:
            Tmp_gameObject = Instantiate(中文教程,Ger_point,transform.rotation);
            break;
            case language_type.英文:
            Tmp_gameObject = Instantiate(英文教程,Ger_point,transform.rotation);
            break;
        }
        
        Vector3 look_point=new Vector3(Player_headTransform.position.x,Tmp_gameObject.transform.position.y,Player_headTransform.position.z);
        Tmp_gameObject.transform.LookAt(look_point);
    }
}
