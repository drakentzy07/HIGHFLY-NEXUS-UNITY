/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 音频系统 / Audio system
 * 功能 / Purpose: 控制背景音乐、动画音效、脚步声、挥砍声和系统音量设置。
 * English: Controls background music, animation sounds, footsteps, swing sounds, and system volume settings.
 */

using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using UnityEngine;
using UnityEngine.Audio;

public class SystemSounds_manager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public int Max_field_amount=3;
    public int Cur_amount;
     List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
    
    private void Awake() {
         Invoke("get_date",0.1f);
        // Invoke("Update_soundSetting",3f);
       
    }

    public List<Dictionary<string, object>> get_sound_data()
    {
        get_date();
        return results;
    }

    private void get_date()
    {
        if(null!=Sqlite_Manager._DBInstance()&&!Sqlite_Manager._DBInstance().is_connection_open())
        {
            Invoke("get_date",0.1f);
            return;
        }
        
        print("进入获取数据");
        SqliteDataReader reader = Sqlite_Manager._DBInstance().查询字段返回Reader("Sounds_setting",new string[] { "SettingName","Amount","is_open"});
       //  List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
         // 遍历查询结果
        while (reader.Read())
        {
              Dictionary<string, object> row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string fieldName = reader.GetName(i);
                object fieldValue = reader.GetValue(i);
                row.Add(fieldName, fieldValue);
            }
            results.Add(row);
            
        }
      //  object amountValue = results[2]["Amount"]; // 通过列名 "Amount" 获取值
          //   Debug.Log("第三行的 Amount 值: " + amountValue);
          Init_Sounds();
    }

    private void Init_Sounds()
    {
        //audioMixer.SetFloat("Environment",-80);
        foreach (var item in results)
        {
            string type =item["SettingName"].ToString();
            string amountValue =item["Amount"].ToString();
        //    print(type+amountValue);
            if(type=="System") print("系统音量为"+amountValue);
            set_audio(type,float.Parse(amountValue));
            
        }
    }

    private void set_audio(string tpye,float amount)
    {
        switch (tpye)
        {
            
            case "Main":
             audioMixer.SetFloat("Master",amount);
            break;
            case "Music":
             audioMixer.SetFloat("Music",amount);
            break;
            case "SoundsEffect":
             audioMixer.SetFloat("SoundEffect",amount);
            break;
            case "Environment":
             audioMixer.SetFloat("Environment",amount);
            break;
            case "Battle":
             audioMixer.SetFloat("Battle",amount);
            break;
            case "Footstep":
             audioMixer.SetFloat("Footstep",amount);
            break;
            case "System":
             audioMixer.SetFloat("System",amount);
            break;
        }
    }

    public void Update_soundSetting(string[] values2,string[] conditions2)
    {
         print("修改数据");
        //string[] values2 = { "Amount", "-80" };
        //string[] conditions2 = { "SettingName", "System" };
        Sqlite_Manager._DBInstance().UpdataData("Sounds_setting",values2,conditions2);
        //fresh_soundSetting();
    }

    public void fresh_soundSetting()
    {
        //print("进入获取数据");
        SqliteDataReader reader = Sqlite_Manager._DBInstance().查询字段返回Reader("Sounds_setting",new string[] { "SettingName","Amount","is_open"});
       //  List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
         // 遍历查询结果
        while (reader.Read())
        {
              Dictionary<string, object> row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string fieldName = reader.GetName(i);
                object fieldValue = reader.GetValue(i);
                row.Add(fieldName, fieldValue);
            }
            results.Add(row);
            
        }
      //  object amountValue = results[2]["Amount"]; // 通过列名 "Amount" 获取值
          //   Debug.Log("第三行的 Amount 值: " + amountValue);
          Init_Sounds();
    }

}
