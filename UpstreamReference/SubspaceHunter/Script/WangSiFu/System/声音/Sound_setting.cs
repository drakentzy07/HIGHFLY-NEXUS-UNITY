/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 音频系统 / Audio system
 * 功能 / Purpose: 控制背景音乐、动画音效、脚步声、挥砍声和系统音量设置。
 * English: Controls background music, animation sounds, footsteps, swing sounds, and system volume settings.
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Sound_setting : MonoBehaviour
{
    public TextMeshProUGUI textMeshProo;
    public Sound_unit  总音量;
    public Sound_unit  音乐;
    public Sound_unit  音效;
    public Sound_unit  战斗;
    public Sound_unit  环境;
    public Sound_unit  脚步;
    public Sound_unit  系统;
    private GameObject SQL_system;
    List<Dictionary<string, object>> res; //存储每次打开panel加载的声音参数，如果不保存就要恢复。
    SystemSounds_manager systemSounds_Manager;
    [HideInInspector]

    public AudioMixer audioMixer;

    private void Start() {
        SQL_system = GameObject.FindWithTag("SQL_manager");
        if(SQL_system==null) textMeshProo.text += "SQL_system没找到";
        else textMeshProo.text += "SQL_system找到了///";
        systemSounds_Manager = SQL_system.GetComponent<SystemSounds_manager>();
        if(systemSounds_Manager==null) textMeshProo.text = "systemSounds_Manager没找到";
        else textMeshProo.text += "systemSounds_Manager找到了////";
        audioMixer = systemSounds_Manager.audioMixer;
        if(audioMixer==null) textMeshProo.text = "audioMixer没找到";
        else textMeshProo.text += "audioMixer找到了////";
        init_sound_panel();
        
        //init_sound_panel();
    }


    //如果点击的是关闭按钮，就reset成之前的声音效果，unity内。
    public void Not_saveReset()
    {
        foreach (var item in res)
        {
            string type =item["SettingName"].ToString();
            string amountValue =item["Amount"].ToString();
        //    print(type+amountValue);
            //if(type=="System") print("系统音量为"+amountValue);
            set_audio(type,float.Parse(amountValue));
            
        }
         GetComponent<UI_openClose_manager>().play_close_animator();
    }

    public void init_sound_panel()
    {
        print("进入声音面板初始化设置");
        textMeshProo.text += "进入声音面板初始化设置/////";
        res = systemSounds_Manager.get_sound_data();
        
        if(res==null) { textMeshProo.text += "初始声音数据没找到"; print("初始声音数据没找到");}
        else textMeshProo.text += "初始声音数据找到了////";
       
         foreach (var item in res)
        {
            string type =item["SettingName"].ToString();
            string amountValue =item["Amount"].ToString();
        //    print(type+amountValue);
            //if(type=="System") print("系统音量为"+amountValue);
            set_vale(type,float.Parse(amountValue));
            
        }
    }

     private void set_vale(string tpye,float amount)
    {
        textMeshProo.text += "进入声音设置////";
        switch (tpye)
        {
            
            case "Main":
            textMeshProo.text += "设置总音量，大小为："+amount;
             总音量.set_init_status( amount);
            break;
            case "Music":
             音乐.set_init_status( amount);
            break;
            case "SoundsEffect":
            音效.set_init_status( amount);
            break;
            case "Environment":
            textMeshProo.text += "设置环境，大小为："+amount;
             战斗.set_init_status( amount);
            break;
            case "Battle":
            textMeshProo.text += "设置战斗，大小为："+amount;
             环境.set_init_status( amount);
            break;
            case "Footstep":
             脚步.set_init_status( amount);
            break;
            case "System":
             系统.set_init_status( amount);
            break;
        }
    }
    
    public void Save_soundSetting()
    {
    
        systemSounds_Manager.Update_soundSetting(
            new string[] { "Amount", 总音量.value().ToString() },new string[] {"SettingName", "Main" });

        systemSounds_Manager.Update_soundSetting(
            new string[] { "Amount", 音乐.value().ToString() },new string[] {"SettingName", "Music" });

        systemSounds_Manager.Update_soundSetting(
            new string[] { "Amount", 音效.value().ToString() },new string[] {"SettingName", "SoundsEffect" });

        systemSounds_Manager.Update_soundSetting(
            new string[] { "Amount", 环境.value().ToString() },new string[] {"SettingName", "Environment" });
        
        systemSounds_Manager.Update_soundSetting(
            new string[] { "Amount", 战斗.value().ToString() },new string[] {"SettingName", "Battle" });
        
        systemSounds_Manager.Update_soundSetting(
            new string[] { "Amount", 脚步.value().ToString() },new string[] {"SettingName", "Footstep" });

        systemSounds_Manager.Update_soundSetting(
            new string[] { "Amount", 系统.value().ToString() },new string[] {"SettingName", "System" });

        systemSounds_Manager.fresh_soundSetting();
        GetComponent<UI_openClose_manager>().play_close_animator();
        //Destroy(transform.gameObject);

        
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

}
