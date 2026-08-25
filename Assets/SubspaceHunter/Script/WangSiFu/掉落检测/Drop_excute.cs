/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 掉落检测 / Drop detection
 * 功能 / Purpose: 检测物体掉落、执行回收或触发掉落管理逻辑。
 * English: Detects dropped objects, performs recovery, or triggers drop-management logic.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop_excute : MonoBehaviour
{
     GameObject[] Drop_items;
    public float Wait_time=4f;
    public float explosionForce = 500f; // 爆炸力
     public float upwardModifier = 300f; // 向上抛射的力的调节因子

    public void Set_objs(GameObject[] groups)
    {
        print("掉落检测6");
        Drop_items = groups;
        if(Drop_items.Length==0) print("掉落检测6数组为空");
        print("掉落检测生成数量为"+Drop_items.Length);
    }

    public void ExplodeAndDropItems()
    {
        print("掉落检测7");
        Invoke("Deal_logic",Wait_time);
    }

    private void Deal_logic()
    {
        print("掉落检测8");
        // 生成一个随机数
            float randomValue = Random.Range(0f, 1f);
        // 遍历所有道具
        foreach (GameObject item in Drop_items)
        {
            // 实例化道具
            GameObject droppedItem = Instantiate(item, transform.position, Quaternion.identity);
            print("掉落检测生成的物体名为:"+droppedItem.gameObject.name);
           
            // 获取 Rigidbody 组件
                Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 计算向上的力量
                    Vector3 upwardForce = Vector3.up * upwardModifier;

                    // 计算爆炸方向（随机方向）
                    Vector3 explosionDirection = Random.onUnitSphere; // 随机单位球面方向
                    explosionDirection.y = Mathf.Abs(explosionDirection.y); // 确保 y 分量为正

                    // 应用爆炸力
                    rb.AddForce(upwardForce + explosionDirection.normalized * explosionForce);
                }
        }
    }
}
