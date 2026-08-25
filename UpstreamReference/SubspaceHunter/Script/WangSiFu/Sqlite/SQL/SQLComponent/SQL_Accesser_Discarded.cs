/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: SQLite 语句组件 / SQLite statement component
 * 功能 / Purpose: 封装 SQLite 增删改查、建表和编辑语句片段。
 * English: Wraps SQLite create, insert, select, update, delete, alter, and editor statement fragments.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
public class SQL_Accesser_Discarded : MonoBehaviour
{
    private IEnumerator worker;
    public int i;
    public static SQL_Accesser_Discarded instance;
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
    }
    private void Start()
    {
          if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            Permission.RequestUserPermission(Permission.ExternalStorageRead);

        DontDestroyOnLoad(this);
        worker = Sqlite_Manager._DBInstance().copyDbFile();
        StartCoroutine(worker);  
        Invoke("ConnectDatabase", 1f);
        
      
    }
    public void ConnectDatabase()
    {
        Sqlite_Manager._DBInstance().OpenConnect();
    }
    public void CloseDatabase()
    {
        Sqlite_Manager._DBInstance().CloseDB();
    }
    public void UpDateData()
    {
       Sqlite_Manager._DBInstance().UpdataData("Test_table", new string[] { "isNewUser", "1" }, new string[] { "id", "12" });
    }
    public void ResetData()
    {
        Sqlite_Manager._DBInstance().UpdataData("Test_table", new string[] { "isNewUser", "0" }, new string[] { "id", "12" });
    }

    public void SelectData()
    {

        Debug.Log(Sqlite_Manager._DBInstance().SelectData("Test_table", new string[] { "data" }, new string[] { "id", "2" }).GetValue(0)); 

    }

    public string WhichSceneToLoad()
    {
        string k = Sqlite_Manager._DBInstance().SelectData("Test_table", new string[] { "isNewUser" }, new string[] { "id", "12" }).GetValue(0).ToString();
        
        //Debug.Log(i);
        //�޷�תint��
        return k;//0 ûͨ�� 1 ͨ��
    }

 

   
}
