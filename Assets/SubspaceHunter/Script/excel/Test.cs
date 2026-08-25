/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 配置表入口 / Configuration table entry
 * 功能 / Purpose: 读取 Luban/Excel 生成配置，向运行时代码提供表格数据入口。
 * English: Reads Luban/Excel generated configuration and exposes table data to runtime code.
 */

using System.Collections;
using System.Collections.Generic;
using GameConfig;
//using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //初始化  不建议这么做 自己框架自己管理里 测试例子
        GameConfigMgr.Load();

        //假设我想获取道具表id为10001的道具 这么做就可以了
       ItemConfig item=  GameConfigMgr.Tables.ItemConfigCategory.GetOrDefault(10001);
       Debug.Log(string.Format("id:{0} name:{1}",item.Id,item.Name));


//又比如我想获取一下某个技能音效的配置  这么做就可以
       AudioConfig audioConfig=GameConfigMgr.Tables.AudioConfigCategory.GetOrDefault(20001);
       Debug.Log(string.Format("id:{0} 音量:{1}  资源地址:{2}",audioConfig.Id,audioConfig.Volume,audioConfig.Path));
       
    }



}
