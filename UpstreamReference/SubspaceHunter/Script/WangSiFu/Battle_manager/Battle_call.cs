/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 战斗流程管理 / Battle flow management
 * 功能 / Purpose: 负责战斗开始、敌人选择与生成、计时、胜负判断和战斗音乐切换。
 * English: Handles battle start, enemy selection and spawning, timing, win/loss decisions, and combat music switching.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle_call : MonoBehaviour
{
    public Transform Player_headTransform;
    public GameObject battle_manager;
    private GameObject Tmp_gameObject;
    public bool is_MR;
    

    public void Enable_battle()
    {
        if(null!=Tmp_gameObject)
            return;
            
        Vector3 Ger_point=new Vector3(transform.position.x,Player_headTransform.position.y+3,transform.position.z);
        Tmp_gameObject = Instantiate(battle_manager,Ger_point,transform.rotation);
        Vector3 look_point=new Vector3(Player_headTransform.position.x,Tmp_gameObject.transform.position.y,Player_headTransform.position.z);
        Tmp_gameObject.transform.LookAt(look_point);
        
    }

    public void Set_enemeyName(string enemy_name)
    {
        Tmp_gameObject.GetComponent<Battle_manager>().ThisTime_enemyName=enemy_name;
    } 

}
