using System;
using UnityEngine;

namespace Pixiv.VroidSdk.Browser
{
#if !UNITY_EDITOR && UNITY_ANDROID
    internal sealed class AndroidBrowser : MonoBehaviour, IManualCodeRegistrable
    {
        private Action<string> _onRegistered;
        private Action<Exception> _onFailed;


        public string State { get; set; }
        public string RedirectUri { get; set; }

        public void OnOpenUrl(String appUrl)
        {
            try
            {
                var query = UrlParser.ParseAuthCode(appUrl, RedirectUri);
                if (query.State != State)
                {
                    _onFailed?.Invoke(new RegisterCodeFailedException("invalid state"));
                    return;
                }

                if (string.IsNullOrEmpty(query.AuthCode))
                {
                    _onFailed?.Invoke(new RegisterCodeFailedException("authCode is null or empty"));
                    return;
                }

                _onRegistered?.Invoke(query.AuthCode);
            }
            catch (InvalidCallbackUrlException ex)
            {
                _onFailed?.Invoke(ex);
            }
        }

        public void OnCancelAuthorize(string _message)
        {
            _onFailed?.Invoke(new RegisterCodeCancelException("code cancel"));
        }

        public void OpenBrowserWindow(string url, Action<string> onRegisterCodeReceived, Action<Exception> onFailed)
        {
            _onRegistered = onRegisterCodeReceived;
            _onFailed = onFailed;
            var androidBrowser = new AndroidJavaClass("net.pixiv.vroidsdk.AuthenticateActivity");
            androidBrowser.CallStatic("launch", url);
        }

        public void CleanUp()
        {
        }

        public void OnRegisterCode(string authCode)
        {
            _onRegistered?.Invoke(authCode);
        }
    }
#endif
}
