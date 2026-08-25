/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能与魔法系统 / Skill and magic system
 * 功能 / Purpose: 管理玩家技能、魔法弹体、范围攻击、命中爆炸、护盾和提示表现。
 * English: Manages player skills, magic projectiles, area attacks, hit explosions, shields, and hint presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ele_round_realease : MonoBehaviour
{
    public GameObject 定点范围攻击;
    public float 范围攻击存在时间=3f;
    public string target_tag="Player_body";
    public GameObject player_Fowrd;
    // Start is called before the first frame update
     public void Deal_range_attack()
    {
        print("执行雷电释放A");
        GameObject player=GameObject.FindWithTag(target_tag);
        if(null!=player)
        {
            Vector3 player_feet=new Vector3(player.transform.position.x,0.1f,player.transform.position.z);
          GameObject temp_magic=Instantiate(定点范围攻击,player_feet,Quaternion.identity);
             Destroy(temp_magic,范围攻击存在时间);
        }else
        {
            print("执行雷电释放B");
            //GameObject player_Fowrd=GameObject.FindWithTag("Enemy_spawn").gameObject;
            Vector3 ger_point= new Vector3(player_Fowrd.transform.position.x,0.1f,player_Fowrd.transform.position.z);
            GameObject temp_magic=Instantiate(定点范围攻击,ger_point,Quaternion.identity);
             Destroy(temp_magic,范围攻击存在时间);
        }
        
    }
}
