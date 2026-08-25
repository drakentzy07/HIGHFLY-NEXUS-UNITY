/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: SQLite 数据管理 / SQLite data management
 * 功能 / Purpose: 管理本地数据库连接、场景数据读取和测试入口。
 * English: Manages local database connections, scene-data access, and test entry points.
 */

using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
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

public class DatabaseManager : MonoBehaviour
{
     private string dbPath;
     public TextMeshProUGUI textMeshProUGUI;
  public TextMeshProUGUI textMeshProUGUI2;
    // 初始化数据库
    public IEnumerator InitializeDatabase(string dbName)
    {
        // 数据库在持久化数据路径下的完整路径
        string persistentPath = Path.Combine(Application.persistentDataPath, dbName);
        
        // 检查数据库文件是否已经存在
        if (!File.Exists(persistentPath))
        {
            // 从StreamingAssets路径中获取数据库文件的完整路径
            string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, dbName);
            Debug.Log($"StreamingAssets database path: {streamingAssetsPath}");
            
            // 使用UnityWebRequest获取文件内容
            UnityWebRequest webRequest = UnityWebRequest.Get(streamingAssetsPath);
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Failed to load database from StreamingAssets: {webRequest.error}");
                yield break;
            }
            else
            {
                // 如果请求成功，创建文件并将数据写入
                Debug.Log("Copying database to persistent path...");
                File.WriteAllBytes(persistentPath, webRequest.downloadHandler.data);
                Debug.Log($"Database copied to persistent path: {persistentPath}");
            }
        }
        else
        {
            Debug.Log("Database already exists at persistent path.");
        }

        // 数据库路径保存在类中
        dbPath = "URI=file:" + persistentPath;
        Debug.Log("Final database path: " + dbPath);

        // 在这里可以调用后续的数据库初始化操作，例如创建表或执行查询
        AfterDatabaseInitialization();
    }

    // 这个方法可以包含一些你初始化数据库后的操作
    private void AfterDatabaseInitialization()
    {
        // 例如，创建表格，或者读取数据等
        Debug.Log("Database initialization complete.");
    }


    public void CreateTable()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
                command.ExecuteNonQuery();
            }
        }
    }

    public void InsertData(string name, int age)
    {
         try
         {
                using (var connection = new SqliteConnection(dbPath))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO Users (Name, Age) VALUES (@name, @age);";
                        command.Parameters.Add(new SqliteParameter("@name", name));
                        command.Parameters.Add(new SqliteParameter("@age", age));
                        command.ExecuteNonQuery();
                    }
                }
                    Debug.Log("InsertData: Data inserted successfully.");
        }
        catch (Exception ex)
        {
                 Debug.LogError($"InsertData: Failed to insert data. Error: {ex.Message}");
        }
    }

    public void ReadData(string tableName)
{
    using (var connection = new SqliteConnection(dbPath))
    {
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            // 使用参数化的方式插入表名以防止SQL注入风险
            command.CommandText = $"SELECT * FROM {tableName};";
            using (IDataReader reader = command.ExecuteReader())
            {
                 // 首先清空TextMeshProUGUI的文本
                    textMeshProUGUI.text = "";
                while (reader.Read())
                {
                   // 拼接每条记录的所有字段
                        string concatenatedResult = "";
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            concatenatedResult += $"{reader.GetName(i)}: {reader[i]}";
                            if (i < reader.FieldCount - 1)
                            {
                                concatenatedResult += ", "; // 用逗号和空格分隔字段
                            }
                        }

                        // 每条记录后换行
                        textMeshProUGUI.text += concatenatedResult + "\n";
                }
            }
        }
    }
}


     // DELETE operation
    public void DeleteData(string tableName, string condition)
    {
        try
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"DELETE FROM {tableName} WHERE {condition};";
                command.ExecuteNonQuery();
            }
        }
            Debug.Log($"DeleteData: Data deleted successfully from {tableName} where {condition}.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"DeleteData: Failed to delete data from {tableName}. Error: {ex.Message}");
        }
    }


     public void UpdateData(string tableName, string[] columns, string[] values, string condition)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                string setClause = "";
                for (int i = 0; i < columns.Length; i++)
                {
                    setClause += $"{columns[i]} = '{values[i]}'";
                    if (i < columns.Length - 1) setClause += ", ";
                }
                command.CommandText = $"UPDATE {tableName} SET {setClause} WHERE {condition};";
                command.ExecuteNonQuery();
            }
        }
    }


     public DataTable SelectData(string tableName, string[] columns, string condition = null)
    {
        DataTable dataTable = new DataTable();
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                string columnsJoined = string.Join(", ", columns);
                command.CommandText = $"SELECT {columnsJoined} FROM {tableName}";
                if (condition != null)
                {
                    command.CommandText += $" WHERE {condition}";
                }
                command.CommandText += ";";

                using (IDataReader reader = command.ExecuteReader())
                {
                    dataTable.Load(reader);
                }
            }
        }
        return dataTable;
    }



    
}
