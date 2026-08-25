/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class Body_hit_Reaction :  HitState_Global 
{
      private enum Weapon_type
    {
        切割,
        拳击手套,
        钝器,
        长矛
    }
   // public float Arm_point;
    public Enemy_statusController enemy_StatusController;
   // public ParticleSystem blood;
    public woosh_controller 砍到;
    public woosh_controller 拳击打到;
    public woosh_controller 长矛刺到;
    public woosh_controller 钝器打到;
    public woosh_controller 子弹射到;
    public AudioSource 子弹爆头声音;
    public AudioSource 技能打到声音;
    public GameObject hit_effect_prefab;
    public float inValid_attackSpeed=3f;
    public float HeadShot_muti=2f;
    public bool is_head;
    bool is_interVal;
    Vector3 hitDirection;
    bool is_bulletHit;
    private bool is_interval_new;
    public GameObject damageTextPrefab;  // 拖入预制件

    public void Player_bulletHit()
    {
        if(null!=子弹射到)
        {
            子弹射到.play_NotCorruptSounds();
        }
    }

    /*public void Set_hit_type(Hit_type type)
    {
        enemy_StatusController.Set_hit_type(type);
    }*/

    IEnumerator interval_hit(Collision other)
    {
        is_interVal=true;
        print("进入间隔2");
                ContactPoint[] test_pointArrary= other.contacts;
                 Vector3 closestPoint =test_pointArrary[0].point;
                 if(null!=hit_effect_prefab)
                 {
                      GameObject hit_effect=Instantiate(hit_effect_prefab,closestPoint,Quaternion.identity);
                      hit_effect.GetComponent<ParticleSystem>().Emit(5);
                      Destroy(hit_effect,1.5f);  
                      Physic_weaponManager physic_WeaponManager=other.transform.GetComponent<Physic_weaponManager>();
                      if(null!=physic_WeaponManager)
                      {
                           
                                    switch(physic_WeaponManager.Get_weaponTypeString())
                                {
                                    case "切割":                                   
                                            if(null!=砍到)
                                        砍到.play_NotCorruptSounds();                      
                                    break;
                                    case "拳击手套":
                                        拳击打到.play_NotCorruptSounds();
                                    break;
                                    case "钝器":
                                        钝器打到.play_NotCorruptSounds();
                                    break;
                                    case "长矛":
                                        长矛刺到.play_NotCorruptSounds();
                                    break;
                                }
                            
                             
                      }                   
                   //  砍到.Play();
                 }
                
        yield return new WaitForSeconds(1f);
        is_interVal=false;
    }

    /*private void OnTriggerEnter(Collider other) {
        if(other.transform.gameObject.tag=="Weapon_inHand"||other.transform.gameObject.tag=="Weapon_inWorld")
        {
            print("硬碰撞体"+other.gameObject.name);
            Physic_weaponManager weaponManager=other.transform.GetComponent<Physic_weaponManager>();
            if(weaponManager.Cur_weaponSpeed>weaponManager.held_speed)
            {
                enemy_StatusController.Deal_character_Be_attack(weaponManager,other);
                  
        
            }
              

        }
    }*/

    private void play_GetHit_sounds(Collision other)
    {
        print("打击声音AAAA");
        if(is_bulletHit&&null!=子弹射到)
        {
            if(is_head)
            子弹爆头声音.Play();
            else
            子弹射到.play_NotCorruptSounds();
            return;
        }
         print("打击声音BBBB");
        Physic_weaponManager physic_WeaponManager=other.transform.GetComponent<Physic_weaponManager>();
                      if(null!=physic_WeaponManager)
                      {
                         switch(physic_WeaponManager.Get_weaponTypeString())
                         {
                            case "切割":
                            if(null!=砍到)
                            {
                                 砍到.play_NotCorruptSounds();
                                print("打击声音CCCC");
                            }
                             
                            break;
                            case "拳击手套":
                                print("打击声音DDDD");
                                拳击打到.play_NotCorruptSounds();
                            break;
                            case "钝器":
                                钝器打到.play_NotCorruptSounds();
                            break;
                            case "长矛":
                                长矛刺到.play_NotCorruptSounds();
                            break;
                         }
                      }
    }

    private void set_interval_reset()
    {
        is_interval_new=false;
    }

    public void Deal_body_damage(float amount)
    {
        enemy_StatusController.Minus_Hp(amount);
        ShowDamage_Full(amount);
    }

    float Cur_dam=0;
    private void OnCollisionEnter(Collision other) {
        if(is_interval_new) return;
        if(other.transform.gameObject.tag=="Weapon_inHand"||other.transform.gameObject.tag=="Weapon"||other.transform.gameObject.tag=="Weapon_inRightHand"
        ||other.transform.gameObject.tag=="Player_bullet")
        {
            print("硬碰撞体"+other.gameObject.name);
            Physic_weaponManager weaponManager=other.transform.GetComponent<Physic_weaponManager>();
           
             if(null!=weaponManager)
            {
                //weaponManager.Deal_long_impusle();
                weaponManager.Deal_middle_impusle();
                
               // weaponManager.Close_collider();
            }
            // hitDirection = (other.contacts[0].point - weaponManager.pre_position).normalized;
            
            if(other.transform.gameObject.tag=="Player_bullet")
            {
                 is_bulletHit=true;
            }

            if(is_bulletHit)
            {
                hitDirection=other.transform.forward;
                is_interval_new=true;
                Invoke("set_interval_reset",0.2f);
                play_GetHit_sounds(other);
                transform.GetComponent<Rigidbody>().AddForceAtPosition(hitDirection * 18000f, other.contacts[0].point);
                if(is_head)
                {
                    enemy_StatusController.Minus_Hp(other.transform.GetComponent<Bullet_status>().bullet_damage*HeadShot_muti);
                    ShowDamage_Full(other.transform.GetComponent<Bullet_status>().bullet_damage*HeadShot_muti);
                    enemy_StatusController.Deal_character_Be_attack(weaponManager,other,hitDirection,is_bulletHit);
                }
                else
                {
                    enemy_StatusController.Minus_Hp(other.transform.GetComponent<Bullet_status>().bullet_damage);
                     ShowDamage_Weakness(other.transform.GetComponent<Bullet_status>().bullet_damage);
                     enemy_StatusController.Deal_character_Be_attack(weaponManager,other,hitDirection,is_bulletHit);
                }
               
                is_bulletHit=false;
            }        
            else if(weaponManager.Cur_weaponSpeed>inValid_attackSpeed)
            {
                is_interval_new=true;
                weaponManager.weapon_Durability.Mins_dur(1f);
                hitDirection=other.transform.right;
                Invoke("set_interval_reset",0.2f);


                //是技能，计算优先级最高
                if(weaponManager.Runtime_Damage==weaponManager.Weapon_baseDamage*weaponManager.Sill_damRate)
                {
                    Cur_dam = weaponManager.Runtime_Damage;
                    ShowDamage_Heavy(Cur_dam);
                }
                else if(weaponManager.Cur_weaponSpeed<weaponManager.普通攻击生效阈值)
                {
                    Cur_dam = weaponManager.Weapon_baseDamage/10;
                    ShowDamage_Invaild(Cur_dam);
                }
                else if(weaponManager.Cur_weaponSpeed>=weaponManager.普通攻击生效阈值&&
                weaponManager.Cur_weaponSpeed<weaponManager.Swing_sounds_activate_held)
                {
                    Cur_dam = weaponManager.Weapon_baseDamage*4f/10;
                    ShowDamage_Weakness(Cur_dam);
                }
                else if(weaponManager.Cur_weaponSpeed>=weaponManager.Swing_sounds_activate_held)
                {
                    Cur_dam = weaponManager.Weapon_baseDamage;
                    ShowDamage_Full(Cur_dam);
                }
                enemy_StatusController.Minus_Hp(Cur_dam);
              
                 enemy_StatusController.Deal_character_Be_attack(weaponManager,other,hitDirection,is_bulletHit);
                 weaponManager.Close_collider();
                play_GetHit_sounds(other);
                transform.GetComponent<Rigidbody>().AddForceAtPosition(hitDirection * 18000f, other.contacts[0].point);
                is_bulletHit=false;
                  
            }

            
            
        }
    }

    private void Face_player(GameObject text)
    {
        GameObject player = GameObject.FindWithTag("Player_body");
        Vector3 targerVec3 = new Vector3(player.transform.position.x,transform.position.y,player.transform.position.z);
        text.transform.LookAt(targerVec3);
    }

    public void ShowDamage_Invaild(float damage)
    {
        // 实例化伤害文本预制件
        GameObject damageTextInstance = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
        Face_player(damageTextInstance);
        // 获取文本组件
        TextMeshProUGUI damageText = damageTextInstance.GetComponentInChildren<TextMeshProUGUI>();
        damageText.text = damage.ToString();
        
                damageText.color = Color.gray;
                damageText.fontSize = 20;  // 普通大小
        
        // 让文本漂浮和下落
        StartCoroutine(FloatAndDrop(damageTextInstance));
    }

     // 创建伤害数字显示效果
    public void ShowDamage_Weakness(float damage)
    {
        // 实例化伤害文本预制件
        GameObject damageTextInstance = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
        Face_player(damageTextInstance);
        // 获取文本组件
        TextMeshProUGUI damageText = damageTextInstance.GetComponentInChildren<TextMeshProUGUI>();
        damageText.text = damage.ToString();
        
                damageText.color = Color.white;
                damageText.fontSize = 20;  // 普通大小
        
        // 让文本漂浮和下落
        StartCoroutine(FloatAndDrop(damageTextInstance));
    }
    public void ShowDamage_Full(float damage)
    {
        // 实例化伤害文本预制件
        GameObject damageTextInstance = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
        Face_player(damageTextInstance);
        // 获取文本组件
        TextMeshProUGUI damageText = damageTextInstance.GetComponentInChildren<TextMeshProUGUI>();
        damageText.text = damage.ToString();

                damageText.color = Color.red ;  // 白色
                damageText.fontSize = 32;  // 稍大

        // 让文本漂浮和下落
        StartCoroutine(FloatAndDrop(damageTextInstance));
    }
    public void ShowDamage_Heavy(float damage)
    {
        // 实例化伤害文本预制件
        GameObject damageTextInstance = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
        Face_player(damageTextInstance);
        // 获取文本组件
        TextMeshProUGUI damageText = damageTextInstance.GetComponentInChildren<TextMeshProUGUI>();
        damageText.text = damage.ToString();
          
                damageText.color = Color.yellow;
                damageText.fontSize = 60;  // 最大
             
        // 让文本漂浮和下落
        StartCoroutine(FloatAndDrop(damageTextInstance));
    }

     // 漂浮并下落、淡出和销毁伤害数字的协程
    private IEnumerator FloatAndDrop(GameObject damageTextInstance)
    {
        float floatUpTime = 0.5f;  // 上升时间
        float dropTime = 0.7f;     // 下落时间
        float totalTime = floatUpTime + dropTime;

        float elapsedTime = 0f;

        // 获取文本组件
        TextMeshProUGUI text = damageTextInstance.GetComponentInChildren<TextMeshProUGUI>();
        Color originalColor = text.color;

        Vector3 originalPosition = damageTextInstance.transform.position;

        // 定义上升高度和下降幅度
        float floatHeight = 1f;    // 上升高度
        float dropHeight = 0.5f;   // 下落的最大高度

        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / totalTime;

            // 处理上升阶段
            if (elapsedTime <= floatUpTime)
            {
                float tFloat = elapsedTime / floatUpTime;
                damageTextInstance.transform.position = originalPosition + new Vector3(0, Mathf.Lerp(0, floatHeight, tFloat), 0);
            }
            // 处理下落阶段
            else
            {
                float tDrop = (elapsedTime - floatUpTime) / dropTime;
                damageTextInstance.transform.position = originalPosition + new Vector3(0, Mathf.Lerp(floatHeight, floatHeight - dropHeight, tDrop), 0);
            }

            // 渐变消失
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1, 0, t));

            yield return null;
        }

        // 完全消失后销毁对象
       // Destroy(damageTextInstance);
    }


}
