/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: SQLite 数据管理 / SQLite data management
 * 功能 / Purpose: 管理本地数据库连接、场景数据读取和测试入口。
 * English: Manages local database connections, scene-data access, and test entry points.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System;
using UnityEngine.SceneManagement;
using TMPro;

using System.Data;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEngine.Networking;

public class Sqlite_Manager : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;
    public TextMeshProUGUI textMeshProUGUI2;
    private static Sqlite_Manager _dbInstance;
    public static Sqlite_Manager _DBInstance()
    {
        
        return _dbInstance;
    }
    [SerializeField]
    private string dbName;//= "SubspaceHunter";
    //建立数据库连接
    SqliteConnection connection;
    //数据库命令
    SqliteCommand command;
    //数据库阅读器
    SqliteDataReader reader;

    /* private void Awake()
    {
        if(_dbInstance==null)
        {
            _dbInstance=this;
            OpenConnect();
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
        //连接数据库
        
    }*/

    public bool is_connection_open()
    {
        
        return connection != null && connection.State == ConnectionState.Open?true:false;
    }

    private void Awake()
    {
        _dbInstance = this;
      //  StartCoroutine(copyDbFile());
    }

    private void OnDestroy()
    {
        //断开数据库连接
        CloseDB();
    }


    private void Show_OnTextDebug(string text)
    {
        if(textMeshProUGUI2!=null)
        {
            textMeshProUGUI2.text+=text+"\n";
        }
    }

    private void Show_OnText(string text)
    {
        if(textMeshProUGUI!=null)
        {
            textMeshProUGUI.text+=text+"\n";
        }
    }

    public void temp()
    {
        OpenConnect();
        
        #region 更新数据
        UpdataData("Test_table", new string[] { "data", "update" }, new string[] { "id", "12" });

        foreach (var item in GetDataBySqlQuery("Test_table", new string[] { "id", "data" }))
        {
            //Debug.Log(item);
            Show_OnText(item);

        }
        #endregion 更新数据 
    }
    void Start()
    {

        DontDestroyOnLoad(this);
         Invoke("测试读取数据",4f);

        //创建表
        //string[] colNames = new string[] { "name", "password" };
        //string[] colTypes = new string[] { "string", "string" };
        //CreateTable("user", colNames, colTypes);

        //使用泛型创建数据表
        //CreateTable<T4>();

        //根据条件查找特定的字段
        //foreach (var item in SelectData("user", new string[] { "name" }, new string[] { "password", "123456" }))
        //{
        //    Debug.Log(item);
        //}

        //更新数据


        //插入数据
        /* string[] values = new string[] { "insert1", "insert2" };
         InsertValues("Test_table", values);*/

        /*
       # region 插入数据
       string[] values = new string[] { "insert1"};
       InsertValues_specifi("Test_table",values);
        foreach (var item in GetDataBySqlQuery("Test_table",new string[] { "id","data"}))
       {
           Debug.Log(item);
           Show_OnText(item);
       }
       #endregion
        */


        //删除数据
        /*
        #region 删除数据
        DeleteValues("Test_table", new string[] { "data","update" });

         foreach (var item in GetDataBySqlQuery("Test_table",new string[] { "id","data"}))
        {
            Debug.Log(item);
            Show_OnText(item);
        }
        #endregion
        */

        //查询数据
        //foreach (var item in GetDataBySqlQuery("T4", new string[] { "name" }))
        //{
        //    Debug.Log(item);
        //}
        /* foreach (var item in GetDataBySqlQuery("Test_table",new string[] { "id","data"}))
         {
             Debug.Log(item);
         }*/



        //使用泛型插入对象
        //T4 t = new T4(2, "22", "222");
        //Insert<T4>(t);

    }

    public void 测试读取数据()
    {
        /*foreach (var item in GetDataBySqlQuery("Sounds_setting",new string[] { "SettingName"}))
         {
             Debug.Log(item);
             textMeshProUGUI.text = item;
             
            // if(item == "yes") print("查找结果为yes");
           //  else  print("查找结果不为yes");
        //     textMeshProUGUI.text+=item+"";
         }*/
    }

    /// <summary>
    /// 删除表
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public SqliteDataReader DeleteTable(string tableName)
    {
        string sql = "DROP TABLE " + tableName;
        return ExecuteQuery(sql);
    }
 
    /// <summary>
    /// 查询整张表的数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public SqliteDataReader SelectFullTableData(string tableName)
    {
        string queryString = "select * from " + tableName;
        return ExecuteQuery(queryString);
    }
 
    /// <summary>
    /// 查询数据
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <param name="fields">需要查找数据</param>
    /// <returns></returns>
    public List<String> GetDataBySqlQuery(string tableName, string[] fields)
    {
        string queryString = "select " + fields[0];
        for (int i = 1; i < fields.Length; i++)
        {
            queryString += " , " + fields[i];
        }
        queryString += " from " + tableName;
       // return ExecuteQuery(queryString);
 
       List<string> list = new List<string>();
       // string queryString = "SELECT * FROM " + tableName;
        reader = ExecuteQuery(queryString);
        while (reader.Read())
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                object obj = reader.GetValue(i);
                list.Add(obj.ToString());
            }
        }
        return list;
    }

    public SqliteDataReader 查询字段返回Reader(string tableName, string[] fields)
    {
        string queryString = "select " + fields[0];
        for (int i = 1; i < fields.Length; i++)
        {
            queryString += " , " + fields[i];
        }
        queryString += " from " + tableName;
       // return ExecuteQuery(queryString);
 
      
       // string queryString = "SELECT * FROM " + tableName;
        reader = ExecuteQuery(queryString);
        return reader;
    }
 
    /// <summary>
    /// 查询数据
    /// </summary>
    /// <param name="tableName">数据表名</param>
    /// <param name="values">需要查询的数据</param>
    /// <param name="fields">查询的条件</param>
    /// <returns></returns>
    public SqliteDataReader SelectData(string tableName, string[] values, string[] fields)
    {
        string sql = "select " + values[0];
        for (int i = 1; i < values.Length; i++)
        {
            sql += " , " + values[i];
        }
        sql += " from " + tableName + " where( ";
        for (int i = 0; i < fields.Length - 1; i += 2)
        {
            sql += fields[i] + " =' " + fields[i + 1] + " 'and ";
        }
        sql = sql.Substring(0, sql.Length - 4) + ");";
        return ExecuteQuery(sql);
 
 
        //用于查看打印
        //List<string> list = new List<string>();
        //reader = ExecuteQuery(sql);
 
        //for (int i = 0; i < reader.FieldCount; i++)
        //{
        //    object obj = reader.GetValue(i);
        //    list.Add(obj.ToString());
        //}
        //return list;
    }
 
 
    /// <summary>
    /// 执行SQL命令
    /// </summary>
    /// <param name="queryString">SQL命令字符串</param>
    /// <returns></returns>
    public SqliteDataReader ExecuteQuery(string queryString)
    {
         // 开始一个事务
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // 创建命令并执行查询
                command = connection.CreateCommand();
                command.CommandText = queryString;
                command.Transaction = transaction; // 将事务关联到命令

                reader = command.ExecuteReader();

                // 提交事务，确保查询结果被保存
                transaction.Commit();

                return reader;
            }
            catch (Exception ex)
            {
                // 如果发生错误，回滚事务
                transaction.Rollback();
                throw new Exception("An error occurred during the query execution.", ex);
            }
         }        
    }
 
    /// <summary>
    /// 创建表(使用泛型)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void CreateTable<T>()
    {
        var type = typeof(T);
        string sql = "create Table " + type.Name + "( ";
        var fields = type.GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            sql += " " + fields[i].Name + " " + CS2DB(fields[i].FieldType) + ",";
        }
        sql = sql.TrimEnd(',') + ")";
        ExecuteQuery(sql);
    }
 
    /// <summary>
    /// CS转化为DB类别
    /// </summary>
    /// <param name="type">c#中字段的类别</param>
    /// <returns></returns>
    string CS2DB(Type type)
    {
        string result = "Text";
        if (type == typeof(Int32))
        {
            result = "Int";
        }
        else if (type == typeof(String))
        {
            result = "Text";
        }
        else if (type == typeof(Single))
        {
            result = "FLOAT";
        }
        else if (type == typeof(Boolean))
        {
            result = "Bool";
        }
        return result;
    }
 
 
    /// <summary>
    /// 创建数据库
    /// </summary>
    /// <param name="tableName">数据库名</param>
    /// <param name="colNames">字段名</param>
    /// <param name="colTypes">字段名类型</param>
    /// <returns></returns>
    public SqliteDataReader CreateTable(string tableName, string[] colNames, string[] colTypes)
    {
        string queryString = "create table " + tableName + "(" + colNames[0] + " " + colTypes[0];
        for (int i = 1; i < colNames.Length; i++)
        {
            queryString += ", " + colNames[i] + " " + colTypes[i];
        }
        queryString += " )";
 
        Debug.Log("添加成功");
        return ExecuteQuery(queryString);
    }
 
    /// <summary>
    /// 向指定数据表中插入数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public SqliteDataReader InsertValues(string tableName, string[] values)
    {
        string sql = "INSERT INTO " + tableName + " values (";
        foreach (var item in values)
        {
            sql += "'" + item + "',";
        }
        sql = sql.TrimEnd(',') + ")";
 
        Debug.Log("插入成功");
        return ExecuteQuery(sql);
    }


     public SqliteDataReader InsertValues_specifi(string tableName, string[] values)
    {
        string sql = "INSERT INTO " + tableName +"(data)"+  " values (";
        foreach (var item in values)
        {
            sql += "'" + item + "',";
        }
        sql = sql.TrimEnd(',') + ")";
 
        Debug.Log("插入成功");
        return ExecuteQuery(sql);
    }
 
    /// <summary>
    /// 插入数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public SqliteDataReader Insert<T>(T t)
    {
        var type = typeof(T);
        var fields = type.GetFields();
        string sql = "INSERT INTO " + type.Name + " values (";
 
        foreach (var field in fields)
        {
            //通过反射得到对象的值
            sql += "'" + type.GetField(field.Name).GetValue(t) + "',";
        }
        sql = sql.TrimEnd(',') + ");";
 
        Debug.Log("插入成功");
        return ExecuteQuery(sql);
    }
 
 
    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="values">需要修改的数据</param>
    /// <param name="conditions">修改的条件</param>
    /// <returns></returns>
    public SqliteDataReader UpdataData(string tableName, string[] values, string[] conditions)
    {
        string sql = "update " + tableName + " set ";
        for (int i = 0; i < values.Length - 1; i += 2)
        {
            sql += values[i] + "='" + values[i + 1] + "',";
        }
        sql = sql.TrimEnd(',') + " where (";
        for (int i = 0; i < conditions.Length - 1; i += 2)
        {
            sql += conditions[i] + "='" + conditions[i + 1] + "' and ";
        }
        sql = sql.Substring(0, sql.Length - 4) + ");";
        Debug.Log("更新成功");
        return ExecuteQuery(sql);
    }

      public SqliteDataReader UpdataDataAll(string tableName, string[] values)
    {
        string sql = "update " + tableName + " set ";
        for (int i = 0; i < values.Length - 1; i += 2)
        {
            sql += values[i] + "='" + values[i + 1] + "',";
        }
         sql = sql.TrimEnd(',', ' ');
         sql += ";";
        Debug.Log("更新成功");
        return ExecuteQuery(sql);
    }
 
 
    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="conditions">查询条件</param>
    /// <returns></returns>
    public SqliteDataReader DeleteValues(string tableName, string[] conditions)
    {
        string sql = "delete from " + tableName + " where (";
        for (int i = 0; i < conditions.Length - 1; i += 2)
        {
            sql += conditions[i] + "='" + conditions[i + 1] + "' and ";
        }
        sql = sql.Substring(0, sql.Length - 4) + ");";
        return ExecuteQuery(sql);
    }
 
    //打开数据库
    public void OpenConnect()
    {
        try
        {
            //数据库存放在 Asset/StreamingAssets
            string path =  dbName ;//就是subspaceHunter 设置为Privatel了
          //  Show_OnTextDebug("enter_connect");
            //新建数据库连接
            connection = new SqliteConnection(GetDataPath(path));
           // Show_OnTextDebug("新建链接");
            //打开数据库
            connection.Open();
        
           // Show_OnTextDebug("打开数据库");
            Debug.Log("打开数据库");
        }
        catch (Exception ex)
        {
           // Show_OnTextDebug(ex.ToString());
            Debug.Log(ex.ToString());
        }
       
    }

   


       public string GetDataPath(string databasePath)
        {
       
       
            #if UNITY_EDITOR     
               return "data source=" + Application.streamingAssetsPath + "/" + databasePath;
           #elif UNITY_ANDROID
               return  connectString;//"URI=file:" + Application.persistentDataPath + "/" + databasePath;
            #endif

            #if UNITY_IOS
               return "data source=" + Application.persistentDataPath + "/" + databasePath;
            #endif
        }

    public static string connectString;
    public IEnumerator copyDbFile()//将StreamingAsset 拷贝到persistent path下
    {
        string dataSandBoxPath = Application.persistentDataPath + "/"+dbName;
        Debug.Log(dataSandBoxPath);
        if (File.Exists(dataSandBoxPath))//存在文件直接返回
        {
           textMeshProUGUI2.text += "文件是存在的";
            yield return null;
        }
         else{     
                if (!Directory.Exists(Application.persistentDataPath))
                {
                    textMeshProUGUI2.text += "持久化目录不能存在，创建持久化目录";
                    Directory.CreateDirectory(Application.persistentDataPath);
                }
                WWW loadWWW = new WWW(Path.Combine(Application.streamingAssetsPath,dbName));
            // Debug.Log(Path.Combine(Application.streamingAssetsPath, dbName));
                yield return loadWWW;
                File.WriteAllBytes(dataSandBoxPath, loadWWW.bytes);///如何判断是否写完？
                textMeshProUGUI2.text += "迁移文件";
                //Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
               } 
         connectString = "URI=file:" + dataSandBoxPath;
                //SQLTest.instance.p();

    }

    
           //关闭数据库
        public void CloseDB()
    {
        if (command != null)
        {
            command.Cancel();
        }
        command = null;
 
        if (reader != null)
        {
            reader.Close();
        }
        reader = null;
 
        if (connection != null)
        {
            //connection.Close();
        }
        connection = null;
 
        Debug.Log("关闭数据库");
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene("AR_tourNew");
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Sqlite_test");
        }
    }

}
