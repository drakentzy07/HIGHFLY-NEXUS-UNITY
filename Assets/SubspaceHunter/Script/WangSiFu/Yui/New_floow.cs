/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 跟随与语音助手原型 / Follow and voice-assistant prototype
 * 功能 / Purpose: 控制跟随对象或早期语音助手交互原型。
 * English: Controls follower objects or early voice-assistant interaction prototypes.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class New_floow : MonoBehaviour
{   public Transform target; // 目标点
    public float followDistance = 5.0f; // 保持的相对距离
    public float relativeHeight = 3.0f; // 保持的相对高度
    public float catchUpSpeed = 1.5f; // 追赶速度倍数
    public float rotationSpeed = 2.0f; // 旋转速度，用于实现平滑看向目标点

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // 计算飞行物体与目标点之间的距离
        Vector3 directionToTarget = target.position - transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        // 计算期望的位置（保持一定距离和高度）
        Vector3 desiredPosition = target.position - directionToTarget.normalized * followDistance;
        desiredPosition.y = target.position.y + relativeHeight;

        // 计算速度，如果距离大于保持距离，就以更快速度追上目标
        float speedMultiplier = distanceToTarget > followDistance ? catchUpSpeed : 1.0f;

        // 移动到期望位置
        rb.velocity = (desiredPosition - transform.position) * speedMultiplier;

        // 让物体看向目标并保持垂直于水平面
        Vector3 lookDirection = (target.position - transform.position).normalized;
        lookDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
    }
}
