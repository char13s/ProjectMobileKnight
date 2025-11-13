using System.IO;
using System.Xml.Linq;
using UnityEditor;

namespace Pixiv.VroidSdk.Editor
{
    public class AndroidManifestFile
    {
        public const string AndroidManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";

        public static bool IsFileExists()
        {
            return File.Exists(AndroidManifestPath);
        }

        public static bool CopyFromExample()
        {
            var manifestDirectory = Path.GetDirectoryName(AndroidManifestPath);
            Directory.CreateDirectory(manifestDirectory);

            // Assets/VRoidSDK/Android/AndroidManifest.xml.example
            var examplePath = AssetDatabase.GUIDToAssetPath("d609d389d881c43428416469a2534af1");
            return AssetDatabase.CopyAsset(examplePath, AndroidManifestPath);
        }

        public static ManifestXmlDocument MakeManifestXml()
        {
            var androidManifestXml = XDocument.Load(AndroidManifestPath);
            var root = androidManifestXml.Root;
            root.SetAttributeValue("package", PlayerSettings.applicationIdentifier);

            return new ManifestXmlDocument(androidManifestXml);
        }
    }
}
