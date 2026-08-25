/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能与魔法系统 / Skill and magic system
 * 功能 / Purpose: 管理玩家技能、魔法弹体、范围攻击、命中爆炸、护盾和提示表现。
 * English: Manages player skills, magic projectiles, area attacks, hit explosions, shields, and hint presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_hint : MonoBehaviour
{
   public Transform Gernerate_Transform;
    public GameObject 直线提示大剑气;
    public GameObject 圆形提示大;
    public GameObject 圆柱提示紫色;
    public GameObject 圆形提示小;
    public GameObject 直线提示小;
    public GameObject 圆柱提示蓝色;
    public GameObject 扇形提示小;
    private GameObject Hint;
    public AudioSource skiil_hillSounds;
    
  Material Weapon_Material;
  public  Renderer weapon_render;

  Material Weapon_Material2;
  public  Renderer weapon_render2;

  public ParticleSystem hill_effect;
  private void Start() {
    if(null!=weapon_render)
    Weapon_Material=weapon_render.material;

    if(null!=weapon_render2)
    {
        Weapon_Material2=weapon_render2.material;
    }

  }

//仅触发蓄力效果
  public void Just_ready()
  {
             if(null!=Weapon_Material)
             Weapon_Material.EnableKeyword("_EMISSION");
            
             if(null!=Weapon_Material2)
             {
                Weapon_Material2.EnableKeyword("_EMISSION");
             }

             if(null!=hill_effect)
             hill_effect.Play();
             skiil_hillSounds.Play();
  }

  public void Just_closeLight()
  {
        if(null!=Weapon_Material)
             Weapon_Material.DisableKeyword("_EMISSION");
            
             if(null!=Weapon_Material2)
             {
                Weapon_Material2.DisableKeyword("_EMISSION");
             }
  }
    
    public void 生成直线提示大()
    {
        if(null!=直线提示大剑气)
        {
            GameObject hint=Instantiate(直线提示大剑气,Gernerate_Transform.position,Gernerate_Transform.rotation);
             //Destroy(hint,3f);
             Hint=hint;
             if(null!=Weapon_Material)
             Weapon_Material.EnableKeyword("_EMISSION");
            
             if(null!=Weapon_Material2)
             {
                Weapon_Material2.EnableKeyword("_EMISSION");
             }

             if(null!=hill_effect)
             hill_effect.Play();
             skiil_hillSounds.Play();
          
        }
        
    }

     public void 生成圆形提示大()
    {
        if(null!=圆形提示大)
        {
            GameObject hint=Instantiate(圆形提示大,Gernerate_Transform.position,Gernerate_Transform.rotation);
             //Destroy(hint,3f);
             Hint=hint;
              if(null!=Weapon_Material)
             Weapon_Material.EnableKeyword("_EMISSION");
            
             if(null!=Weapon_Material2)
             {
                Weapon_Material2.EnableKeyword("_EMISSION");
             }

              if(null!=hill_effect)
             hill_effect.Play();
             skiil_hillSounds.Play();
        
        }
        
    }

     public void 生成圆柱提示紫色()
    {
        if(null!=圆柱提示紫色)
        {
            GameObject hint=Instantiate(圆柱提示紫色,Gernerate_Transform.position,Gernerate_Transform.rotation);
             //Destroy(hint,3f);
             Hint=hint;
              if(null!=Weapon_Material)
             Weapon_Material.EnableKeyword("_EMISSION");
            
             if(null!=Weapon_Material2)
             {
                Weapon_Material2.EnableKeyword("_EMISSION");
             }
              if(null!=hill_effect)
             hill_effect.Play();
             skiil_hillSounds.Play();
            
        }
        
    }

     public void 生成圆形提示小()
    {
        if(null!=圆形提示小)
        {
            GameObject hint=Instantiate(圆形提示小,Gernerate_Transform.position,Quaternion.identity);
             //Destroy(hint,3f);
             Hint=hint;
              if(null!=Weapon_Material)
             Weapon_Material.EnableKeyword("_EMISSION");
            
             if(null!=Weapon_Material2)
             {
                Weapon_Material2.EnableKeyword("_EMISSION");
             }
              if(null!=hill_effect)
             hill_effect.Play();
             skiil_hillSounds.Play();
             
        }
        
    }

     public void 生成直线提示小()
    {
        if(null!=直线提示小)
        {
            GameObject hint=Instantiate(直线提示小,Gernerate_Transform.position,Gernerate_Transform.rotation);
             //Destroy(hint,3f);
             Hint=hint;
             if(null!=Weapon_Material)
             Weapon_Material.EnableKeyword("_EMISSION");
            
             if(null!=Weapon_Material2)
             {
                Weapon_Material2.EnableKeyword("_EMISSION");
             }
              if(null!=hill_effect)
             hill_effect.Play();
             skiil_hillSounds.Play();
          
        }
        
    }

     public void 生成圆柱提示蓝色()
    {
        if(null!=圆柱提示蓝色)
        {
            GameObject hint=Instantiate(圆柱提示蓝色,Gernerate_Transform.position,Gernerate_Transform.rotation);
             //Destroy(hint,3f);
             Hint=hint;
              if(null!=Weapon_Material)
             Weapon_Material.EnableKeyword("_EMISSION");
            
             if(null!=Weapon_Material2)
             {
                Weapon_Material2.EnableKeyword("_EMISSION");
             }
              if(null!=hill_effect)
             hill_effect.Play();
             skiil_hillSounds.Play();
           
        }
        
    }

     public void 生成扇形提示小()
    {
        if(null!=扇形提示小)
        {
            GameObject hint=Instantiate(扇形提示小,Gernerate_Transform.position,Gernerate_Transform.rotation);
             //Destroy(hint,3f);
             Hint=hint;
              if(null!=Weapon_Material)
             Weapon_Material.EnableKeyword("_EMISSION");
            
             if(null!=Weapon_Material2)
             {
                Weapon_Material2.EnableKeyword("_EMISSION");
             }
              if(null!=hill_effect)
             hill_effect.Play();
             skiil_hillSounds.Play();
              
        }
        
    }

  

    public void Destroy_hint()
    {
        if(null!=Hint)
        {
            Destroy(Hint);
        }
        if(null!=Weapon_Material)
        Weapon_Material.DisableKeyword("_EMISSION");

        if(null!=Weapon_Material2)
        Weapon_Material2.DisableKeyword("_EMISSION");

    }
}
