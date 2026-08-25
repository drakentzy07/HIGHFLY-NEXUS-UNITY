/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 角色 IK 与控制器绑定 / Character IK and controller binding
 * 功能 / Purpose: 处理手部、控制器和角色骨骼之间的 IK 对齐关系。
 * English: Handles IK alignment between hands, controllers, and character bones.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class EventListener<T>                          //事件监听参数类***
    {
        public delegate void OnValueChangeDelegate(T newVal);
        public event OnValueChangeDelegate OnVariableChange;
        private T m_value;
       
        public T Value
        {
            get
            {
                return m_value;
            }
            set
            {
                if (m_value.Equals(value)) return;
                if (OnVariableChange != null)
                    OnVariableChange(m_value);
                m_value = value;
            }
        }
    }



public class Hand_and_controller : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("控制器追踪")]
    public SkinnedMeshRenderer controller_hand;
    [Header("手部追踪")]
    public SkinnedMeshRenderer Tracing_hand;
    [Header("当前追踪的类型")]
    public string Cur_Tracing_type;
    [Header("玩家模型挂载的IK")]
    public VRIK Character_IK;

    [Header("手部追踪对应的VR节点")]
    public GameObject Hand_trace_left;
    public GameObject Hand_trace_right;

    [Header("手部追踪对应的控制器节点")]
    public GameObject Controller_trace_left;
    public GameObject Controller_trace_right;


    EventListener<bool> listenerTest;//用来监听是否控制器和手部追踪切换  

    private bool  Hand_is_init;
    public bool is_handtracing;
    
    void Start()
    {
        //初始化确定玩家进入游戏一开始用的是手柄还是手追
        first_HandController();
        //后续动态监听
        Invoke("Register_listen_hand",1f);
         
        // listenerTest.Value=controller_hand.enabled;

    }

    void first_HandController()
    {
        if(controller_hand.enabled==false)
        {
              
                SYN_Hand_taceingPoint();
        }else
        {
               
                SYN_controller_taceingPoint();
        }
    }

    private void FixedUpdate() {
        if(true==Hand_is_init)
        listenerTest.Value=controller_hand.enabled;
    }

    private void test_valueChanger(bool value)
    {
      
        print("变化后的变量为: "+value);
        if(false==value)
        {
            print("追踪手");
           
            SYN_Hand_taceingPoint();
        }else
        {
            print("追踪控制器");
           
            SYN_controller_taceingPoint();
        }
    }


    //注册手部切换监听事件
    private void Register_listen_hand()
    {
         listenerTest = new EventListener<bool>(); 
         listenerTest.OnVariableChange += test_valueChanger; 
         Hand_is_init=true;
    }
    
    private void SYN_controller_taceingPoint()
    {
        //不知道为什么是反过来的..
        is_handtracing =true;
         Character_IK.solver.leftArm.target=null;
        Character_IK.solver.rightArm.target=null;

        Character_IK.solver.leftArm.target=Hand_trace_left.transform;
        Character_IK.solver.rightArm.target=Hand_trace_right.transform;
        print("切换控制器");

    }

    private void SYN_Hand_taceingPoint()
    {
        is_handtracing =false;
            
          Character_IK.solver.leftArm.target=null;
        Character_IK.solver.rightArm.target=null;

        Character_IK.solver.leftArm.target=Controller_trace_left.transform;
        Character_IK.solver.rightArm.target=Controller_trace_right.transform;
        print("切换手");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
