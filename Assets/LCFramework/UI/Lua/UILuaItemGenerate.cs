using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

using System.Text;
#endif

#if ODIN_INSPECTOR
#endif

#if UNITY_EDITOR

#region CodeOverwrite
// 代码覆盖
// 基于语法解析的覆盖肯定很麻烦，所以这里使用行解析的方法来进行处理吧。
// 主要思路是：Time追加
// 
public class LuaItemCodeOverwrite
{
    private string luaPath;

    // 导出列表注释
    private string memberList;
    // 导出事件列表
    private string addEventList;
    private string removeEventList;
    private string eventCallbackList;
    // 文本控件处理
    private string textLocalList;
    private string otherLocalList;

    // 段文字
    private Dictionary<string, string> blockText = new Dictionary<string, string>();

    // prefab路径
    private string prefabPath;
    // 最终输出路径
    private string outPath;
    // 类名
    private string className;
    // 要导出的列表
    private UILuaItem target;

    // 当前要覆盖的文件，每行文本列表
    private List<string> luaLineText = new List<string>();

    // 本次的object列表
    private List<string> thisObjList = new List<string>();

    // 当前文件的function列表,
    // string = function name
    // int = line number
    class Func
    {
        public string name;
        public int lineno;
        public int endno;
        public bool has;
    }
    private List<Func> eventFuncs = new List<Func>();

    // 最大的函数行
    private int maxFuncLine = 0;


    public void replaceBlock(ref string outtext)
    {
        blockText["${LuaPath}"] = luaPath;
        blockText["${AssetPath}"] = prefabPath;
        blockText["${DateTime}"] = DateTime.Now.ToLocalTime().ToString();
        blockText["${ClassName}"] = className;
        blockText["${ViewComment}"] = (target.uiComment != null) ? target.uiComment : "";

        //System.Security.Principal.WindowsIdentity windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
        //blockText["${Creator}"] = windowsIdentity.Name;
        blockText["${Creator}"] = Environment.UserName;
        //${ControlCommentBlock}

        foreach (var t in blockText)
        {
            outtext = outtext.Replace(t.Key, t.Value);
        }

        return;
    }

    // 把文件按照读行的方式读取到内存
    public void readLines(string f)
    {
        string line;
        System.IO.StreamReader file = new System.IO.StreamReader(f);
        while ((line = file.ReadLine()) != null)
        {
            luaLineText.Add(line);
        }

        file.Close();

        return;
    }

    // 挑出所有的function
    public void getFunctions()
    {
        for (int lineNo = 0; lineNo < luaLineText.Count;)
        {
            string line = luaLineText[lineNo].Trim();

            // 简单用字符串方式来比较是否为event function
            if (line.StartsWith("function "))
            {
                lineNo = parseOKFunc(lineNo);
            }
            else if (line == "-- Datetime:")
            {
                Func f = new Func();
                f.name = "Datetime";
                f.lineno = lineNo;
                f.endno = lineNo;
                f.has = true;
                eventFuncs.Add(f);
                ++lineNo;
            }
            else
            {
                ++lineNo;
            }
        }

    }

    // 处理是我们需要的函数
    public int parseOKFunc(int lineNo)
    {
        bool eventFunc = false;
        string line = luaLineText[lineNo].Trim();

        Func f = new Func();
        f.name = null;

        if (line.IndexOf(":InitControl") > 0)
        {
            f.name = "InitControl";
            f.lineno = lineNo;
            f.has = true;
        }
        else if (line.IndexOf(":RegisterUIEvent") > 0)
        {
            f.name = "RegisterUIEvent";
            f.lineno = lineNo;
            f.has = true;

        }
        else if (line.IndexOf(":UnregisterUIEvent") > 0)
        {
            f.name = "UnregisterUIEvent";
            f.lineno = lineNo;
            f.has = true;
        }

        else if (line.IndexOf(":onClick_") > 0 || line.IndexOf(":onValueChanged_") > 0)
        {
            int beginPos = line.IndexOf(":onClick_");
            if (beginPos < 0)
            {
                beginPos = line.IndexOf(":onValueChanged_");
            }
            beginPos += 1;


            int endPos = line.IndexOf("(");
            if (beginPos > 0 && endPos > beginPos)
            {
                string fname = line.Substring(beginPos, endPos - beginPos);
                f.name = fname;
                f.lineno = lineNo;
                f.has = false;

                eventFunc = true;
            }
        }

        // 简单从function找到end即可。
        if (!string.IsNullOrEmpty(f.name))
        {
            for (int j = lineNo; j < luaLineText.Count; ++j)
            {
                string lineEnd = luaLineText[j];
                if (lineEnd.StartsWith("end"))
                {
                    if (f.name == "RegisterUIEvent")
                    {
                        f.endno = j;
                    }
                    else
                    {
                        int row = j;

                        f.endno = row;

                        while (string.IsNullOrEmpty(luaLineText[--row].Trim()))
                            f.endno = row;
                    }

                    eventFuncs.Add(f);

                    if (eventFunc)
                    {
                        if (j > maxFuncLine)
                        {
                            maxFuncLine = j;    // 记录最大行。。。
                        }
                    }

                    return j + 1;
                }
            }

        }

        return lineNo + 1;
    }

    // 是否有一行
    public bool hasLine(string line2)
    {
        for (int lineNo = 0; lineNo < luaLineText.Count; ++lineNo)
        {
            string line = luaLineText[lineNo].Trim();
            if (line == line2)
            {
                return true;
            }
        }

        return false;
    }

    // 检测事件行
    public bool checkEventFunc(string line2, string funcname)
    {
        for (int lineNo = 0; lineNo < luaLineText.Count; ++lineNo)
        {
            string line = luaLineText[lineNo].Trim();
            if (line.Contains(line2.Trim()))
            {
                for (int i = 0; i < eventFuncs.Count; ++i)
                {
                    if (eventFuncs[i].name.Contains(funcname))
                    {
                        eventFuncs[i].has = true;
                    }
                }
                return true;
            }
        }

        return false;
    }

    public bool hasFunction(string funcname)
    {
        for (int i = 0; i < eventFuncs.Count; ++i)
        {
            if (eventFuncs[i].name == funcname)
            {
                return true;
            }
        }
        return false;
    }

    public bool findFunc(string fname, out int beginPos, out int endPos)
    {
        for (int i = 0; i < eventFuncs.Count; ++i)
        {
            if (eventFuncs[i].name == fname)
            {
                beginPos = eventFuncs[i].lineno;
                endPos = eventFuncs[i].endno;

                return true;
            }
        }

        beginPos = -1;
        endPos = -1;
        return false;
    }

    // 有些没有导出按钮，那么就没有事件函数，这个时候要寻找--------------------------------------------------
    public void fix_maxFuncLine()
    {
        if (maxFuncLine > 0)
        {
            return;
        }

        string retstr = "return " + className;

        for (int j = 0; j < luaLineText.Count; ++j)
        {
            string lineEnd = luaLineText[j];

            if (lineEnd == "--------------------------------------------------")
            {
                maxFuncLine = j;
                return;
            }


            if (lineEnd.StartsWith(retstr))
            {
                maxFuncLine = j - 1;
            }

        }

    }

    public void generate(UILuaItem com, string path)
    {
        // 直接通过相关的工具函数获取到现在编辑的stage！但是这个方法在2019貌似已经废弃了。。。
        // 如果要修改，可以把root通过参数传过来。
        UnityEditor.SceneManagement.PrefabStage stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null)
        {
            return;
        }

        luaPath = path;
        prefabPath = stage.assetPath;
        target = com;
        outPath = path;
        className = Path.GetFileNameWithoutExtension((path.Length > 0) ? path : prefabPath);

        // 读取文件
        readLines(luaPath);

        getFunctions();

        // 从选中的项，开始dump
        GameObject root = Selection.activeObject as GameObject;
        IteratorChildren(root.name);

        parserSpecialFunc();
        fix_maxFuncLine();


        string finalText = getFormat();


        //string funcText = getFuncs();
        //File.WriteAllText(path + ".func.txt", funcText);


        // replaceBlock(ref finalText);

        //WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
        finalText = finalText.Replace("${DateTime}", DateTime.Now.ToLocalTime().ToString());
        //finalText = finalText.Replace("${Creator}", windowsIdentity.Name);
        finalText = finalText.Replace("${Creator}", Environment.UserName);

        // 替换控件列表
        finalText = finalText.Replace("${ControlCommentBlock}", memberList);

        // 替换事件
        finalText = finalText.Replace("${RegisterEvent}", addEventList);
        finalText = finalText.Replace("${UnregisterEvent}", removeEventList);
        finalText = finalText.Replace("${EventCallbackList}", eventCallbackList);

        //// 文本本地化
        //finalText = finalText.Replace("${TextLocal}", textLocalList);
        //finalText = finalText.Replace("${OtherLocal}", otherLocalList);


        File.WriteAllText(path, finalText);

        return;
    }

    public string getFuncs()
    {
        string fullText = "";

        foreach (var f in eventFuncs)
        {
            string s = string.Format("{0}   {1}-{2}   {3}", f.name, f.lineno, f.endno, f.has);
            fullText += s;
            fullText += "\n";
        }

        return fullText;
    }

    public string getFormat()
    {
        //string fullText = "";
        StringBuilder sb = new StringBuilder();

        for (int j = 0; j < luaLineText.Count; ++j)
        {
            //if (checkLine(j) == false)
            //{
            //    sb.Append("-- ");
            //}
            //else if (checkRegisterLine(j) == false)
            //{
            //    string line = luaLineText[j];
            //    sb.Append("-- ");
            //}

            sb.Append(luaLineText[j]);
            sb.Append("\n");

            //hasFunction
        }

        return sb.ToString();
    }

    // 从has=false的函数中，找到相应行是否为false
    public bool checkLine(int line)
    {
        foreach (var f in eventFuncs)
        {
            if (f.has == false)
            {
                if (line >= f.lineno && line <= f.endno)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // 检测RegisterUIEvent和UnregisterUIEvent里面的有效行
    public bool checkRegisterLine(int lineno)
    {
        int beginLine1 = -1;
        int endLine1 = -1;

        int beginLine2 = -1;
        int endLine2 = -1;
        findFunc("RegisterUIEvent", out beginLine1, out endLine1);
        findFunc("UnregisterUIEvent", out beginLine2, out endLine2);

        bool inLine = false;
        if ((lineno >= beginLine1 && lineno <= endLine1) ||
                (lineno >= beginLine2 && lineno <= endLine2))
        {
            inLine = true;
        }
        else
        {
            return true;
        }

        string str = luaLineText[lineno];
        int beginPos = str.IndexOf(".");
        if (beginPos < 0)
        {
            return true;
        }

        beginPos += 1;
        int endPos = str.IndexOf(".", beginPos);
        if (endPos < 0)
        {
            return true;
        }

        string objname = str.Substring(beginPos, endPos - beginPos);
        if (hasObject(objname) == false)
        {
            return false;
        }

        return true;
    }

    public bool hasObject(string objName)
    {
        foreach (var l in thisObjList)
        {
            if (l == objName)
            {
                return true;
            }
        }

        return false;
    }

    public void IteratorChildren(string parentName)
    {
        string nodePath = string.Empty;
        string exportName = string.Empty;

        StringBuilder lineBuilder = new StringBuilder();

        LuaComType type = LuaComType.Unknown;

        GameObject tempObj = null;

        UILuaItem t = (UILuaItem)target;
        for(int i = 0; i < t.LuaComGroups.Length; i++)
        {
            for (int j = 0; j < t.LuaComGroups[i].LuaComs.Length; ++j)
            {
                lineBuilder.Clear();
                lineBuilder.Append("\t-- ");
                
                exportName = t.LuaComGroups[i].LuaComs[j].Name;
                type = t.LuaComGroups[i].LuaComs[j].Type;
                nodePath = t.LuaComGroups[i].LuaComs[j].ComObj.name;
                tempObj = t.LuaComGroups[i].LuaComs[j].GetGameObject();

                Component component = t.LuaComGroups[i].LuaComs[j].ComObj as Component;
                if (null != component)
                {
                    Transform point = component.transform;
                    while (point.parent != null)
                    {
                        point = point.parent;
                        nodePath = point.name + "/" + nodePath;

                        if (point.name == parentName)
                            break;
                    }

                    // 这里使用他导出的名字，而不使用原始名字；因为导出的名字是可以修改的
                    lineBuilder.AppendFormat("[{0}] {1} = {2}\n", type.ToString(), exportName, nodePath);

                    if (hasLine(lineBuilder.ToString().Trim()) == false)
                    {
                        memberList += lineBuilder.ToString();
                    }
                }
            }
        }

        if (type == LuaComType.CDButton)
        {
            string ss = string.Format("onClick_{0}", exportName);
            thisObjList.Add(exportName);

            string finder = "\tself.${ButtonName}.onClick:AddListener(";
            finder = finder.Replace("${ButtonName}", exportName);
            if (checkEventFunc(finder.Trim(), ss) == false)
            {
                string format = "\tself.${ButtonName}.onClick:RemoveAllListeners()\n";
                format += "\tself.${ButtonName}.onClick:AddListener(function()\n";
                format += "\t\tself:onClick_${ButtonName}()\n";
                format += "\tend)";
                string line = format.Replace("${ButtonName}", exportName);
                addEventList += line;

                string format3 = "\tself.${ButtonName}.onClick:RemoveAllListeners()";
                string line3 = format3.Replace("${ButtonName}", exportName);
                removeEventList += line3;

                string format2 = "function M:onClick_${ButtonName}()\n\nend";
                string line2 = format2.Replace("${ButtonName}", exportName);
                line2 = line2.Replace("${ClassName}", className);
                eventCallbackList += line2;
            }
        }
        else if (type == LuaComType.Toggle || type == LuaComType.Slider)
        {
            string ss = string.Format("onValueChanged_{0}", exportName);
            thisObjList.Add(exportName);

            string finder = "\tself.${ToggleName}.onValueChanged:AddListener(";
            finder = finder.Replace("${ToggleName}", exportName);
            if (checkEventFunc(finder.Trim(), ss) == false)
            {
                string format = "\tself.${ToggleName}.onValueChanged:RemoveAllListeners()\n";
                format += "\tself.${ToggleName}.onValueChanged:AddListener(function()\n";
                format += "\t\tself:onValueChanged_${ToggleName}\n";
                format += "\tend)";
                string line = format.Replace("${ToggleName}", exportName);
                addEventList += line;

                string format3 = "\tself.${ToggleName}.onValueChanged:RemoveAllListeners()\n";
                string line3 = format3.Replace("${ToggleName}", exportName);
                removeEventList += line3;

                string format2 = "function M:onValueChanged_${ToggleName}()\n\nend\n\n";
                string line2 = format2.Replace("${ToggleName}", exportName);
                line2 = line2.Replace("${ClassName}", className);
                eventCallbackList += line2;
            }
        }
        else if (type == LuaComType.IMTextMeshProUGUI)
        {
            string format = "\t -- ${TextName}:SetText({\"\"});\n";
            string line = format.Replace("${TextName}", exportName);

            if (hasLine(line.Trim()) == false)
            {
                textLocalList += line;
            }
        }
    }

    // parserSpecialFunc，这样方便追加
    // 这里简单做了一下处理,,,因为处理的超级土，所以这代码不知道谁写的。
    public void parserSpecialFunc()
    {
        int beginLine = -1;
        int endLine = -1;
        if (findFunc("RegisterUIEvent", out beginLine, out endLine))
        {
            string newLine = luaLineText[endLine - 1] + "\n${RegisterEvent}\n";
            luaLineText[endLine - 1] = newLine;
        }

        if (findFunc("UnregisterUIEvent", out beginLine, out endLine))
        {
            string newLine = luaLineText[endLine - 1] + "\n${UnregisterEvent}\n";
            luaLineText[endLine - 1] = newLine;
        }


        if (findFunc("TextLocalization", out beginLine, out endLine))
        {
            string newLine = luaLineText[endLine - 1] + "\n${TextLocal}\n";
            luaLineText[endLine - 1] = newLine;
        }

        // 怎么着最后一行也应该是return，所以这里就不判断越界了
        if (maxFuncLine > 0)
        {
            string newLine = luaLineText[maxFuncLine + 1] + "\n\n${EventCallbackList}\n";
            luaLineText[maxFuncLine + 1] = newLine;
        }

        if (findFunc("Datetime", out beginLine, out endLine))
        {
            string newLine = luaLineText[beginLine] + "\n--     ${Creator} - ${DateTime}";
            luaLineText[beginLine] = newLine;
        }

    }
}

#endregion

#region LuaCodeGen

// 代码生成
public class LuaItemCodeGen
{
    private string luaPath;
    // 生成的code类型
    private int codeType = 0;

    // 导出列表注释
    private string memberList;
    // 导出事件列表
    private string addEventList;
    private string removeEventList;
    private string eventCallbackList;
    // 文本控件处理
    private string textLocalList;
    private string otherLocalList;

    // 段文字
    private Dictionary<string, string> headText = new Dictionary<string, string>();

    // prefab路径
    private string prefabPath;
    // 最终输出路径
    private string outPath;
    // 类名
    private string className;
    // 要导出的列表
    private UILuaItem target;

    private string assetFlag = "Assets";

    public void replaceFileHead(ref string outtext)
    {
        headText["${LuaPath}"] = this.GetAssetPath();
        headText["${AssetPath}"] = prefabPath;
        headText["${DateTime}"] = DateTime.Now.ToLocalTime().ToString();
        headText["${ClassName}"] = className;
        headText["${MarkClassName}"] = string.Format("\"{0}\"", className);
        headText["${MarkUILuaItem}"] = string.Format("\"{0}\"", "UILuaItem");
        headText["${ViewComment}"] = (target.uiComment != null) ? target.uiComment : "";

        //WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
        //blockText["${Creator}"] = windowsIdentity.Name;
        headText["${Creator}"] = Environment.UserName;
        //${ControlCommentBlock}

        foreach (var t in headText)
            outtext = outtext.Replace(t.Key, t.Value);

        return;
    }

    private string GetAssetPath()
    {
        string assetPath = this.luaPath;
        int index = this.luaPath.IndexOf(assetFlag);
        if (index > 0)
            assetPath = this.luaPath.Substring(index);

        return assetPath;
    }

    public void generate(UILuaItem com, string path)
    {
        // 如果要修改，可以把root通过参数传过来。
        UnityEditor.SceneManagement.PrefabStage stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null)
        {
            return;
        }

        GameObject root = Selection.activeObject as GameObject;

        luaPath = path;
        prefabPath = stage.assetPath;
        target = com;
        outPath = path;
        className = root.name;

        // 从选中的项，开始dump
        IteratorChildren(className);

        string finalText = getItemFormat();

        replaceFileHead(ref finalText);

        // 替换控件列表
        finalText = finalText.Replace("${ControlCommentBlock}", this.memberList);

        // 替换事件
        finalText = finalText.Replace("${RegisterEvent}", addEventList);
        finalText = finalText.Replace("${UnregisterEvent}", removeEventList);
        finalText = finalText.Replace("${EventCallbackList}", eventCallbackList);

        // 文本本地化
        finalText = finalText.Replace("${TextLocal}", textLocalList);
        finalText = finalText.Replace("${OtherLocal}", otherLocalList);


        File.WriteAllText(path, finalText);

        return;
    }

    public void IteratorChildren(string parentName)
    {
        this.memberList = "";

        string nodePath = string.Empty;
        string exportName = string.Empty;

        StringBuilder lineBuilder = new StringBuilder();

        LuaComType type = LuaComType.Unknown;

        UILuaItem t = (UILuaItem)target;
        for(int i = 0; i < t.LuaComGroups.Length; i++)
        {
            for (int j = 0; j < t.LuaComGroups[i].LuaComs.Length; ++j)
            {
                lineBuilder.Clear();
                lineBuilder.Append("\t-- ");

                exportName = t.LuaComGroups[i].LuaComs[j].Name;
                type = t.LuaComGroups[i].LuaComs[j].Type;
                nodePath = t.LuaComGroups[i].LuaComs[j].ComObj.name;

                Component component = t.LuaComGroups[i].LuaComs[j].ComObj as Component;
                if(null != component)
                {
                    Transform point = component.transform;
                    while (point.parent != null)
                    {
                        point = point.parent;
                        nodePath = point.name + "/" + nodePath;

                        if (point.name == parentName)
                            break;
                    }

                    lineBuilder.AppendFormat("[{0}] {1} = {2}\n", type.ToString(), exportName, nodePath);

                    this.memberList += lineBuilder.ToString();
                }

                if (type == LuaComType.CDButton)
                {
                    string format = "\tself.${ButtonName}.onClick:RemoveAllListeners()\n";
                    format += "\tself.${ButtonName}.onClick:AddListener(function()\n";
                    format += "\t\tself:onClick_${ButtonName}()\n";
                    format += "\tend)\n";
                    string line1 = format.Replace("${ButtonName}", exportName);
                    addEventList += line1;

                    string format3 = "\tself.${ButtonName}.onClick:RemoveAllListeners()\n";
                    string line3 = format3.Replace("${ButtonName}", exportName);
                    removeEventList += line3;

                    string format2 = "function M:onClick_${ButtonName}()\n\nend\n\n";
                    string line2 = format2.Replace("${ClassName}", className);
                    line2 = line2.Replace("${ButtonName}", exportName);
                    eventCallbackList += line2;
                }
                else if (type == LuaComType.Toggle || type == LuaComType.Slider)
                {
                    string format = "\t${ToggleName}.onValueChanged:RemoveAllListeners()\n";
                    format += "\t${ToggleName}.onValueChanged:AddListener(function()\n";
                    format += "\t\tself:onValueChanged_${ToggleName}\n";
                    format += "\tend)\n";
                    string line1 = format.Replace("${ToggleName}", exportName);
                    addEventList += line1;

                    string format3 = "\tself.${ToggleName}.onValueChanged:RemoveAllListeners()\n";
                    string line3 = format3.Replace("${ToggleName}", exportName);
                    removeEventList += line3;

                    string format2 = "function M:onValueChanged_${ToggleName}()\n\nend\n\n";
                    string line2 = format2.Replace("${ToggleName}", exportName);
                    line2 = line2.Replace("${ClassName}", className);
                    eventCallbackList += line2;
                }
                else if (type == LuaComType.IMTextMeshProUGUI)
                {
                    string format = "\t -- self.${TextName}:SetText({\"\"});\n";
                    string line = format.Replace("${TextName}", exportName);
                    textLocalList += line;
                }
            }
        }
    }

    #region 代码模板


    // 这个表示一个子项，monoitem
    public string getItemFormat()
    {
        return @"
----------------------------------------
-- 这是一个直接从prefab上某个GameObject进行实例化的脚本
-- ${LuaPath}
-- ${AssetPath}
-- Datetime:
--     ${Creator} - ${DateTime}
--
-- ${ViewComment}
----------------------------------------

local ${ClassName} = class(${MarkClassName}, UIBaseItem)
local M = ${ClassName}

function M.Create(assetObj, param)
    if assetObj == nil or assetObj:IsNull() then 
        return nil;
    end

    local luaItemCom = assetObj.transform:GetComponent(${MarkUILuaItem});
    if luaItemCom == nil then
        return nil;
    end

    local item = luaItemCom:GetLuaItem();
    if item == nil then
        item = ${ClassName}.new();
        luaItemCom:Bind(item, param)
    end

    item:Init(param);

    return item;
end

function M:ctor()
    UIBaseItem.ctor(self)
end

function M:Init(param)

end

--唤醒事件,执行OnDestroy前只执行一次--
function M:Awake()
    -- 注册控件事件
    self:RegisterUIEvent()

    -- basic
    self:InitControl()
    self:TextLocalization()
end

function M:SetData(userData)
    -- datas

end

--初始化面板--
function M:InitControl()
    -- 所有控件路径
${ControlCommentBlock}
end

-- 语言本地化
function M:TextLocalization()
${TextLocal}
${OtherLocal}
end

-- 控件事件
function M:RegisterUIEvent()
${RegisterEvent}
end

function M:UnregisterUIEvent()
${UnregisterEvent}
end

function M:OnDestroy()
    self:UnregisterUIEvent()
end

--------------------------------------------------
${EventCallbackList}
--------------------------------------------------

return M

";
    }

    #endregion
}

#endregion

#endif