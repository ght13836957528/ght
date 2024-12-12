using System.IO;
using Framework;
using UnityEngine;

namespace LS
{
    public static partial class Constant
    {
        public static class Tables 
        {
            public static string DynamicXmlPath =  Path.Combine(Application.persistentDataPath, "remote");
            public static string LocalXmlPathEditor = Path.Combine(Application.dataPath, "Main", "Datatable");
            public static string LocalXmlPath = Path.Combine(Utility.GetStreamingAssetsDirectory(), "datatable");
            public static string VersionFilename = "VERSION.txt";  
            public static string FreeXmlFile = "database1.local.xml";
            public static string XmlOtherFile = "database.other.xml";
            public static string IgnoreXmlFile = "ignorefiles.xml";
            
            public static string LocalConfigPathEditor = Path.Combine(Application.dataPath, "Main", "Config");
            public static string LocalConfigPath = Path.Combine(Utility.GetStreamingAssetsDirectory(), "config");
        }
    }
}