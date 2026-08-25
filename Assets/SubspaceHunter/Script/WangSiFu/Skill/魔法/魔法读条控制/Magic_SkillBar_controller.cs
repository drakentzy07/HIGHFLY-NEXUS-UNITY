/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 魔法读条控制 / Magic cast-bar control
 * 功能 / Purpose: 管理魔法准备读条、释放条件和取消状态。
 * English: Manages magic cast preparation, release conditions, and cancellation state.
 */

using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class Magic_SkillBar_controller : UnityEngine.MonoBehaviour
{
    public UnityEngine.UI.Image[] bar_set;
    private bool is_preparing;
    private float Total_bar_Number;
    private float TotalPrepare_Time;

    private float Cur_ready_level=0;
    public GameObject Magic_aim;
    [HideInInspector]
    public bool is_skillReady;
    public Player_magicController player_MagicController;
    public float Cur_scale;

  
    private void FixedUpdate() {
        if(is_preparing) preparing();
    }


    public void prepare(float TotalPrepareTime=2f,float Total_barNumber=1)
    {   
        Total_bar_Number = Total_barNumber;
        TotalPrepare_Time = TotalPrepareTime;
        player_MagicController.activate_magic_ball();
        is_preparing = true;
    }

    public void Pause_bar()
    {
        player_MagicController.Deactivate_magic_ball();
        is_preparing =false;
    }

    public void Reset_bar_status()
    {
        foreach (var item in bar_set)
        {
            item.fillAmount=0;
            Cur_ready_level = 0;
            player_MagicController.get_CurBall().transform.localScale= new Vector3(0,0,0);
            Magic_aim.SetActive(false);
            player_MagicController.Deactivate_magic_ball();
            is_skillReady=false;
            is_preparing=false;

        }
    }

    private void preparing()
    {
       if(Total_bar_Number==1)
       {
            if(bar_set[0].fillAmount<1)
            {
                bar_set[0].fillAmount+=1/TotalPrepare_Time*Time.fixedDeltaTime;
                 Cur_scale = player_MagicController.get_CurBall().transform.localScale.x+1/TotalPrepare_Time*Time.fixedDeltaTime;
                player_MagicController.get_CurBall().transform.localScale = new Vector3(Cur_scale,Cur_scale,Cur_scale);
            }
            
            else if(!is_skillReady)
            {
                bar_set[0].fillAmount=1;
                Cur_ready_level = 1;
                if(player_MagicController.need_aim())
                Magic_aim.SetActive(true);
                player_MagicController.get_CurBall().transform.localScale = new Vector3(1,1,1);
                player_MagicController.Deal_Skill_excute_immediatly();
                is_skillReady= true;
                is_preparing = false;
            }else if(is_skillReady)
            {
                player_MagicController.get_CurBall().transform.localScale = new Vector3(1,1,1);
            }
       }
    }
}
