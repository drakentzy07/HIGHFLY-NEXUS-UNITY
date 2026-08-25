/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EzySlice;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityQuaternion;

public class Enemy_statusController : HitState_Global 
{
    public bool is_multi_hpBar;
    public float HP_amount=100;
    public float Max_Hp=100;
    public Image Hp_image;
    public Image[] Boss_hp_image_set;

    private bool is_interval=false;

    private GameObject sourceGo;//切割的物体
    GameObject slicerGo;//切片物体
    public GameObject defualt_sliceGo;
    public Material sectionMat;//切面材质
    private bool 已经被切割;
    public bool is_character;
    public AudioClip 破碎音效;
    public ParticleSystem deathParticle;
    public AnimationCurve deathGlowCurve;
    public AudioSource 最后一击音效;
    SlicedHull hull;

    public GameObject head_paint;
   public OutlineDrawer body_outline;
   public bool close_outline;

   public GameObject Blood;
   public GameObject 冷兵器砍中特效;
   public ParticleSystem 拳击打到特效;
   public ParticleSystem 钝器打到特效;
   public ParticleSystem 长矛刺到特效;
   public ParticleSystem 子弹射到特效;
   public ParticleSystem 剑技能击中特效;
   //public ParticleSystem blood;
   public AudioSource  Get_Hit;
   public Random_plays Get_hit_mount;
  
   public Enemy_circleBrith  enemy_Spawn;
    

 //循环生成
   public bool is_loop_generate;


    private Vector3 temp_directoin;
    private Collision temp_other;
    //持续刷怪的单一种类敌人
    public bool is_death_NoCall;
    //单次固定复数敌人
    public bool is_static_deathNoCALL;
     //持续刷怪的非单一种类敌人
    public bool is_death_NoCall_variate;
    float HP_PerAmount;
    private GameObject Player_eyes;
    public Hit_Reaction 敌人受击状态控制;
    private void Start() {
        HP_amount=Max_Hp;
        Player_eyes= GameObject.FindWithTag("MainCamera");
       // Hp_image.fillAmount=1;
       if(is_multi_hpBar)
       HP_PerAmount=Max_Hp/(Boss_hp_image_set.Length+1);
    }

    public void Set_hit_type(Hit_type type)
    {
        敌人受击状态控制.Set_hit_type(type);
    }


    IEnumerator interval_minusHP(float minus_amount)
    {
        is_interval=true;
        HP_amount-=minus_amount;
        yield return new WaitForSeconds(0.1f);
        is_interval=false;
    }

    IEnumerator Set_HpZero()
    {
        is_interval=true;
        HP_amount=0;
        最后一击音效.transform.SetParent(null);
        最后一击音效.Play();
        if(is_loop_generate)
        if(null!=enemy_Spawn)
        {
            enemy_Spawn.Delay_ger_Enemey();
        } 
        else
        {
            GameObject.FindWithTag("Enemy_spawn").SendMessage("Delay_ger_Enemey");;
        }
        Destroy(最后一击音效.transform.gameObject,2);
       // Invoke("POP_win",5f);
        // POP_win();
        GameObject battle_man=GameObject.FindWithTag("Battle_manager");
         print("持续生成进入AAA");
        //歼灭战模式批量生成的敌人，不会只死一个就结束
        if(is_death_NoCall||is_death_NoCall_variate)
        {
            print("持续生成进入bbb");
            if(null!=battle_man)
            {
                 print("持续生成进入ccc");
                 bool result=battle_man.GetComponent<Battle_manager>().Deal_group_enemy_Death();
                 if(result)
                 {
                    print("持续生成进入ddd");
                    GameObject player_bodyer=GameObject.FindWithTag("Player_body");
                    if(null!=player_bodyer)
                    player_bodyer.GetComponent<Player_managerV2>().Reset_playerStatus();
                 }else
                 {
                    print("持续生成进入eee");

                    if(is_death_NoCall)
                    battle_man.GetComponent<Battle_manager>().Deal_Group_Enemy_ger();
                    
                    if(is_death_NoCall_variate)
                    battle_man.GetComponent<Battle_manager>().Deal_Group_Enemy_ger_variate();
                 }
                
            }
            
        }else if(is_static_deathNoCALL)
        {
                 if(null!=battle_man)
            {
                 battle_man.GetComponent<Battle_manager>().Deal_Static_Death();
               
                
            }
        }
        else
        {//死一个敌人就结束
            if(null!=battle_man)
            battle_man.GetComponent<Battle_manager>().Battle_end("win");
            GameObject player_bodyer=GameObject.FindWithTag("Player_body");
            if(null!=player_bodyer)
            player_bodyer.GetComponent<Player_managerV2>().Reset_playerStatus();
        }

            
            
         Deal_hp_zero();
        yield return new WaitForSeconds(0.1f);
        is_interval=false;
    }

    private void POP_win()
    {
        
        GameObject tmp  =GameObject.FindWithTag("Scene_Manager");
             if(tmp!=null)
             {
                  //Scene_manager manager=tmp.GetComponent<Scene_manager>();
                //  manager.open_All_set();
             }
        
    }

//刺击反应处理
    public void Deal_character_Be_stick(Physic_weaponManager weaponManager,Collision collision,GameObject sliceGo)
    {
        print("进入刺击处理");
       // Minus_Hp(weaponManager.Runtime_Damage);
       // if(weaponManager.get_hitBack()) Deal_hitBack();
        
        weaponManager.Close_collider();
        Set_sliceGo(sliceGo);
        //ContactPoint[] test_pointArrary= other.contacts;
        

            ContactPoint[] test_pointArrary= collision.contacts;
                 Vector3 closestPoint =test_pointArrary[0].point;
           
                  Blood.transform.position =closestPoint;
                     Blood.transform.rotation = Quaternion.LookRotation(-test_pointArrary[0].normal);
                     Blood.GetComponentInChildren<ParticleSystem>().Emit(5);

                    Show_slash_effect_rotation(collision,weaponManager.Swing_direction);
                   /*  冷兵器砍中特效.transform.position=closestPoint;
                     冷兵器砍中特效.transform.LookAt(Player_eyes.transform);
                     Transform child = 冷兵器砍中特效.transform.GetChild(0);
                     child.GetComponent<ParticleSystem>().Play();*/
                     
               //      if(null!=Get_hit_mount)
                //     Get_hit_mount.random_play();
     
    }


    private void Blood_delay2_emit()
    {
                 Blood.transform.position =temp_other.contacts[0].point;
                  Blood.transform.right = temp_directoin;
                  Blood.GetComponentInChildren<ParticleSystem>().Play();
    }

    private bool slash_effet_bool=false;

    private void Show_slash_effect_rotation(Collision other,Vector3 dir)
    {
        if(slash_effet_bool) return;
        slash_effet_bool = true;
         冷兵器砍中特效.transform.position=other.contacts[0].point;
                                                  //   Transform child = 冷兵器砍中特效.transform.GetChild(0);
                                                    // child.right = other.transform.right;
                                                    // 冷兵器砍中特效.transform.LookAt(Player_eyes.transform);
                                                    //child.up = dir;
                                                    //冷兵器砍中特效.transform.forward = dir;
                                                 
                                                     // 获取摄像头的位置与特效位置的方向
                                                    // Vector3 effectToCamera = (Camera.main.transform.position - 冷兵器砍中特效.transform.position).normalized;
                                                    // 先让特效的Y轴对齐挥舞方向（横线对齐）
                                                  //  Quaternion alignYToSwing = Quaternion.FromToRotation(冷兵器砍中特效.transform.up, dir);
                                                     // 接着调整特效的forward方向对准摄像头
                                                   // 冷兵器砍中特效.transform.rotation = Quaternion.LookRotation(effectToCamera) * alignYToSwing;
                                                    //child.up = dir;
                                                    冷兵器砍中特效.transform.right = other.transform.right;
                                                    冷兵器砍中特效.GetComponent<ParticleSystem>().Play();
         Invoke("reset_slash_bool",0.2f);
    }

    private void reset_slash_bool()
    {
        slash_effet_bool = false;
    }

    //挥砍反应处理

       public void Deal_character_Be_attack(Physic_weaponManager weaponManager,Collision other,Vector3 hitDirection,bool is_BulletHit)
    { 
            //给血液喷射准备
               temp_directoin=hitDirection;
               temp_other=other;

                Set_sliceGo(other.gameObject);
                
         //    if(weaponManager.get_hitBack()) Deal_hitBack();
         //    if(weaponManager.get_hitFly()) Deal_hitFly();
                
                 //weaponManager.Close_collider();
                if(is_BulletHit)
                {
                         if(null!=子弹射到特效)
                         {
                            //攻击特效位置
                                子弹射到特效.transform.position=other.contacts[0].point;
                                子弹射到特效.transform.right = hitDirection;
                                子弹射到特效.Play();
                         }
                               
                                // 根据砍击方向调整特效的朝向  
                            //    Minus_Hp(other.transform.GetComponent<Bullet_status>().bullet_damage*headShotMuti);
                                Destroy(other.transform.gameObject);
                }
                else if(null!=weaponManager)
                      {
                        // Minus_Hp(weaponManager.Runtime_Damage);
                         if(weaponManager.Get_skillStatus())
                         {
                                                if(null!=剑技能击中特效 ){
                                                    //攻击特效位置
                                                    剑技能击中特效.transform.position=other.contacts[0].point;
                                                    剑技能击中特效.Play();
                                                    // 根据砍击方向调整特效的朝向
                                                    剑技能击中特效.transform.right = hitDirection;
                                                   // Set_hit_type(Hit_type.击退);
                                                }
                         }
                         else
                                            switch(weaponManager.Get_weaponTypeString())
                                            {
                                                case "切割":
                                                if(null!=冷兵器砍中特效){
                                                    //攻击特效位置
                                                   Show_slash_effect_rotation(other,weaponManager.Swing_direction);
                                                    // 根据砍击方向调整特效的朝向
                                                   
                                                    //冷兵器砍中特效.transform.right = -other.transform.right;
                                                    //冷兵器砍中特效.transform.up = other.transform.forward;
                                                   
                                                    
                                                }
                                                break;
                                                case "拳击手套":
                                            
                                                    拳击打到特效.transform.position=other.contacts[0].point;
                                                    拳击打到特效.Play();
                                                //  拳击打到特效.transform.right = hitDirection;
                                            
                                                break;
                                                case "钝器":
                                                if(null!=钝器打到特效){
                                                    钝器打到特效.transform.position=other.contacts[0].point;
                                                    钝器打到特效.Play();
                                                    钝器打到特效.transform.right = hitDirection;
                                                }
                                                break;
                                                case "长矛":
                                                if(null!=长矛刺到特效){
                                                    长矛刺到特效.transform.position=other.contacts[0].point;
                                                    长矛刺到特效.Play();
                                                    长矛刺到特效.transform.right = hitDirection;
                                                }
                                                break;
                                            }
                      }

                //攻击特效位置
              /*  冷兵器砍中特效.transform.position=other.contacts[0].point;
                冷兵器砍中特效.Play();
                // 根据砍击方向调整特效的朝向
                冷兵器砍中特效.transform.right = hitDirection;*/
                
                //血液应该在砍中后延迟喷出
                Invoke("Blood_delay2_emit",0.2f);
                               
                    
                  //Get_Hit.Play();
                  if(Get_hit_mount!=null)
                  Get_hit_mount.random_play();
    }

   public void Minus_Hp(float minus_amount)
   {
       if(0<HP_amount-minus_amount)
       {
         
           if(false==is_interval)
             StartCoroutine(interval_minusHP(minus_amount));
           
       }else
       {
       //   GameObject.FindWithTag("Battle_manager").GetComponent<Battle_manager>().Battle_end("win");
        //  GameObject.FindWithTag("Player_body").GetComponent<Player_managerV2>().Reset_playerStatus();
           if(false==is_interval)
             StartCoroutine(Set_HpZero());
       }

            if(is_multi_hpBar)
            {
                
                if(HP_amount>=Boss_hp_image_set.Length*HP_PerAmount)
                {
                        Hp_image.fillAmount=(HP_PerAmount-(Max_Hp-HP_amount))/HP_PerAmount;
                }else if(HP_amount>=(Boss_hp_image_set.Length-1)*HP_PerAmount)
                {
                        Hp_image.fillAmount=0;
                        Boss_hp_image_set[0].fillAmount=(HP_PerAmount-(Max_Hp-HP_PerAmount-HP_amount))/HP_PerAmount;
                }
                else if(HP_amount>=(Boss_hp_image_set.Length-2)*HP_PerAmount)
                {
                        Boss_hp_image_set[0].fillAmount=0;
                        Boss_hp_image_set[1].fillAmount=(HP_PerAmount-(Max_Hp-HP_PerAmount*2-HP_amount))/HP_PerAmount;
                }
                else 
                {
                         Boss_hp_image_set[1].fillAmount=0;
                        Boss_hp_image_set[2].fillAmount=(HP_PerAmount-(Max_Hp-HP_PerAmount*3-HP_amount))/HP_PerAmount;
                }
            }else
            {
                Hp_image.fillAmount=HP_amount/Max_Hp;
            }
        
   }

   public void Add_Hp(float add_amount)
   {
       if(HP_amount+add_amount<Max_Hp)
       {
           HP_amount+=add_amount;
       }else
       {
           HP_amount=Max_Hp;
       }
       Hp_image.fillAmount=HP_amount/Max_Hp;

   }

    public void Activate_flow()
    {
        head_paint.GetComponent<Animator>().SetTrigger("activate_flow");
        if(!close_outline)
        body_outline.enabled=true;
    }
    public void Stop_flow()
    {
//        print("进入取消");
        head_paint.GetComponent<Animator>().SetTrigger("Cancel");
        if(!close_outline)
        body_outline.enabled=false;
    }



   public void Set_sliceGo(GameObject other)
   {
        slicerGo=other;
   }


    private void comfirm_kill_mesh()
    {
        if(null!=sourceGo)
        Deal_self_broken(sourceGo);
    }

    private void Deal_hp_zero()
    {

         print("进入切割");
         if(已经被切割==false)
            {
                      已经被切割=true; 

                       Gernerate_StaticMesh Mesh_controller =GetComponent<Gernerate_StaticMesh>();
                       if(null==Mesh_controller)
                        print("Mesh_congtroller为空");
                       else 
                        sourceGo=Mesh_controller.Create_staticMesh(); //生成了静态模型

                       // Destroy(sourceGo,1.5f);
                       Invoke("comfirm_kill_mesh",1.8f);


                        if(null==sourceGo){}
                         print("sourceGo为空");


                        if(slicerGo==null)
                        {
                            slicerGo=defualt_sliceGo;
                             print("sliceGo 为空");
                        }


                        Physic_weaponManager physic_WeaponManager=slicerGo.GetComponent<Physic_weaponManager>();           
                        if(physic_WeaponManager!=null&&physic_WeaponManager.Get_weaponTypeString()!="切割")
                        {
                            slicerGo=defualt_sliceGo;
                            
                        }
                        
                    
                        hull = sourceGo.Slice(slicerGo.transform.position, slicerGo.transform.right);
                        if(hull==null)
                        {
                            print("hull为空");
                             hull = sourceGo.Slice(slicerGo.transform.position, defualt_sliceGo.transform.right);
                        }
                       
                           
                       
                        transform.gameObject.SetActive(false);
                        sourceGo.SetActive(false);
                        GameObject upper = hull.CreateUpperHull(sourceGo, sectionMat);
                        GameObject lower = hull.CreateLowerHull(sourceGo, sectionMat);
                        
                        
                        
                        


                      
                       upper.AddComponent<MeshCollider>().convex=true;
                       upper.AddComponent<Rigidbody>().AddExplosionForce( 100f, upper.transform.position,2f);
                       upper.AddComponent<AudioSource>().clip=破碎音效;

                      //  upper.layer=8;
                        
                        lower.AddComponent<MeshCollider>().convex=true;
                        lower.AddComponent<Rigidbody>().AddExplosionForce( 100f, lower.transform.position,2f);
                     //   lower.layer=8;
                      
                        Deal_self_broken(upper);
                        Deal_self_broken(lower);
                        Drop_manager drop_Manager = GetComponent<Drop_manager>();
                        if(null!=drop_Manager)
                        drop_Manager.GenerateDrops();
                        Destroy(transform.gameObject);
                        Destroy(sourceGo);
                    //   StartCoroutine(delay_fade());
                     //   sourceGo.SetActive(false);
                      //  transform.gameObject.SetActive(false);

            }
    }

        private void Deal_self_broken(GameObject goal_obj)
        {
            
            SaoDeathCtr ctr = goal_obj.AddComponent<SaoDeathCtr>();
                ctr.autoPlay = false;
                ctr.deathParticleInstance = deathParticle;
                ctr.glowCurve = deathGlowCurve;
                ctr.GridTilling = 1;
                //ctr.deathTime = 1f;
                ctr.Delay_2Death();

        }


        public void Deal_hitBack()
        {
           // GetComponent<Character_controller_debug>().Deal_hitBack();
        }

        public void Deal_hitFly()
        {
         //   GetComponent<Character_controller_debug>().Deal_hitFly();
        }




}
