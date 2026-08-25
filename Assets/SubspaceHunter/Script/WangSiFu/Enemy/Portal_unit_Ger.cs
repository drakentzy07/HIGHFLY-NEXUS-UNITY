/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Portal_unit_Ger : MonoBehaviour
{
   [HideInInspector]
    public GameObject enemy_prefab;
   [HideInInspector]
    public GameObject enemy_set;

    
    public GameObject unit_portal_small;
    public GameObject unit_portal_middle;
    public GameObject unit_portal_large;
    public AudioSource Cur_music;

   

    public void init_paramater(GameObject prefab,GameObject set,AudioSource bgm)
    {
        enemy_prefab=prefab;
        enemy_set=set;
        Cur_music = bgm;
    }

    public void set_protal_type(string enemy_name)
    {
         switch(enemy_name)
        {
            case "狗头人小兵" :
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "暗精灵居合":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "巨斧巨人":
            unit_portal_large.SetActive(true);
            unit_portal_large.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "哥布林枪猎手":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "邪恶的鹿角":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "猪人屠夫":
            unit_portal_middle.SetActive(true);
            unit_portal_middle.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "黑衣双剑":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "皇都近卫军剑盾":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "希斯克里夫":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "弑君者匕首":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "武器大师":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "西部双枪":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "北境之腿":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "时空突击手":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "北境之拳":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "青眼恶魔":
            unit_portal_large.SetActive(true);
            unit_portal_large.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "狗头人领主":
            unit_portal_large.SetActive(true);
            unit_portal_large.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "丧尸群":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "时空危机":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "异星危机":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "殖民危机":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
             break;
            case "SAO石头人":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
            case "叛教徒尼克拉斯":
            unit_portal_large.SetActive(true);
            unit_portal_large.GetComponent<Protal_unit>().Set_potrol_NoAnime();
            break;
            case "蜥蜴人士兵":
            unit_portal_small.SetActive(true);
            unit_portal_small.GetComponent<Protal_unit>().Set_potrol();
            break;
                     
        }
    }

    
}
