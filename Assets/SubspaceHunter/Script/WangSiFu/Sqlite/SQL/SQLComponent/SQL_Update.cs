/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: SQLite 语句组件 / SQLite statement component
 * 功能 / Purpose: 封装 SQLite 增删改查、建表和编辑语句片段。
 * English: Wraps SQLite create, insert, select, update, delete, alter, and editor statement fragments.
 */

using Mono.Data.Sqlite;


public static partial class SQLComponent
{
    public static SqliteDataReader UpdataData(string tableName, string[] values, string[] conditions)
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
        
        return ExecuteQuery(sql);
    }

}
