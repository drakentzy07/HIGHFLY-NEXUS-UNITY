/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 控制器魔法输入 / Controller magic input
 * 功能 / Purpose: 把控制器输入映射到魔法选择、准备和释放流程。
 * English: Maps controller input to magic selection, preparation, and release flow.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magic_choice : MonoBehaviour
{
    public enum MagicType
    {
        火,
        电,
        盾牌,
        冰,
        陨石,
        治疗,
        未激活
    }
    public MagicType magicType = MagicType.未激活;
    public GameObject Father;
    private Player_magicController player_MagicController_L;
    private Player_magicController player_MagicController_R;
    // Start is called before the first frame update
    void Start()
    {
        player_MagicController_L = GameObject.FindWithTag("Magic_leftHand").GetComponent<Player_magicController>();
        player_MagicController_R = GameObject.FindWithTag("Magic_rightHand").GetComponent<Player_magicController>();
    
    }

    private void OnTriggerEnter(Collider other) {
        print("进入魔法使用检测");
        if(other.tag=="Player_rightHand")
        {
            print("进入魔法使用检测右手");
           Set_magic_right();
        }else if(other.tag=="Player_leftHand")
        {
            print("进入魔法使用检测左手");
            Set_magic_left();
        }
    }

    private void Set_magic_left()
    {
        if(magicType == MagicType.火)
        {
            player_MagicController_L.set_fire();
        }else if(magicType == MagicType.电)
        {
            player_MagicController_L.set_electr();
        }else if(magicType == MagicType.盾牌)
        {
            player_MagicController_L.set_shield();
        }else if(magicType == MagicType.冰)
        {
             player_MagicController_L.set_ice();
        }else if(magicType == MagicType.陨石)
        {
             player_MagicController_L.set_stone();
        }
        Destroy(Father);
    }
    private void Set_magic_right()
    {
        if(magicType == MagicType.火)
        {
            player_MagicController_R.set_fire();
        }else if(magicType == MagicType.电)
        {
            player_MagicController_R.set_electr();
        }else if(magicType == MagicType.盾牌)
        {
            player_MagicController_R.set_shield();
        }else if(magicType == MagicType.冰)
        {
             player_MagicController_R.set_ice();
        }else if(magicType == MagicType.陨石)
        {
             player_MagicController_R.set_stone();
        }
        Destroy(Father);
    }

}
