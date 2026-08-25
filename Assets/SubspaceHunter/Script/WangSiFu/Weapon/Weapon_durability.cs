/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 武器交互 / Weapon interaction
 * 功能 / Purpose: 处理左右手检测、武器切换、耐久度和命中特效入口。
 * English: Handles left/right hand detection, weapon switching, durability, and hit-effect entry points.
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapon_durability : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;
    public TextMeshProUGUI textMeshProUGUI2;
    public Destory_self destory_Self;
    [Range(1,1000)]
    public float Max_durability=200;
    private float Cur_durability=0;
    public SaoDeathCtr saoDeathCtr;

    private bool is_inTheDeating;


    private void Start() {
        Cur_durability = Max_durability;
        if(null!=textMeshProUGUI)
        textMeshProUGUI.text = Cur_durability+"/"+Max_durability;
        if(null!=textMeshProUGUI2)
        textMeshProUGUI2.text = Cur_durability+"/"+Max_durability;
    }

    public bool Mins_dur(float amount)
    {
        if(is_inTheDeating) return false;
        float temp_result = Cur_durability-amount;
        if(temp_result<0) temp_result = 0;
        Cur_durability = temp_result;
        refresh_text();
        if(Cur_durability==0) Deal_weaponBroken();
        return true;

    }

    private void refresh_text()
    {
        if(null!=textMeshProUGUI)
        textMeshProUGUI.text = Cur_durability+"/"+Max_durability;
        if(null!=textMeshProUGUI2)
        textMeshProUGUI2.text = Cur_durability+"/"+Max_durability;
    }

    
    public void Deal_weaponBroken()
    {
        if(is_inTheDeating) return;
        is_inTheDeating=true;
        GetComponent<AudioSource>().Play();
        if(0!=saoDeathCtr.transform.childCount)
        {
           GameObject Item_shadow =saoDeathCtr.transform.GetChild(0).gameObject;
           if(null!= Item_shadow) Item_shadow.SetActive(false);
        }
        
       
        
        saoDeathCtr.Death();
        Invoke("close_weaponItem",saoDeathCtr.deathTime);
    }

    private void close_weaponItem()
    {
        destory_Self.Destroy_selfGameObject();
    }


    
}
