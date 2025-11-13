using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pixiv.VroidSdk.Api;
using Pixiv.VroidSdk.Api.DataModel;
using Pixiv.VroidSdk.Cache;
using Pixiv.VroidSdk.Cache.DataModel;
using Pixiv.VroidSdk.Cache.Migrate;
using Pixiv.VroidSdk.IO;
using Pixiv.VroidSdk.Logger;
using Pixiv.VroidSdk.Unity.Crypt;
using UniGLTF;
using UnityEngine;
using UniVRM10;

namespace Pixiv.VroidSdk
{
    /// <summary>
    /// モデルの読み込みユーティリティ
    ///
    /// 原則、コールバックは非同期に呼び出される
    ///
    /// Asyncがつくものは非同期に実行されるかつ、成功結果の<see cref="Task"/>が返ってくる
    /// </summary>
    public static class ModelLoader
    {
        private static ModelDataCache s_cache;
        private static string s_temporaryCachePath;
        private static SynchronizationContext s_context;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="config">VRoid SDKの利用設定</param>
        /// <param name="downloadApi">ダウンロードAPIをリクエストするクライアント</param>
        /// <param name="cacheEncryptPassword">モデルデータを暗号化するアプリケーション固有の任意文字列</param>
        /// <param name="maxCacheCount">保持するモデルの最大数</param>
        /// <param name="context">メインスレッドの<see cref="SynchronizationContext"/></param>
        public static void Initialize(ISdkConfig config, IDownloadLicensePublishable downloadApi, string cacheEncryptPassword, uint maxCacheCount = 10, SynchronizationContext context = null)
        {
            s_temporaryCachePath = Application.temporaryCachePath;
            s_context = context ?? SynchronizationContext.Current;

            var cryptoFileReadWrite = new CharacterModelVersionCacheFile(CharacterModelVersionCacheFilePath(config), new UnityFileCryptor(cacheEncryptPassword));
            var storage = DownloadLicenseCacheStorage.Load(DownloadLicenseCacheStorageFilePath(config), maxCacheCount);
            s_cache = new ModelDataCache(downloadApi, cryptoFileReadWrite, storage, new LegacyCacheMigrator(config.SymbolPrefix, Application.persistentDataPath));
        }

        /// <summary>
        /// モデルのキャッシュを削除します
        /// </summary>
        /// <param name="config"><see cref="Initialize"/>で使用した、あるいは使用する予定のconfigを指定します</param>
        /// <exception cref="InvalidOperationException">その他の理由で削除に失敗しました。</exception>
        /// <remarks>
        /// <see cref="ModelLoader.Initialize"/>を呼び出した後に呼び出した場合は、メモリからもキャッシュを削除します。
        /// </remarks>
        public static void DeleteModelCache(ISdkConfig config)
        {
            if (s_cache == null)
            {
                DeleteModelCacheFromFile(config);
                return;
            }

            DeleteModelCacheFromFileAndMemory();
        }

        /// <summary>
        /// モデルキャッシュをファイルとメモリから削除します
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="DeleteModelCacheFromFileAndMemory"/>メソッドは<see cref="Initialize"/>を呼び出す前に呼んではいけません。
        /// この例外を受け取った場合、ファイルのみを削除する<see cref="DeleteModelCacheFromFile"/>メソッドを呼んでください。
        /// </exception>
        private static void DeleteModelCacheFromFileAndMemory()
        {
            // キャッシュ初期化済みの状態で削除する
            if (s_cache == null)
            {
                throw new InvalidOperationException("MultiplayModelLoader is not initialized. Use DeleteModelCacheFromFile instead of DeleteModelCacheFromFileAndMemory.");
            }

            s_cache.DeleteStorage();
        }

        /// <summary>
        /// モデルキャッシュのファイルを削除します。
        ///
        /// <see cref="Initialize"/>メソッドの初期化前のみ呼ぶことができます。
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="InvalidOperationException">
        /// <see cref="DeleteModelCacheFromFile"/>メソッドは<see cref="Initialize"/>を呼び出した後に呼んではいけません。
        /// 初期化後であればファイルとメモリ上のキャッシュを同時に削除する<see cref="DeleteModelCacheFromFileAndMemory"/>メソッドを呼んでください。
        /// </exception>
        private static void DeleteModelCacheFromFile(ISdkConfig config)
        {
            if (s_cache != null)
            {
                throw new InvalidOperationException("ModelLoader is already initialized. Use DeleteModelCacheFromFileAndMemory instead of DeleteModelCacheFromFile.");
            }

            var storePath = DownloadLicenseCacheStorageFilePath(config);
            try
            {
                File.Delete(storePath);
            }
            catch (DirectoryNotFoundException)
            {
                // ファイルが存在しない場合のみ許容する
            }
            catch (Exception ex)
            {
                SdkLogger.LogException(ex);
            }
        }


        /// <summary>
        /// VRMモデルを読み込む
        /// </summary>
        /// <remarks>
        /// デバイスにキャッシュがある場合はキャッシュから読み込む。
        /// キャッシュが存在しない場合はダウンロードを行う。
        /// </remarks>
        /// <param name="characterModel">ダウンロードをするモデル</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onProgress">進捗コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="materialGenerator">VRMロード時に使用するMaterialDescriptorGenerator</param>
        /// <exception cref="Exception"><see cref="ModelLoader"/>が初期化されていない場合にスローされる例外</exception>
        /// <seealso cref="LoadVrmAsync"/>
        public static void LoadVrm(CharacterModel characterModel, Action<GameObject> onSuccess, Action<float> onProgress,
            Action<ModelLoadFailException> onFailed, IMaterialDescriptorGenerator materialGenerator = null)
        {
            if (s_cache == null)
            {
                throw new Exception("ModelLoader is not initialized. Please call ModelLoader.Initialize");
            }

            s_cache.Fetch(characterModel,
                (binary) => LoadVRMFromBinary(binary, onSuccess, onFailed, materialGenerator),
                onProgress,
                (error) => onFailed?.Invoke(new ModelLoadFailException(error)));
        }

        /// <summary>
        /// VRMモデルを読み込む
        /// </summary>
        /// <remarks>
        /// デバイスにキャッシュがある場合はキャッシュから読み込む。
        /// キャッシュが存在しない場合はダウンロードを行う。
        /// </remarks>
        /// <param name="characterModel">ダウンロードをするモデル</param>
        /// <param name="onProgress">進捗コールバック</param>
        /// <exception cref="Exception"><see cref="ModelLoader"/>が初期化されていない場合にスローされる例外</exception>
        /// <seealso cref="LoadVrm"/>
        public static async Task<GameObject> LoadVrmAsync(CharacterModel characterModel, Action<float> onProgress, IMaterialDescriptorGenerator materialGenerator = null)
        {
            if (s_cache == null)
            {
                throw new Exception("ModelLoader is not initialized. Please call ModelLoader.Initialize");
            }

            try
            {
                var binary = await s_cache.FetchAsync(characterModel, onProgress);
                return await LoadVRMFromBinaryAsync(binary, materialGenerator);
            }
            catch (Exception e)
            {
                throw new ModelLoadFailException(e.Message);
            }
        }

        private static void LoadVRMFromBinary(byte[] characterBinary, Action<GameObject> onVrmModelLoaded, Action<ModelLoadFailException> onFailed, IMaterialDescriptorGenerator materialGenerator = null)
        {
            Vrm10.LoadBytesAsync(characterBinary, canLoadVrm0X: true, showMeshes: true, materialGenerator: materialGenerator).ContinueWith(result =>
            {
                s_context.Post(_ =>
                {
                    if (result.Exception != null)
                    {
                        var exception = result.Exception.Flatten().InnerException;
                        onFailed?.Invoke(new ModelLoadFailException(exception.Message));
                    }
                    else
                    {
                        onVrmModelLoaded?.Invoke(result.Result.gameObject);
                    }
                }, null);
            });
        }

        private static Task<GameObject> LoadVRMFromBinaryAsync(byte[] characterBinary, IMaterialDescriptorGenerator materialGenerator = null)
        {
            var taskResult = new TaskCompletionSource<GameObject>();

            Vrm10.LoadBytesAsync(characterBinary, canLoadVrm0X: true, showMeshes: true, materialGenerator: materialGenerator).ContinueWith(result =>
            {
                s_context.Post(_ =>
                {
                    if (result.Exception != null)
                    {
                        var exception = result.Exception.Flatten().InnerException;
                        taskResult.SetException(exception);
                    }
                    else
                    {
                        taskResult.SetResult(result.Result.gameObject);
                    }
                }, null);
            });

            return taskResult.Task;
        }

        private static string CharacterModelVersionCacheFilePath(ISdkConfig config)
        {
            Debug.Assert(!string.IsNullOrEmpty(s_temporaryCachePath), "s_temporaryCachePath is null or empty.");

            return Path.Combine(s_temporaryCachePath,
                                            config.BaseDirectoryName,
                                            "character_model_version_cache_files");
        }

        private static string DownloadLicenseCacheStorageFilePath(ISdkConfig config)
        {
            Debug.Assert(!string.IsNullOrEmpty(s_temporaryCachePath), "s_temporaryCachePath is null or empty.");

            return Path.Combine(s_temporaryCachePath,
                                        config.BaseDirectoryName,
                                        $"{config.SymbolPrefix}_download_license_cache");
        }
    }
}
