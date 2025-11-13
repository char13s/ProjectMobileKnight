using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pixiv.VroidSdk.Editor
{
    public class CacheClearMenu : MonoBehaviour
    {
        [MenuItem("VRoidSDK/Clear Model Cache")]
        static void ClearModelCache()
        {
            var asset = Resources.Load<TextAsset>("credential.json");
            if (asset == null)
            {
                Debug.LogError("credential.json is not found.");
                return;
            }

            var config = OauthProvider.CreateSdkConfig(asset.text);
            var path = Path.Combine(Application.temporaryCachePath,
                config.BaseDirectoryName,
                $"{config.SymbolPrefix}_download_license_cache");
            // ファイルを削除
            try
            {
                File.Delete(path);
            }
            catch (DirectoryNotFoundException)
            {
                // ファイルが存在しなくても黙る
            }
            Debug.Log("[VRoidSDK] model cache is cleared.");
        }
    }
}
