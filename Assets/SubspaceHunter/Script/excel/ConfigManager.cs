/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 配置表入口 / Configuration table entry
 * 功能 / Purpose: 读取 Luban/Excel 生成配置，向运行时代码提供表格数据入口。
 * English: Reads Luban/Excel generated configuration and exposes table data to runtime code.
 */

using GameConfig;
using Luban;
using UnityEngine;



    public static class GameConfigMgr
    {
        private static Tables _tables;
        public static Tables Tables => _tables;

        public static void Load()
        {
            _tables = new Tables(LoadByteBuf);
        }

        /// <summary>
        /// 加载二进制配置。
        /// </summary>
        /// <param name="file">FileName</param>
        /// <returns>ByteBuf</returns>
        private static ByteBuf LoadByteBuf(string file)
        {
            TextAsset textAsset = Resources.Load<TextAsset>("Configs/bytes/"+file);
            byte[] bytes = textAsset.bytes;
            Resources.UnloadAsset(textAsset);
            return new ByteBuf(bytes);
        }

    }
