/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人与玩家距离控制 / Enemy-player distance control
 * 功能 / Purpose: 根据玩家距离调整敌人移动、攻击和保持距离策略。
 * English: Adjusts enemy movement, attacks, and spacing strategies based on player distance.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_distance : MonoBehaviour
{
    public float PullBack_Time=0.5f;
    public CommandExecutor commandExecutor;
    public float valid_distance=1.2f;
    public AudioSource 武器推击声音;
    public AudioSource 身体推击声音;
    public float Sword_distance=1.5f;
   
   public bool Check_player_sword_Distace()
   {
        GameObject 左手武器 = GameObject.FindWithTag("Weapon_inHand");
        GameObject 右手武器 = GameObject.FindWithTag("Weapon_inRightHand");
        GameObject[] 投掷武器 = GameObject.FindGameObjectsWithTag("Weapon_inWorld");
        if(右手武器!=null)
        {
            float dis = Vector3.Distance(右手武器.transform.position,transform.position);
            if(dis<=Sword_distance) return true;
        }
        if(左手武器!=null)
        {
            float dis = Vector3.Distance(左手武器.transform.position,transform.position);
            if(dis<=Sword_distance) return true;
        }
        if(投掷武器.Length>0)
        {
            foreach (var item in 投掷武器)
            {
                 float dis = Vector3.Distance(item.transform.position,transform.position);
                 if(dis<=Sword_distance) return true;
            }
        }

        return false;
   }
    public void Deal_push_player(float fly_time=0)
{
    if(fly_time==0.4f) print("进入技能击退");
    GameObject player = GameObject.FindWithTag("Player_body");
    float targetDis = Vector3.Distance(this.transform.position, player.transform.position);
   // if(fly_time==0.4f) print("进入技能击退2");
    if(targetDis>valid_distance) return;
    //if(fly_time==0.4f) print("进入技能击退3");
    if(fly_time>0) PullBack_Time = fly_time; 
    //if(fly_time==0.4f) print("进入技能击退4");
    if(null!=武器推击声音) 武器推击声音.Play();
    if(null!=身体推击声音) 身体推击声音.Play();
    if(null==commandExecutor) commandExecutor = GetComponent<CommandExecutor>();
    commandExecutor.excute_push_player(PullBack_Time);

}
}
