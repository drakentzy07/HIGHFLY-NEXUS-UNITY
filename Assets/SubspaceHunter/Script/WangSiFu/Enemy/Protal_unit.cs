/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Protal_unit : MonoBehaviour
{
    public Portal_unit_Ger portal_Unit_Ger;
     public Transform ger_point;
     public GameObject Potrol_obj;
     private bool is_potrol_aniame;
    // Start is called before the first frame update
    private void Start() {
        Transform player_head = GameObject.FindWithTag("Enemy_spawn").GetComponent<Battle_call>().Player_headTransform;
        transform.LookAt(new Vector3(player_head.position.x,transform.position.y,player_head.position.z));
    }

    public void Set_potrol()
    {
        print("进入裂缝生成1");
        is_potrol_aniame=true;
        
        Potrol_obj.SetActive(true);
        transform.GetComponent<Animator>().enabled=true;
        transform.GetComponent<Animator>().SetTrigger("Game_start");
        print("进入裂缝生成2");
    }

    public void Set_potrol_NoAnime()
    {
        //is_potrol_aniame=true;
       // Potrol_obj.SetActive(true);
        //battle_start();
        Invoke("battle_start",1.5f);
        //transform.GetComponent<Animator>().enabled=true;
        //transform.GetComponent<Animator>().SetTrigger("Game_start");
    }

    // Update is called once per frame
    public void battle_start()
    {
             GameObject temp = Instantiate(portal_Unit_Ger.enemy_prefab,ger_point.position,ger_point.rotation);
             temp.transform.SetParent(portal_Unit_Ger.enemy_set.transform);
             
             if(is_potrol_aniame)
             {
                if(null!=portal_Unit_Ger.Cur_music && !portal_Unit_Ger.Cur_music.isPlaying) portal_Unit_Ger.Cur_music.Play();
                Invoke("Potrol_close",1.5f);
             }
   
    }

    private void Potrol_close()
    {
        GetComponent<Animator>().SetTrigger("Game_End");
    }
}
