using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using UnityEngine;

namespace Pixiv.VroidSdk.Editor
{
    public class ManifestXmlDocument
    {
        private static readonly XNamespace AndroidNamespaceUri = "http://schemas.android.com/apk/res/android";
        private static readonly XName XNameScheme = AndroidNamespaceUri + "scheme";
        private static readonly XName XNameName = AndroidNamespaceUri + "name";
        private static readonly XName XNameHost = AndroidNamespaceUri + "host";
        private static readonly XName XNamePath = AndroidNamespaceUri + "path";
        private static readonly XName XNameTheme = AndroidNamespaceUri + "theme";
        private static readonly XName XNameLaunchMode = AndroidNamespaceUri + "launchMode";

        private readonly XDocument _manifestDocument;

        public ManifestXmlDocument(XDocument doc)
        {
            _manifestDocument = doc;
        }

        private static XElement CreateUrlSchemeData(Uri uri)
        {
            var data = new XElement("data");
            data.SetAttributeValue(XNameScheme, uri.Scheme);
            data.SetAttributeValue(XNameHost, uri.Authority);
            if (uri.LocalPath != "/") data.SetAttributeValue(XNamePath, uri.LocalPath);
            return data;
        }

        private static XElement CreateAuthenticateActivity(Uri uri)
        {
            var activity = new XElement("activity");
            activity.SetAttributeValue(XNameName, "net.pixiv.vroidsdk.AuthenticateActivity");
            activity.SetAttributeValue(XNameTheme, "@android:style/Theme.Translucent.NoTitleBar");
            activity.SetAttributeValue(XNameLaunchMode, "singleTask");

            var intentFilter = new XElement("intent-filter");
            activity.Add(intentFilter);

            var action = new XElement("action");
            action.SetAttributeValue(XNameName, "android.intent.action.VIEW");
            intentFilter.Add(action);

            var category1 = new XElement("category");
            category1.SetAttributeValue(XNameName, "android.intent.category.DEFAULT");
            intentFilter.Add(category1);

            var category2 = new XElement("category");
            category2.SetAttributeValue(XNameName, "android.intent.category.BROWSABLE");
            intentFilter.Add(category2);

            intentFilter.Add(CreateUrlSchemeData(uri));
            return activity;
        }

        public void Save(string path)
        {
            _manifestDocument.Save(path);
        }

        public void UpdateAuthenticateActivity(Uri uri)
        {
            var application = _manifestDocument.XPathSelectElements("/manifest/application").First();
            try
            {
                var activity = application.XPathSelectElements("./activity").First(element =>
                    element.Attribute(XNameName)?.Value == "net.pixiv.vroidsdk.AuthenticateActivity");
                var intentFilter = activity.XPathSelectElements("./intent-filter").First();
                var hasData = intentFilter.XPathSelectElements("./data").Where(element =>
                {
                    if (element.Attribute(XNameScheme)?.Value != uri.Scheme) return false;
                    if (element.Attribute(XNameHost)?.Value != uri.Authority) return false;
                    return uri.LocalPath == "/" || element.Attribute(XNamePath)?.Value == uri.LocalPath;
                }).Any();

                if (hasData) return;

                intentFilter.Add(CreateUrlSchemeData(uri));
                Debug.Log("AndroidManifestのUrlSchemeを更新しました");
            }
            catch (Exception)
            {
                application.Add(new XComment(" begin VRoidSDK "));
                application.Add(CreateAuthenticateActivity(uri));
                application.Add(new XComment(" end VRoidSDK "));
                Debug.Log("AndroidManifestにActivityを追加しました。");
            }
        }
    }
}
