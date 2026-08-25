/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_circleBrith : MonoBehaviour
{
    public GameObject Enemy_prefab;
    public ParticleSystem brith_particle;
    public bool auto_play;
    public float effect_time=6f;
    public float Enemy_gen_time=8f;
    public Rigidbody rig;


    private void Start() {
        if(auto_play) Delay_ger_Enemey();
    }
    private void Gernerate_enemy()
    {
        
        brith_particle.Stop();
       GameObject ene= Instantiate(Enemy_prefab,transform.position,transform.rotation);
       ene.transform.SetParent(this.transform);

    }

    private void Update() {
         if(Input.GetKeyDown(KeyCode.G))
         {
             if(null!=rig)
             rig.useGravity=true;
              Delay_ger_Enemey();
         }
    }

    public void Generate_enemy()
    {
         if(null!=rig)
             rig.useGravity=true;
              Delay_ger_Enemey();
    }

    private void play_effect()
    {
        GameObject father=transform.parent.gameObject;
        transform.SetParent(null);
        Destroy(father);
        brith_particle.Play();
    }
    public void Delay_ger_Enemey()
    {
       // brith_particle.Play();
        Invoke("play_effect",effect_time);
        Invoke("Gernerate_enemy",Enemy_gen_time);

    }
}
