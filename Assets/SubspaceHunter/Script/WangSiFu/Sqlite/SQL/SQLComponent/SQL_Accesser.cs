/*
 * SubspaceHunter-SAO bilingual code note / åŒè¯­ä»£ç è¯´æ˜
 * æ¨¡å— / Module: SQLite è¯­å¥ç»„ä»¶ / SQLite statement component
 * åŠŸèƒ½ / Purpose: å°è£… SQLite å¢åˆ æ”¹æŸ¥ã€å»ºè¡¨å’Œç¼–è¾‘è¯­å¥ç‰‡æ®µã€‚
 * English: Wraps SQLite create, insert, select, update, delete, alter, and editor statement fragments.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SQL_Accesser : MonoBehaviour
{
    private IEnumerator worker;
    public static SQL_Accesser instance;

    static public string DatabaseName = "Test_table";

    public float connectTime = 1f;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
       // DontDestroyOnLoad(this.gameObject);

        worker = SQLComponent.copyDbFile();//ÒòÎªÊÇĞ¯³ÌËùÒÔÏÂÃæµÄ»áÖ±½Ó¿ªÆô£¬²»»á°´Ë³Ğò
        StartCoroutine(worker);

        
          
        
       
        //Invoke("ConnectDatabase", connectTime);

    }
    private void OnDisable()
    {

        //StopAllCoroutines();//?Õâ¸öÎÊÌâÂğ
        //Debug.Log("Disable SQL_Caller");
        //CloseDatabase();
    }
    private void OnDestroy()
    {
      
    }
    private void OnApplicationQuit()
    {
        Debug.Log("´İ»Ù");
        StopAllCoroutines();//?Õâ¸öÎÊÌâÂğ ¸Ğ¾õÊÇÕâ¸öÎÊÌâ onDisable Èç¹ûdontdestyyÓ¦¸Ã²»»áµ÷ÓÃ°É£¿£¿£¿
        CloseDatabase();
        //https://blog.csdn.net/weixin_34081595/article/details/85755067 ²»Í¬ÎïÌåondisable destryµ÷ÓÃË³Ğò²»Í¬
        //Ôì³ÉÊı¾İ¿â¹Ø±Õ»¹»á·ÃÎÊ ¼´destroy±ÈdisableÏÈµ÷ÓÃ
    }

    public void ConnectDatabase()
    {
        SQLComponent.OpenConnect();
    }
    public void CloseDatabase()
    {
        SQLComponent.CloseDB();
    }
    public void UpDateData()
    {
        SQLComponent.UpdataData(DatabaseName, new string[] { "isNewUser", "1" }, new string[] { "id", "12" });
    }
    public void UpdateVolumeData(float volume)//È«¾ÖÒôÁ¿
    {
        SQLComponent.UpdataData(DatabaseName, new string[] { "GlobalSoundVolume", volume.ToString() }, new string[] { "id", "12" });
    }
    public void UpdateMuteData(int i)
    {
        SQLComponent.UpdataData(DatabaseName, new string[] { "IsMute", i.ToString() }, new string[] { "id", "12" });
    }
    public void UpdateBGMVolumeData(float volume)//BGMÒôÁ¿
    {
        SQLComponent.UpdataData(DatabaseName, new string[] { "BGMVolume", volume.ToString() }, new string[] { "id", "12" });
    }
    public void UpdateBGMMuteData(int i)//BGM½ûÓÃ
    {
        SQLComponent.UpdataData(DatabaseName, new string[] { "IsMuteBGM", i.ToString() }, new string[] { "id", "12" });
    }
    public void UpdateFrequencyData(int volume)//È«¾ÖÒôÁ¿
    {
        SQLComponent.UpdataData(DatabaseName, new string[] { "Frequency", volume.ToString() }, new string[] { "id", "12" });
    }
    public void UpdateAmplitudeData(float volume)//È«¾ÖÒôÁ¿
    {
        SQLComponent.UpdataData(DatabaseName, new string[] { "Amplitude", volume.ToString() }, new string[] { "id", "12" });
    }
    public void UpdateVRorMR(int i)
    {
        SQLComponent.UpdataData(DatabaseName, new string[] { "VRMR", i.ToString() }, new string[] { "id", "12" });
    }
    public void ResetData()
    {
        SQLComponent.UpdataData(DatabaseName, new string[] { "isNewUser", "0" }, new string[] { "id", "12" });
    }
    public void InsertData()
    {

    }
    public void SelectData()
    {

        Debug.Log(SQLComponent.SelectData(DatabaseName, new string[] { "isNewUser" }, new string[] { "id", "12" }).GetValue(0));

    }

    public string WhichSceneToLoad()
    {
        string k = SQLComponent.SelectData(DatabaseName, new string[] { "isNewUser" }, new string[] { "id", "12" }).GetValue(0).ToString();
        //Debug.Log(i);
        //ÎŞ·¨×ªint£¿
        return k;//0 Ã»Í¨¹ı 1 Í¨¹ı
    }

    public string ReturnData( string type,string id)
    {
        string k = SQLComponent.SelectData(DatabaseName, new string[] { type }, new string[] { "id", id }).GetValue(0).ToString();
        return k;
    }

    public string LoadToMRorVR()
    {
        return SQLComponent.SelectData(DatabaseName, new string[] { "VRMR" }, new string[] { "id", "12" }).GetValue(0).ToString();
    }
    /// <summary>
    /// Ã¿´Î¸üĞÂÇ°¼ÇµÃ¸üĞÂÕâ¸ö
    /// </summary>
    public void AddColumn()
    {
        UpdateDatabaseColumns("GlobalSoundVolume", "float", "0.6");
        UpdateDatabaseColumns("IsMute", "integer", "0");
        UpdateDatabaseColumns("BGMVolume", "float", "0.6");
        UpdateDatabaseColumns("IsMuteBGM", "int", "0");
        UpdateDatabaseColumns("Frequency", "int", "150");
        UpdateDatabaseColumns("Amplitude", "float", "0.6");
        UpdateDatabaseColumns("VRMR", "int", "1");//1 MR 0 VR
    }

    public  void UpdateDatabaseColumns(string columnName, string type, string value)
    {
        try
        {
            SQLComponent.AddColumn(SQL_Accesser.DatabaseName, columnName, type);//ÈôÊ§°ÜÖ±½ÓÌø³ö£¬²»¸³Öµ£¬´ú±íÓĞcolumnÁË
            SQLComponent.UpdataData(SQL_Accesser.DatabaseName, new string[] { columnName, value }, new string[] { "id", "12" });
            // SQLComponent.AddColumn(SQL_Accesser.DatabaseName, "B", "integer");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

    }

}
