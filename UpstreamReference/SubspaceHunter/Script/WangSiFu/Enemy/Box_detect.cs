/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box_detect : MonoBehaviour
{
    public bool enable_boxDetect;
    public bool enable_boxDetect_NohitBack;
 
    public bool enable_Minus_hp=false;
    public float minus_hp_amount=25f;
    public Vector3 checkBox;
    public Vector3 center;
    public float confront_powerSpeed=5f;
 

    // 进行检测的层级,暴露在属性页面进行勾选
	[SerializeField] LayerMask impactLayer;
	// 碰撞检测的物体
	//[SerializeField] Transform colBox;
	
    private bool is_interval=false;
    public string Weapon_type="切割";
    
    


    IEnumerator interval_reaction(Collider col)
    {
        is_interval=true;
        GetComponent<Weapon_hitEach_other>().active_souonds_effect(col,true);
        GetComponent<Weapon_hit_back>().play_hitBack();
        yield return new WaitForSeconds(0.5f);
        is_interval=false;
    }

     IEnumerator interval_reaction_NonHit_back(Collider col)
    {
        is_interval=true;
        GetComponent<Weapon_hitEach_other>().active_souonds_effect(col,false);
        col.transform.GetComponent<Physic_weaponManager>().Close_collider();
      //  GetComponent<Weapon_hitBack>().play_hitBack();
        yield return new WaitForSeconds(0.5f);
        is_interval=false;
    }

 



	private void Update()
    {
       // OnDrawGizmos();
        if(enable_boxDetect) BoxCheck();
        
    }

    

	/// <summary>
    /// 箱型检测
    /// </summary>
    /// <param name="colBox">箱子物体，可以空物体，作为武器的子物体跟随移动</param>
    void BoxCheck()
    {

        Collider[] colliders = Physics.OverlapBox(transform.position, checkBox, transform.rotation, impactLayer);
        foreach (var col in colliders)
        {
            print("箱子检测物为:"+col.name);
            if(col.tag!="Weapon_inHand"||col.tag!="Weapon_inRightHand")
               continue;
            
            if(!is_interval&&(col.tag=="Weapon_inHand"||col.tag=="Weapon_inRightHand"))
            {
               // print("箱子进入1");

               is_reach_hiBack(col);
               // print("箱子检测成功");     
            }
                  
        }
    }

    private void OnTriggerEnter(Collider other) {
        print("碰到的其它物体是"+other.gameObject.name);
        if(other.tag=="Player_body"&&false!=enable_Minus_hp&&false==is_interval)
        {
            print("碰到玩家");//打到玩家扣血
            other.transform.GetComponent<Player_managerV2>().Minus_Hp(minus_hp_amount,Weapon_type);
        }
    }



    private void is_reach_hiBack(Collider clo)
    {
         clo.transform.GetComponent<Physic_weaponManager>().Deal_short_impusle();
        if(!enable_boxDetect_NohitBack)
        {
            enable_boxDetect=false;
            if(clo.transform.GetComponent<Rigidbody>().velocity.magnitude<confront_powerSpeed)
            {
               
                StartCoroutine(interval_reaction(clo));
            }
            else
            {
               enable_Minus_hp=false;
                StartCoroutine(interval_reaction_NonHit_back(clo));   
                // Invoke("Activate_boxDetect",0.4f);   
            }
        }else
        {
                 enable_Minus_hp=false;
                StartCoroutine(interval_reaction_NonHit_back(clo));   
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;//改变线框的颜色
        Gizmos.DrawWireCube(transform.position, checkBox);//正方体虚线框
    }
}
