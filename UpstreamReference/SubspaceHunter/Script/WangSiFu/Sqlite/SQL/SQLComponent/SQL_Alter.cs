/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: SQLite 语句组件 / SQLite statement component
 * 功能 / Purpose: 封装 SQLite 增删改查、建表和编辑语句片段。
 * English: Wraps SQLite create, insert, select, update, delete, alter, and editor statement fragments.
 */

using Mono.Data.Sqlite;

/// <summary>
/// https://www.w3schools.com/sql/sql_alter.asp
/// </summary>


public static partial class SQLComponent
{
    public static SqliteDataReader AddColumn(string tableName, string columnName, string dataType)
    {
        string sql = "ALTER TABLE " + tableName + " ADD "+columnName+" "+dataType;
        

        return ExecuteQuery(sql);
    }

}
