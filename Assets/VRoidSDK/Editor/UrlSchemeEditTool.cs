using Pixiv.VroidSdk.Oauth.DataModel;
using UnityEngine;

namespace Pixiv.VroidSdk.Editor
{
    public class UrlSchemeEditTool
    {
        public class UrlScheme
        {
            public string IosUrlScheme;
            public string AndroidUrlScheme;
        }

        public static UrlScheme GetUrlScheme()
        {
            var asset = Resources.Load<TextAsset>("credential.json");

            var credential = Credential.FromJson(asset.text);

            return new UrlScheme
            {
                IosUrlScheme = credential.IosUrlScheme,
                AndroidUrlScheme = credential.AndroidUrlScheme,
            };
        }
    }
}
