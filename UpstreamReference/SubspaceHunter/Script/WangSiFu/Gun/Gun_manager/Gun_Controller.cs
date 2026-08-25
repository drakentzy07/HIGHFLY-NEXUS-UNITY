/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 枪械控制 / Gun control
 * 功能 / Purpose: 管理玩家或敌人的枪械射击、弹道、动画和输入响应。
 * English: Manages player or enemy firearm shooting, ballistics, animation, and input response.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;
using TMPro;
using UnityEngine.UI;


public class Gun_Controller : MonoBehaviour
{
   public enum Gun_type
   {
      Reload,
      Hill
   }
   public GameObject Bullet;
   public float Max_bulletCount=30f;
   public float Bullet_speed=200f;
   public float Bullet_destroyTime;
   public Transform Bullet_Ger_Point;
   public ParticleGroupPlayer[] particleGroupPlayers;
   public ParticleGroupEmitter[] ParticleGroupEmitters;
   public ParticleSystem[] particleSystems;
   public Gun_type gun_Type=Gun_type.Reload;
   public TextMeshProUGUI 子弹数量显示;
   public Animator 枪械动画控制;

   public woosh_controller ShootSounds_controller;
   public float shell_dropTime=1.3f;
   public woosh_controller Shell_dropSounds;
   public AudioSource[] 子弹空了音效;
   public AudioSource 上弹匣音效;
   public AudioSource 拉枪栓音效;
   public bool is_need2_pullBar;
   public bool is_keepShoot;
   private float Cur_bulletCount;
   private bool is_interval_new;
   private bool is_Pull_bar=true;

   public GameObject Gun_parent;


   public Transform WeaponSpeed_detectPoint;
   private  Vector3 pre_position;
   
    private float Cur_weaponSpeed;
   [Range(0.01f,10)]
   public float reload_speedActivate=5;
   private bool is_reloading;

    public float Reload_Time=1.5f;
    public GameObject Reload_effect;
    public GameObject Gun_model;
    private float reload_passTime;
    public Image reload_bar;
    public bool 是否动画控制子弹激发;
    public Animator Gun_animator;
    public bool 单次换多个弹药;
    public ParticleGroupEmitter 弹壳;
    private  GameObject player;

    public Weapon_durability weapon_Durability;

    

    private void LateUpdate() {
        if(Gun_parent.tag=="Weapon_inHand"||Gun_parent.tag=="Weapon_inRightHand")
        {
               if(is_reloading) return;
                
                Calculate_Speed();
                 if(reload_speedActivate<Cur_weaponSpeed&&Cur_bulletCount<Max_bulletCount)
                 弹匣重装();

                 if(reload_speedActivate<Cur_weaponSpeed&&OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch)&&
                 Gun_parent.tag=="Weapon_inRightHand")
                 弹匣重装();
                 if(reload_speedActivate<Cur_weaponSpeed&&OVRInput.Get(OVRInput.Button.Four, OVRInput.Controller.LTouch)
                 &&Gun_parent.tag=="Weapon_inHand")
                 弹匣重装();
                 pre_position= player.transform.InverseTransformPoint(WeaponSpeed_detectPoint.position);
        }
                 
    }

    private void Calculate_Speed()
    {
                  Vector3 localGun_MovingPoint = player.transform.InverseTransformPoint(WeaponSpeed_detectPoint.position);
                // 计算从 baseTransform 到 localPositionOfMovingPoint 的距离
                 float distance = Vector3.Distance(pre_position, localGun_MovingPoint);
             //    print("相对位移距离为："+distance);
                 Cur_weaponSpeed = distance/0.02f;
                 // Cur_weaponSpeed=(WeaponSpeed_detectPoint.transform.position.y-pre_position.y)/0.02f;
    }

    private void FixedUpdate() {
        if(is_reloading)
        {
            reload_passTime+=0.02f;
            reload_bar.fillAmount=reload_passTime/Reload_Time;
        }
    }

    private void Start() {
         Cur_bulletCount=Max_bulletCount;
        子弹数量显示.text=Cur_bulletCount+"/"+Max_bulletCount;
        player = GameObject.FindWithTag("Player");
        pre_position=  player.transform.InverseTransformPoint(WeaponSpeed_detectPoint.position);
        
      
    }

   private void Update() {
     
     if(Input.GetKeyDown(KeyCode.E))
     {
            点射();
     }
     if(Input.GetKey(KeyCode.W))
     {
            点射();
     }
      if(Input.GetKey(KeyCode.R))
     {
            弹匣重装();
     }
     if(Input.GetKey(KeyCode.T))
     {
           拉枪栓();
     }
     
         if(!is_keepShoot)
        {  
        
          //左右手触发
            if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)&&Gun_parent.tag=="Weapon_inRightHand"
            ||OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)&&Gun_parent.tag=="Weapon_inHand")
            {
                 if(!是否动画控制子弹激发) 点射();
                 else 动画点射();
            }
            
        }

         if(is_keepShoot)
        {  
            //左右手触发
            if(OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)&&Gun_parent.tag=="Weapon_inRightHand"
            ||OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)&&Gun_parent.tag=="Weapon_inHand")
            {
                if(!是否动画控制子弹激发) 点射();
                 else 动画点射();
            }
        }
     
       
       
   }

   private void reset_interval()
   {
      is_interval_new=false;
   }

    private void Play_shellDropSounds()
    {
        Shell_dropSounds.play_NotCorruptSounds();
    }

    public void 动画射出结束()
    {
        is_interval_new=false;
    }

    public void 动画子弹射出()
    {
       
         Activate_shoot_effect(); //开枪效果
         ShootSounds_controller.play_NotCorruptSounds();//开枪声音
         if(null!=Shell_dropSounds)
         Invoke("Play_shellDropSounds",shell_dropTime);

      

         GameObject temp_bullet=Instantiate(Bullet,Bullet_Ger_Point.position,Bullet_Ger_Point.rotation);
         temp_bullet.GetComponent<Rigidbody>().AddForce(Bullet_Ger_Point.forward*Bullet_speed,ForceMode.Impulse);
         Destroy(temp_bullet,Bullet_destroyTime);

         Cur_bulletCount--;
         if(Cur_bulletCount==0) if(is_need2_pullBar) is_Pull_bar=false;
         Refesh_bulletCountText();
    }

    private void 动画点射()
    {
        if(is_reloading) return;
        //射击间隔
        if(is_interval_new) return ;
        if(Cur_bulletCount==0) { Bullet_NonSounds();  return;}
        if(!is_Pull_bar) { Bullet_NonSounds();  return;}//拉枪栓
        bool dur_result =weapon_Durability.Mins_dur(1f);
        if(!dur_result) return;
        is_interval_new=true;
    

        //是否有子弹
        

        Gun_animator.SetTrigger("Fire");
    }

    public void 点射()
    {
        //换弹停止射击
        if(is_reloading) return;

        //射击间隔
        if(is_interval_new) return ;
        //是否有子弹
        if(Cur_bulletCount==0) { Bullet_NonSounds();  return;}
        if(!is_Pull_bar) { Bullet_NonSounds();  return;}//拉枪栓
        bool dur_result =weapon_Durability.Mins_dur(1f);
        if(!dur_result) return;
        is_interval_new=true;
        Invoke("reset_interval",0.1f);

         
         Activate_shoot_effect(); //开枪效果
         ShootSounds_controller.play_NotCorruptSounds();//开枪声音
         if(null!=Shell_dropSounds)
         Invoke("Play_shellDropSounds",shell_dropTime);

        

         GameObject temp_bullet=Instantiate(Bullet,Bullet_Ger_Point.position,Bullet_Ger_Point.rotation);
         temp_bullet.GetComponent<Rigidbody>().AddForce(Bullet_Ger_Point.forward*Bullet_speed,ForceMode.Impulse);
         Destroy(temp_bullet,Bullet_destroyTime);

         Cur_bulletCount--;
         if(Cur_bulletCount==0) if(is_need2_pullBar) is_Pull_bar=false;
         Refesh_bulletCountText();
    }


    public void 弹匣重装()
    {
       if(is_reloading) return;
       is_reloading=true;
       上弹匣音效.Play();
       if(null!=Reload_effect) {Reload_effect.SetActive(true); Gun_model.SetActive(false); 
       if( 单次换多个弹药 )substance_shell();}
       Invoke("Reload_complete",Reload_Time);
      
    }   

    public void 停止装弹()
    {
        
       上弹匣音效.Stop();
       if(null!=Reload_effect) {Reload_effect.SetActive(false); Gun_model.SetActive(true);}
       reload_passTime=0;
       reload_bar.fillAmount=0;
       is_reloading=false;
      // Invoke("Reload_complete",Reload_Time);
    }

    private void Reload_complete()
    {
        if(!is_reloading) return;
        Cur_bulletCount=Max_bulletCount;
        Refesh_bulletCountText();
        拉枪栓音效.Play();
          if(null!=Reload_effect) {Reload_effect.SetActive(false); Gun_model.SetActive(true);}
         reload_passTime=0;
         reload_bar.fillAmount=0;
         is_reloading=false;
    }

    public void 拉枪栓()
    {
        is_Pull_bar=true;
        拉枪栓音效.Play();
    }

    private void Bullet_NonSounds()
    {
        foreach(var child in 子弹空了音效)
        {
           
             child.Play();
        }
    }

    private void Refesh_bulletCountText()
    {
        子弹数量显示.text = Cur_bulletCount + "/" + Max_bulletCount;
    }

    public void substance_shell()
    {
        if(null!= 弹壳)
        弹壳.Emit(6);
    
    }

    private void Activate_shoot_effect()
    {
       /* foreach(var emit in ParticleGroupEmitters)
        {
            if(null!=emit)
            {
                emit.Emit(1);
            }
        }*/
        
         foreach(var parti in ParticleGroupEmitters)
        {
            if(null!=parti)
            parti.Emit(1);
        }
        foreach(var parti in particleSystems)
        {
            if(null!=parti)
            parti.Play();
        }
        foreach(var play in particleGroupPlayers)
        {
            if(null!=play)
            play.Play();
        }
    }


}
