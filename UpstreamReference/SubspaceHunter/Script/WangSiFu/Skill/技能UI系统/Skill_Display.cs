/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能 UI / Skill UI
 * 功能 / Purpose: 展示技能状态、图标、冷却或选择反馈。
 * English: Displays skill state, icons, cooldowns, or selection feedback.
 */

using System.Collections;
using System.Collections.Generic;
using CurvedUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Skill_Display : MonoBehaviour
{
    public enum Skill_Status
    {
        就绪,
        技能持续中,
        技能恢复中
    }
    private Skill_Status 技能状态=Skill_Status.就绪;
    public TextMeshProUGUI Skill_cdText;
    public Image CD_cycle;
    private bool is_start2_CD;
    private bool is_Keep_skill;
    private float CD_Time;
    private float Skill_Time;
    private float Cur_Time;
    private bool is_AnimeLoading;

    public Skill_Display  LR对称显示; //用于左右手切换的时候继承原先的显示状态

    public void Reset_AndReady()
    {
         Skill_cdText.SetText(" ");
        CD_cycle.fillAmount=1f;
        CD_cycle.color = Color.green;
        Cur_Time=0;
        CD_Time=0;
        is_start2_CD=false;
        is_AnimeLoading=false;
       // 技能状态 = Skill_Status.就绪;
         换手状态继承();
      //  技能状态=Skill_Status.就绪;
    
      
    }

    public void Play_pop()
    {
        GetComponent<AudioSource>().Play();
    }

    private void  换手状态继承()
    {
         switch(LR对称显示.技能状态)
         {
             case Skill_Status.技能恢复中:
             继承技能恢复(LR对称显示.Cur_Time,LR对称显示.CD_Time,LR对称显示.CD_cycle.fillAmount);
             break;
             case Skill_Status.技能持续中:
             继承技能持续(LR对称显示.Cur_Time,LR对称显示.Skill_Time,LR对称显示.CD_cycle.fillAmount);
              break;
             case Skill_Status.就绪:
                技能状态 = Skill_Status.就绪;
             break;
         }
    }

    public void Close_Display()
    {
        // Get the current AnimatorStateInfo
        AnimatorStateInfo stateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);

       if(stateInfo.IsName("关闭")||stateInfo.IsName("Skill_close")) return;
       
        GetComponent<Animator>().SetTrigger("Close_skill");
    }

    public void Open_Display()
    {
        GetComponent<Animator>().SetTrigger("Open_skill");
    }

    public bool isSkill_ready()
    {
      
       // Skill_cdText.text = "dawdqdawda";
        if(is_AnimeLoading) return false;
        if(is_start2_CD) return false;
        if(is_Keep_skill) return false;
        return true;
    }

    public void Set_skillActivate(float time)
    {
        技能状态 = Skill_Status.技能持续中;
        GetComponent<Animator>().SetTrigger("Activate_skill");
        is_Keep_skill=true;
        Cur_Time = 0;
        CD_cycle.fillAmount=0;
        Skill_Time = time;
        CD_cycle.color = Color.red;
        
    }

    private void 继承技能持续(float CurT,float Max_T,float fill)
    {
        print("进入技能持续");
        技能状态 = Skill_Status.技能持续中;
        GetComponent<Animator>().SetTrigger("Activate_skill");
        is_Keep_skill=true;
        Cur_Time = CurT;
        CD_cycle.fillAmount=fill;
        Skill_Time = Max_T;
        CD_cycle.color = Color.red;
    }

    private void 继承技能恢复(float CurT,float Max_T,float fill)
    {
        print("进入技能恢复");
        技能状态 = Skill_Status.技能恢复中;
        GetComponent<Animator>().SetTrigger("DeActivate_skill");
        CD_Time=Max_T;
        CD_cycle.fillAmount=fill;
        Cur_Time=CurT;
        Skill_cdText.text=" ";
        CD_cycle.color = Color.green;
        is_start2_CD=true;
    }

   

    

    public void Set_DeActivate()
    {
       // GetComponent<Animator>().SetTrigger("DeActivate_skill");
    }

    public void Set_SkillReady()
    {
        技能状态 = Skill_Status.就绪;
    }


    public void Start_Cd(float cd)
    {
        //
        技能状态 = Skill_Status.技能恢复中;
        GetComponent<Animator>().SetTrigger("DeActivate_skill");
        CD_Time=cd;
        CD_cycle.fillAmount=0;
        Cur_Time = 0;
        Skill_cdText.text=" ";
        CD_cycle.color = Color.green;
        is_start2_CD=true;
    }
      

    private void FixedUpdate() {
         if(is_start2_CD) CD_display();
         if(is_Keep_skill) Skill_Keep();
    }

    private void Skill_Keep()
    {
        Cur_Time+=Time.deltaTime;
        if(Cur_Time>=Skill_Time) {Skill_Keepcomplete(); return;};
        float temp_time = Skill_Time-Cur_Time;
        Display_Text(temp_time);
        Display_SkillFill(temp_time);
    }

    private void CD_display()
    {
        Cur_Time+=Time.deltaTime;
        if(Cur_Time>=CD_Time) {CD_complete(); return;};
        float temp_time = CD_Time-Cur_Time;
        Display_Text(temp_time);
        Display_CDFill(temp_time);
        
    }

    private void Display_Text(float time)
    {
//  print("倒计时时间为："+time);
        Skill_cdText.SetText(time.ToInt()+"");
       
    }

    private void Display_CDFill(float time)
    {
        CD_cycle.fillAmount =  Cur_Time/CD_Time;  
    }

    private void Display_SkillFill(float time)
    {
        CD_cycle.fillAmount =  Cur_Time/Skill_Time;  
    }

    private void CD_complete()
    {
        is_start2_CD=false;
    
        Skill_cdText.SetText(" ");
        CD_cycle.color= Color.green;
        CD_cycle.fillAmount=1f;
        Cur_Time=0;
    }

    private void Skill_Keepcomplete()
    {
        is_Keep_skill=false;
        Skill_cdText.SetText(" ");
        CD_cycle.color= Color.green;
        CD_cycle.fillAmount=0f;
        Cur_Time=0;
    }


}
