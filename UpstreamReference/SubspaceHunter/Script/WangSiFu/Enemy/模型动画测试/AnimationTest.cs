/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人动画测试 / Enemy animation test
 * 功能 / Purpose: 用于验证敌人模型、动画状态和动作切换。
 * English: Used to validate enemy models, animation states, and action transitions.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W)) { print("按下W"); GetComponent<Animator>().SetTrigger("W");}
        if(Input.GetKeyDown(KeyCode.A)) GetComponent<Animator>().SetTrigger("A");
        if(Input.GetKeyDown(KeyCode.D)) GetComponent<Animator>().SetTrigger("D");
        if(Input.GetKeyDown(KeyCode.S)) GetComponent<Animator>().SetTrigger("S");

        if(Input.GetKeyDown(KeyCode.Alpha1)) GetComponent<Animator>().SetTrigger("1");
        if(Input.GetKeyDown(KeyCode.Alpha3)) GetComponent<Animator>().SetTrigger("3");
        if(Input.GetKeyDown(KeyCode.Alpha4)) GetComponent<Animator>().SetTrigger("4");
        if(Input.GetKeyDown(KeyCode.Alpha5)) GetComponent<Animator>().SetTrigger("5");
        if(Input.GetKeyDown(KeyCode.Alpha6)) GetComponent<Animator>().SetTrigger("6");
        if(Input.GetKeyDown(KeyCode.Alpha7)) GetComponent<Animator>().SetTrigger("7");

        if(Input.GetKeyDown(KeyCode.B)) GetComponent<Animator>().SetTrigger("B");


    }



}
