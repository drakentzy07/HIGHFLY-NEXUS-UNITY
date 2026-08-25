/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 系统时间显示 / System time display
 * 功能 / Purpose: 读取并显示系统时间，用于 UI 或演示面板。
 * English: Reads and displays system time for UI or demo panels.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class Display_systemTime : MonoBehaviour
{
    //public Text CurrrentTimeText;
    public TextMeshProUGUI Second_TimeText;
    public TextMeshProUGUI Year_TimeText;
    private int hour;
    private int minute;
    //private int second;
    private int year;
    private int month;
    private int day;

    // Use this for initialization
    void Start () {
       // CurrrentTimeText = GetComponent<Text>();

    }
    
    // Update is called once per frame
    void Update () {
        //获取当前时间
        hour = DateTime.Now.Hour;
        minute = DateTime.Now.Minute;
        //second = DateTime.Now.Second;
        year = DateTime.Now.Year;
        month = DateTime.Now.Month;
        day = DateTime.Now.Day;

        //格式化显示当前时间
        Second_TimeText.text = string.Format("{0:D2}:{1:D2} " , hour, minute);
         Year_TimeText.text = string.Format("{0:D4}/{1:D2}/{2:D2}", year, month, day);

        #if UNITY_EDITOR
      // Debug.Log("W now " + System.DateTime.Now);     //当前时间（年月日时分秒）
     //   Debug.Log("W utc " + System.DateTime.UtcNow);  //当前时间（年月日时分秒）
        #endif
    }
}
