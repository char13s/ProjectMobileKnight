using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace Pixiv.VroidSdk.Editor
{
    public static class AndroidMainTemplateFile
    {
        private const string AndroidMainTemplatePath = "Assets/Plugins/Android/mainTemplate.gradle";

        public static bool IsFileExists()
        {
            return File.Exists(AndroidMainTemplatePath);
        }

        public static bool CopyFromExample()
        {
            var manifestDirectory = Path.GetDirectoryName(AndroidMainTemplatePath);
            Directory.CreateDirectory(manifestDirectory);

            var examplePath = AssetDatabase.GUIDToAssetPath("55d3749bdae045a4f88b7a506e15a59c");
            return AssetDatabase.CopyAsset(examplePath, AndroidMainTemplatePath);
        }
    }
}
