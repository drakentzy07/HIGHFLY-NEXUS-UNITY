/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 物理武器系统 / Physical weapon system
 * 功能 / Purpose: 管理抓取、释放、刚体添加、盾牌阻挡、武器碰撞和轨迹检测。
 * English: Manages grabbing, releasing, Rigidbody setup, shield blocking, weapon collision, and trace detection.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class releaseWeapon : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody goal;
    void Start()
    {
        
    }

    public void release_weopon()
    {
        print("进入解除静态");
        this.transform.GetComponent<Rigidbody>().isKinematic=false;
        if(goal) goal.isKinematic=false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
