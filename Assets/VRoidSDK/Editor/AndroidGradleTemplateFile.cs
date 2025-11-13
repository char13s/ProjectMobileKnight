using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace Pixiv.VroidSdk.Editor
{
    public static class AndroidGradleTemplateFile
    {
        private const string AndroidGradleTemplatePath = "Assets/Plugins/Android/gradleTemplate.properties";

        public static bool IsFileExists()
        {
            return File.Exists(AndroidGradleTemplatePath);
        }

        public static bool CopyFromExample()
        {
            var manifestDirectory = Path.GetDirectoryName(AndroidGradleTemplatePath);
            Directory.CreateDirectory(manifestDirectory);

            var examplePath = AssetDatabase.GUIDToAssetPath("68bfbb343e84dbd499fc8b4ab6ca0efe");
            return AssetDatabase.CopyAsset(examplePath, AndroidGradleTemplatePath);
        }
    }
}
