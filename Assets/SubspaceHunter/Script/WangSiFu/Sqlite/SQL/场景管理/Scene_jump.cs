/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: SQLite 数据管理 / SQLite data management
 * 功能 / Purpose: 管理本地数据库连接、场景数据读取和测试入口。
 * English: Manages local database connections, scene-data access, and test entry points.
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_jump : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;
    public string Tutorial_scene;
    public string default_scene;
    public static Scene_jump instance;
    public static Scene_jump _DBInstance()
    {
        
        return instance;
    }
    private void Awake()
    {
        if (instance == null) instance = this;
    }

     private void Start()
    {
        DontDestroyOnLoad(this); 
    }

    public void start2_switchScene()
    {
        textMeshProUGUI.text+="进入场景跳转函数\n";
        if(is_Frist())
        {
            textMeshProUGUI.text+="成功进入初次逻辑\n";
            print("初次");
            SceneManager.LoadScene(Tutorial_scene);
            update_enter();
        }
        else
        {
            print("非初次");
            SceneManager.LoadScene(default_scene);
        }
        
    }

    private bool is_Frist()
    {
        print("进入初次判断");
        string res="";
        foreach (var item in Sqlite_Manager._DBInstance().GetDataBySqlQuery("Player",new string[] { "First_enter"}))
         {
             //Debug.Log(item);
             res =item;
             if(item == "yes")  textMeshProUGUI.text+="第一次访问\n"; // print("查找结果为yes");
             else   textMeshProUGUI.text+="非第一次访问\n";   //print("查找结果不为yes");
        //     textMeshProUGUI.text+=item+"";
         }
        return res=="yes"? true : false;
    }



    public void update_enter()
    {
       // Sqlite_Manager._DBInstance().UpdataDataAll("Player",new string[] { "First_enter","no"});
         foreach (var item in Sqlite_Manager._DBInstance().UpdataDataAll("Player",new string[] { "First_enter","no"}))
         {
             Debug.Log(item);
            // res =item;
            // if(item == "yes") print("查找结果为yes");
           //  else  print("查找结果不为yes");
        //     textMeshProUGUI.text+=item+"";
         }
    }

   
}
