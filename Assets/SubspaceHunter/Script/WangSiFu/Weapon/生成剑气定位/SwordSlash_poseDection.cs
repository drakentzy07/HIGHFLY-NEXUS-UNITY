/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 剑气生成定位 / Sword-slash spawn positioning
 * 功能 / Purpose: 根据武器姿态计算剑气或斩击特效的生成位置。
 * English: Computes spawn positions for sword slash or blade-wave effects from weapon pose.
 */

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SwordSlash_poseDection : MonoBehaviour
{
    Transform referenceTransform;
    // 被检测的实时运动的点
     Transform movingPoint;
     // 定义一个检测平面
    private Plane detectionPlane;

    // 用来保存前一次的距离
    private float previousDistance;
    [Range(0.01f,0.5f)]
    public float 中间波动范围绝对值= 0.01f;
    private bool is_detecting;


    void Start()
    {   
           movingPoint = GetComponent<Physic_weaponManager>().slash_trans;
           referenceTransform = Camera.main.transform;
           // 使用参考Transform的Z轴和Y轴方向计算平面的法向量
            Vector3 planeNormal = Vector3.Cross(referenceTransform.up, referenceTransform.forward);
            // 使用参考Transform的位置作为平面上的一点
            Vector3 planePoint = referenceTransform.position;

            // 创建检测平面
            detectionPlane = new Plane(planeNormal, planePoint);

            // 初始化时，计算运动点到平面的初始距离
            previousDistance = detectionPlane.GetDistanceToPoint(movingPoint.position);
    }

    private void auto_stop()
    {
        if(is_detecting) is_detecting=false;
    }

    public void Start_slash_Detect()
    {
        print("挥舞检测5");
        previousDistance = detectionPlane.GetDistanceToPoint(movingPoint.position);
        is_detecting=true;
        Invoke("auto_stop",0.4f);
        
    }

    public bool Get_dir()
    {
        return detectionPlane.GetDistanceToPoint(movingPoint.position)>0? true :false;
    }

    // Update is called once per frame
    void Update()
    {
        if(is_detecting) CheckPointPositionRelativeToPlane();
    }

    void CheckPointPositionRelativeToPlane()
    {
        // 获取当前运动点到平面的距离
        float currentDistance = detectionPlane.GetDistanceToPoint(movingPoint.position);
        print("挥舞检测6");
       if(math.abs(currentDistance)<中间波动范围绝对值)
       {
            print("挥舞检测7");
            GetComponent<Physic_weaponManager>().Ger_slash_effect();
            previousDistance = currentDistance;
            is_detecting =false;
       }

        // 更新前一次的距离
        previousDistance = currentDistance;
    }

      private void OnDrawGizmos()
    {
        if (referenceTransform == null) return;

        // 计算平面法向量和位置
        Vector3 planeNormal = Vector3.Cross(referenceTransform.forward, referenceTransform.up);
        Vector3 planePoint = referenceTransform.position;

        // 平面大小设置
        float planeSize = 5f;

        // 获取平面上两个轴方向（用于绘制平面的轮廓）
        Vector3 planeForward = referenceTransform.forward * planeSize;
        Vector3 planeUp = referenceTransform.up * planeSize;

        // 计算平面四个角的顶点
        Vector3 corner1 = planePoint + planeForward + planeUp;
        Vector3 corner2 = planePoint + planeForward - planeUp;
        Vector3 corner3 = planePoint - planeForward + planeUp;
        Vector3 corner4 = planePoint - planeForward - planeUp;

        // 设置Gizmos的颜色
        Gizmos.color = Color.green;

        // 绘制平面的四条边
        Gizmos.DrawLine(corner1, corner2);
        Gizmos.DrawLine(corner2, corner4);
        Gizmos.DrawLine(corner4, corner3);
        Gizmos.DrawLine(corner3, corner1);

        // 还可以绘制平面的法向量
        Gizmos.color = Color.red;
        Gizmos.DrawLine(planePoint, planePoint + planeNormal * 2); // 法向量方向可视化
    }
}
