/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 物理武器系统 / Physical weapon system
 * 功能 / Purpose: 管理抓取、释放、刚体添加、盾牌阻挡、武器碰撞和轨迹检测。
 * English: Manages grabbing, releasing, Rigidbody setup, shield blocking, weapon collision, and trace detection.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;



public class Physic_weaponManager : MonoBehaviour
{
    public enum Weapon_type
    {
        切割,
        拳击手套,
        钝器,
        长矛
    }

    public enum 物品类型
    {
        剑,
        手枪,
        步枪,
        盾牌
    }

     private bool Weapon_inHand;
    public Transform WeaponSpeed_detectPoint;
    //public float held_speed=8;
    public Vector3 pre_position;
    public float Cur_weaponSpeed;
    [HideInInspector]
    public Vector3 Swing_direction;
    public Collider Sword_bodyCollider;
    //public GameObject head_collider;
    public AudioSource Swing_sounds;
    [Range(1,10)]
    public float Swing_sounds_activate_held=3f;
    public bool Weapon_isInHand;
    bool swingSound_IsPlaying;
    //Skill+++++++++++++++++++++++++++++++
    public bool  have_skill=true;
    public ParticleSystem Held_effect;
    public GameObject[] Skill_effect;
    Material Weapon_Material;
    public  Renderer weapon_render;
    public GameObject 剑技胶囊;
    public GameObject 剑技胶囊2;
    public AudioSource Skill_sounds;
    public AudioSource Skill_hit;
    public AudioSource skill_end;
     bool skill_isReady;
    public bool is_skill_interval;
    public float skill_stamina_comsume=20f;
    public float Skill_CD=15f;


    public float Weapon_baseDamage=10;
    [HideInInspector]
    public float Runtime_Damage;
     [Range(1,10)]
    public float Sill_damRate= 3f;
    public bool is_gun_mode;
    [Range(1,20)]
    public float 技能持续时间=7f;

    public 物品类型 此物品的类型=物品类型.剑;

    public Weapon_type weapon_Type=Weapon_type.切割;
    private String weapon_typeString="切割";

    private Player_managerV2 Player;

   
     GameObject Motion_player ;

    public GameObject 划破空气特效;
    public Transform  slash_trans;

    public Weapon_durability weapon_Durability;
    public GameObject 普通白色拖尾;
    //public GameObject 重击红色拖尾;
    public woosh_controller 初级挥舞声音控制;
    public float 普通攻击生效阈值=3f;
    public bool is_keepSkill=false;

    public Player_managerV2 Get_Player()
    {
        return Player;
    }

    public string get_itemType()
    {
        string res = "剑";
        switch(此物品的类型)
        {
             case 物品类型.剑:
             res="剑";
             break;
             case 物品类型.手枪:
             res="手枪";
             break;
             case 物品类型.步枪:
             res="步枪";
             break;
             case 物品类型.盾牌:
             res="盾牌";
             break;
        }
        return res;
    }

    private void Reset_Damage()
    {
        Runtime_Damage = Runtime_Damage=Weapon_baseDamage;
    }

 //HVRController controller;
 // HVRController RightController => HVRInputManager.Instance.RightController;

 /* private bool checkinput()
        {
           if (RightController.ControllerType == HVRControllerType.Vive)
            {
                return RightController.TrackPadUp.JustActivated;
            }

            if (RightController.ControllerType == HVRControllerType.WMR)
            {
                return RightController.TrackPadDown.JustActivated;
            }

            return RightController.TriggerButtonState.JustActivated;
        }*/

    public String Get_weaponTypeString()
    {
        return weapon_typeString;
    }       

    
    void Start()
    {
        Motion_player = GameObject.FindWithTag("Player");
        Player = GameObject.FindWithTag("Player_body").GetComponent<Player_managerV2>();
        Runtime_Damage=Weapon_baseDamage;
        pre_position=WeaponSpeed_detectPoint.transform.position;
        Weapon_Material=weapon_render.material;
        switch(weapon_Type)
        {
            case Weapon_type.切割:
            weapon_typeString="切割";
            break;
            case Weapon_type.拳击手套:
            weapon_typeString="拳击手套";
            break;
            case Weapon_type.钝器:
            weapon_typeString="钝器";
            break;
            case Weapon_type.长矛:
            weapon_typeString="长矛";
            break;
        }
    }

    IEnumerator Skill_CdTime()
    {
       // Player.skill_Display.Set_DeActivate();
       if(transform.gameObject.tag=="Weapon_inHand")
        Player.skill_LeftDisplay.Start_Cd(Skill_CD);
        else if(transform.gameObject.tag=="Weapon_inRightHand")
        Player.skill_RightDisplay.Start_Cd(Skill_CD);
        yield return new WaitForSeconds(Skill_CD);
        //skill_isReady=false;
        if(transform.gameObject.tag=="Weapon_inHand")
        Player.skill_LeftDisplay.Set_SkillReady();
        else if(transform.gameObject.tag=="Weapon_inRightHand")
        Player.skill_RightDisplay.Set_SkillReady();
        
        is_skill_interval=false;
    }

    private void Set_SkillDam()
    {
        Runtime_Damage=Weapon_baseDamage*Sill_damRate;
    }

    private void Reset_SkillDam()
    {
        Runtime_Damage=Weapon_baseDamage;
    }


    public void Close_collider()
    {
            Sword_bodyCollider.enabled=false;
            
          //  head_collider.SetActive(false);
            //  Held_effect.Play();
            if(!is_skill_interval&&skill_isReady&&!is_keepSkill)
            {
                
                
                        is_skill_interval=true;
                        skill_isReady=false;
                        StopCoroutine("Stop_skill");
                        if(剑技胶囊!=null)
                        剑技胶囊.GetComponent<Animator>().SetTrigger("CloseSkill");
                        if(剑技胶囊2!=null)
                        剑技胶囊2.SetActive(false);
                        Reset_SkillDam();
                        if(Skill_effect.Length!=0)
                        Skill_effect[0].SetActive(false);
                        Weapon_Material.DisableKeyword("_EMISSION");
                        if(Skill_sounds.isPlaying) Skill_sounds.Stop();
                        if(!Skill_hit.isPlaying) Skill_hit.Play();
                        StartCoroutine(Skill_CdTime());
                
              
                
                
            }
            
            Invoke("open_collider",0.3f);
    }

    public void Deal_short_impusle()
    {
        if(transform.gameObject.tag=="Weapon_inHand")
        OVRInput.SetControllerVibration(1f,1f,OVRInput.Controller.LTouch);
        else 
        OVRInput.SetControllerVibration(1f,1f,OVRInput.Controller.RTouch);
            Invoke("close_controller_impulse",0.25f);
    }

    public void Deal_middle_impusle()
    {
             if(transform.gameObject.tag=="Weapon_inHand")
            OVRInput.SetControllerVibration(1f,1f,OVRInput.Controller.LTouch);
            else 
            OVRInput.SetControllerVibration(1f,1f,OVRInput.Controller.RTouch);
            Invoke("close_controller_impulse",0.7f);
    }

    public void Deal_long_impusle()
    {
            if(transform.gameObject.tag=="Weapon_inHand")
            OVRInput.SetControllerVibration(1f,1f,OVRInput.Controller.LTouch);
            else 
            OVRInput.SetControllerVibration(1f,1f,OVRInput.Controller.RTouch);
            Invoke("close_controller_impulse",1.2f);
    }

    private void close_controller_impulse()
    {
        if(transform.gameObject.tag=="Weapon_inHand")
            OVRInput.SetControllerVibration(0f,0f,OVRInput.Controller.LTouch);
            else 
            OVRInput.SetControllerVibration(0f,0f,OVRInput.Controller.RTouch);

    }

    public void open_collider()
    {
        Sword_bodyCollider.enabled=true;
       // head_collider.SetActive(true);
    }

    
    IEnumerator Interval_playBgm()
    {
        swingSound_IsPlaying=true;
        Swing_sounds.Play();


        yield return new WaitForSeconds(0.2f);

       
        swingSound_IsPlaying=false;

    }

    IEnumerator   Stop_skill()
    {

                    yield return new WaitForSeconds(技能持续时间);
                    skill_isReady=false;
                    is_skill_interval=true;
                    Reset_SkillDam();
                    skill_end.Play();
                    if(剑技胶囊!=null)
                    剑技胶囊.GetComponent<Animator>().SetTrigger("CloseSkill");
                    if(剑技胶囊2!=null)
                    剑技胶囊2.SetActive(false);
                   // if(!Held_effect.isPlaying) Held_effect.Play();
                 //  if(!Skill_hit.isPlaying) Skill_hit.Play();
                     if(Skill_effect.Length!=0)
                    Skill_effect[0].SetActive(false);
                    Weapon_Material.DisableKeyword("_EMISSION");
                    StartCoroutine(Skill_CdTime());
                   // if(!Skill_sounds.isPlaying) Skill_sounds.Play();
    }

    public bool Get_skillStatus()
    {
       
        return skill_isReady;
    }

    //public ParticleSystem Held_effect;
  //  public GameObject[] Skill_effect;
  // Material Weapon_Material;
 // public  Renderer weapon_render;
   // public AudioSource Skill_sounds;

   private void Calculate_Speed()
    {
                  Vector3 localGun_MovingPoint = Motion_player.transform.InverseTransformPoint(WeaponSpeed_detectPoint.position);
                // 计算从 baseTransform 到 localPositionOfMovingPoint 的距离
                 float distance = Vector3.Distance(pre_position, localGun_MovingPoint);
                 Swing_direction = WeaponSpeed_detectPoint.position - pre_position;
              //   print("相对位移距离为："+distance);
                 Cur_weaponSpeed = distance/0.02f;
                 pre_position = localGun_MovingPoint;
                 // Cur_weaponSpeed=(WeaponSpeed_detectPoint.transform.position.y-pre_position.y)/0.02f;
    }

    private bool Check_isHeavySwing()
    {
        return (null!=Swing_sounds&&!swingSound_IsPlaying&&Swing_sounds_activate_held<Cur_weaponSpeed&&Player.Minus_HeavySwing())?true:false;
    }

    private void Set_FullDamage()
    {
        Runtime_Damage = Weapon_baseDamage;
    }

    private void CloseAll_trail()
    {
        //if(重击红色拖尾.activeSelf)
          //      重击红色拖尾.SetActive(false);
        if(普通白色拖尾.activeSelf)
                普通白色拖尾.SetActive(false);
    }

    private void Swing_while_trail()
    {
        if(null==普通白色拖尾) return;
        if(Cur_weaponSpeed>普通攻击生效阈值&&Cur_weaponSpeed<=Swing_sounds_activate_held)
        {
                if(初级挥舞声音控制!=null)初级挥舞声音控制.Play_oneShot();
               // CloseAll_trail();
                if(!普通白色拖尾.activeSelf)
                普通白色拖尾.SetActive(true);
        }else
        {
                CloseAll_trail();
        }
    }



    private bool is_slashEffect_CD;
    private void Reset_slashEffect_CD()
    {
        print("挥舞检测9");
        is_slashEffect_CD = false;
    }
    public void Ger_slash_effect()
    {
            is_slashEffect_CD = true;
            print("挥舞检测8");
            Instantiate(划破空气特效,slash_trans.position,slash_trans.rotation);
            Invoke("Reset_slashEffect_CD",0.5f);
    }

    private void Deal_slash_startDetect()
    {
        print("挥舞检测1");
        if(划破空气特效==null) return;
        print("挥舞检测2");
        if(is_slashEffect_CD) return;
        print("挥舞检测3");
       // bool res = GetComponent<SwordSlash_poseDection>().Get_dir();
       // if(!res) return;
       // print("挥舞检测4");
        GetComponent<SwordSlash_poseDection>().Start_slash_Detect();

    }

    public void Ger_slash_effect_immediate()
    {
           if(null!=划破空气特效)
            Instantiate(划破空气特效,slash_trans.position,slash_trans.rotation);
            
    }


    private void LateUpdate() {
            if(transform.gameObject.tag=="Weapon_inHand"||transform.gameObject.tag=="Weapon_inRightHand")
         {
               //  Cur_weaponSpeed=(WeaponSpeed_detectPoint.transform.position-pre_position).magnitude/0.02f;
                 //pre_position=WeaponSpeed_detectPoint.position;
                 Calculate_Speed();
                 Swing_while_trail();

                  if(Check_isHeavySwing())
                 {
                    //进行重攻击剑光是否生成检测
                   Ger_slash_effect_immediate();
                    // Ger_slash_effect();
                    //满足全额伤害，需要用例挥舞才能发挥真正的伤害
                     //Set_FullDamage();
                     //持续1秒后重置
                     //Invoke("Reset_Damage",1f);
                     //处理重攻击声音
                     StartCoroutine(Interval_playBgm());
                 }

                 if(have_skill)
                 if(!is_gun_mode&&!skill_isReady&&!is_skill_interval)
                {
                    //右手技能触发
                    if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)&&
                     transform.gameObject.tag=="Weapon_inRightHand"&&Player.Minus_Skill(skill_stamina_comsume))
                    {
                               //     if(Player.skill_RightDisplay.isSkill_ready())
                              //  {
                                    skill_isReady=true;
                                    Player.skill_RightDisplay.Set_skillActivate(技能持续时间);
                                    Set_SkillDam();
                                    if(null!=剑技胶囊)
                                    剑技胶囊.GetComponent<Animator>().SetTrigger("ActivateSkill");
                                    if(null!=剑技胶囊2)
                                    剑技胶囊2.SetActive(true);
                                    if(!Held_effect.isPlaying) Held_effect.Play();
                                    if(Skill_effect.Length!=0)
                                    Skill_effect[0].SetActive(true);
                                    Weapon_Material.EnableKeyword("_EMISSION");
                                    if(!Skill_sounds.isPlaying) Skill_sounds.Play();
                                // Invoke("Stop_skill",技能持续时间);
                                    StartCoroutine("Stop_skill");
                            //    }
                             //   else print("技能CD显示出错或未准备好");

                    }else if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)
                 &&transform.gameObject.tag=="Weapon_inHand"&&Player.Minus_Skill(skill_stamina_comsume))//左手技能触发
                    {
                              //  if(Player.skill_LeftDisplay.isSkill_ready())
                                 //       {
                                            skill_isReady=true;
                                            Player.skill_LeftDisplay.Set_skillActivate(技能持续时间);
                                            Set_SkillDam();
                                            if(null!=剑技胶囊)
                                            剑技胶囊.GetComponent<Animator>().SetTrigger("ActivateSkill");
                                            if(剑技胶囊2!=null)
                                            剑技胶囊2.SetActive(true);
                                            if(!Held_effect.isPlaying) Held_effect.Play();
                                            if(Skill_effect.Length!=0)
                                            Skill_effect[0].SetActive(true);
                                            Weapon_Material.EnableKeyword("_EMISSION");
                                            if(!Skill_sounds.isPlaying) Skill_sounds.Play();
                                        // Invoke("Stop_skill",技能持续时间);
                                            StartCoroutine("Stop_skill");
                                     //   }
                                      //  else print("技能CD显示出错或未准备好");
                    }
                        
                    
                    
                }
                //pre_position= Motion_player.transform.InverseTransformPoint(WeaponSpeed_detectPoint.position);
                 
         }
    }

     /*private void FixedUpdate() 
     {
         
         
   
       
    }*/

    
    
}
