using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class UILuaComBinding
{
    private static string ReadLuaFile(string luaPath)
    {
        string luaCode = string.Empty;

        if (string.IsNullOrEmpty(luaCode))
        {
            return luaCode;
        }

        if (File.Exists(luaPath))
        {
            luaCode = File.ReadAllText(luaPath);
        }

        return luaCode;
    }

    private static List<string> GetSelfFields(string luaCode)
    {
        HashSet<string> fields = new();
        string pattern = @"self\.([a-zA-Z0-9_]+)(?=:|\.)";
        foreach (Match match in Regex.Matches(luaCode, pattern))
        {
            if (!match.Success)
            {
                continue;
            }

            string fieldName = match.Groups[1].Value;
            if (fieldName.StartsWith("m_"))
            {
                fields.Add(fieldName);
            }
        }

        return fields.ToList();
    }

    private static List<string> GetSelfArrayFields(string luaCode)
    {
        HashSet<string> fields = new();
        string[] patterns =
        {
            @"self\[""([a-zA-Z0-9_]+)[^""]*?(?=""(\s)*\.\.)",
            @"self\[\'([a-zA-Z0-9_]+)[^""]*?(?=\'(\s)*\.\.)",
        };
        foreach (var pattern in patterns)
        {
            foreach (Match match in Regex.Matches(luaCode, pattern))
            {
                if (!match.Success)
                {
                    continue;
                }

                string fieldName = match.Groups[1].Value;
                if (fieldName.StartsWith("m_"))
                {
                    fields.Add(fieldName);
                }
            }
        }

        return fields.ToList();
    }

    private static LuaComGroup GenerateLuaComGroup(Transform holder, List<string> fields, List<string> arrayFields)
    {
        LuaComGroup group = new LuaComGroup();
        group.Name = "auto-generate";

        List<LuaCom> coms = new();
        RectTransform[] candidates = holder.GetComponentsInChildren<RectTransform>(true);

        // 普通字段
        for (int i = fields.Count - 1; i >= 0; i--)
        {
            string field = fields[i];
            bool searched = false;

            // 精确匹配
            foreach (var candidate in candidates)
            {
                if (candidate.name.Equals(field, StringComparison.OrdinalIgnoreCase))
                {
                    coms.Add(new LuaCom()
                        { Name = field, ComObj = candidate, Type = LuaComType.RectTransform, m_InstanceId = candidate.gameObject.GetInstanceID() });
                    searched = true;
                    fields.RemoveAt(i);
                    break;
                }
            }

            if (searched)
            {
                continue;
            }

            // 模糊匹配
            foreach (var candidate in candidates)
            {
                if (candidate.name.StartsWith(field, StringComparison.OrdinalIgnoreCase))
                {
                    coms.Add(new LuaCom()
                        { Name = field, ComObj = candidate, Type = LuaComType.RectTransform, m_InstanceId = candidate.gameObject.GetInstanceID() });
                    fields.RemoveAt(i);
                    break;
                }
            }
        }

        // 数组类型字段
        for (int i = arrayFields.Count - 1; i >= 0; i--)
        {
            string field = fields[i];
            bool searched = false;

            foreach (var candidate in candidates)
            {
                if (candidate.name.StartsWith(field, StringComparison.OrdinalIgnoreCase))
                {
                    coms.Add(new LuaCom() { Name = candidate.name, ComObj = candidate, Type = LuaComType.RectTransform });
                    searched = true;
                }
            }

            if (searched)
            {
                fields.RemoveAt(i);
            }
        }

        group.LuaComs = coms.ToArray();
        return group;
    }

    public static void HandleErrMessage(Transform holder, List<string> fields, List<string> arrayFields)
    {
        bool err1 = fields.Count > 0;
        bool err2 = arrayFields.Count > 0;
        if (err1)
        {
            Debug.LogError($"prefab: {holder.name} 部分普通字段未找到对应组件: {string.Join(",", fields)}");
        }

        if (err2)
        {
            Debug.LogError($"prefab: {holder.name} 部分普通字段未找到对应组件: {string.Join(",", arrayFields)}");
        }

        if (err1 || err2)
        {
            EditorUtility.DisplayDialog("提示", "部分普通字段未找到对应组件 详见Console窗口", "OK");
        }
    }

    
    public static LuaComGroup[] AutoBindCompontent(Transform holder, string luaPath, LuaComGroup[] groups)
    {
        string luaCode = ReadLuaFile(luaPath);
        List<string> fields = GetSelfFields(luaCode);
        List<string> arrayFields = GetSelfArrayFields(luaCode);
        LuaComGroup group = GenerateLuaComGroup(holder, fields, arrayFields);
        LuaComGroup[] results = new LuaComGroup[groups.Length + 1];
        Array.Copy(groups, results, groups.Length);
        results[groups.Length] = group;
        HandleErrMessage(holder, fields, arrayFields);
        return results;
    }

    public static void ExecuteAutoCombine()
    {
        
    }

}
