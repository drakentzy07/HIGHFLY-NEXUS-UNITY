/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: AR/MR 场景效果控制 / AR/MR scene effect control
 * 功能 / Purpose: 管理 AR/MR 场景中的可视效果、环境表现和演示状态切换。
 * English: Controls visual effects, environment presentation, and demo state switching in AR/MR scenes.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AR_scen_controller : MonoBehaviour
{
    public GameObject[] Activate_Set;
    public bool is_close_ARLight;
    private GameObject Temp_light;
    
    // Start is called before the first frame update
    void Start()
    {
        if(is_close_ARLight) Close_defual_light();
        Activate_AR_set();
    }

    private void Close_defual_light()
    {
        Temp_light = GameObject.FindWithTag("AR_light");
        if(null!=Temp_light)
        Temp_light.SetActive(false);
    }

    private void Activate_AR_set()
    {
        if(SceneManager.GetActiveScene().name == "AR")
        {
            foreach (var item in Activate_Set)
            {
                item.SetActive(true);
            }
        }
    }

    public void Recovery_AR_defultLight()
    {
       
        if(null!=Temp_light) Temp_light.SetActive(true);
    }

    
}
