/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: SQLite 语句组件 / SQLite statement component
 * 功能 / Purpose: 封装 SQLite 增删改查、建表和编辑语句片段。
 * English: Wraps SQLite create, insert, select, update, delete, alter, and editor statement fragments.
 */

using Mono.Data.Sqlite;

public static partial class SQLComponent
{
    public static void CreateTable<T>()
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
    public static SqliteDataReader CreateTable(string tableName, string[] colNames, string[] colTypes)
    {
        string queryString = "create table " + tableName + "(" + colNames[0] + " " + colTypes[0];
        for (int i = 1; i < colNames.Length; i++)
        {
            queryString += ", " + colNames[i] + " " + colTypes[i];
        }
        queryString += " )";

        return ExecuteQuery(queryString);
    }
}
