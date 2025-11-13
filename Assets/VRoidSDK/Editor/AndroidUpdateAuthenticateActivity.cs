using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Pixiv.VroidSdk.Editor
{
    public class AndroidUpdateAuthenticateActivity : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; private set; }
        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                ProcessForAndroidAuthentication();
            }
        }

        private void ProcessForAndroidAuthentication()
        {
            if (!AndroidManifestFile.IsFileExists())
            {
                AndroidManifestFile.CopyFromExample();
                Debug.Log("AndroidManifest.xmlを作成しました");
            }

            if (!AndroidMainTemplateFile.IsFileExists())
            {
                AndroidMainTemplateFile.CopyFromExample();
                Debug.Log("mainTemplate.gradleを作成しました");
            }

            if (!AndroidGradleTemplateFile.IsFileExists())
            {
                AndroidGradleTemplateFile.CopyFromExample();
                Debug.Log("gradleTemplate.propertiesを作成しました");
            }

            var manifestXml = AndroidManifestFile.MakeManifestXml();
            var cfg = UrlSchemeEditTool.GetUrlScheme();
            if (cfg.AndroidUrlScheme == null)
            {
                return;
            }
            var urlScheme = cfg.AndroidUrlScheme;
            var Validator = new UriSchemeValidator(urlScheme);
            if (!Validator.Validate())
            {
                Debug.LogError("AndroidのURLスキーマが不正なのでURLスキーマの追加処理をスキップしました");
                return;
            }

            Uri uri;
            try
            {
                uri = new Uri(urlScheme);
            }
            catch (UriFormatException)
            {
                return;
            }

            manifestXml.UpdateAuthenticateActivity(uri);
            manifestXml.Save(AndroidManifestFile.AndroidManifestPath);
        }
    }
}
