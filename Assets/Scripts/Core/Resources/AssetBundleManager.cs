using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JRPGStudio.Core.Resources
{
    /// <summary>
    /// AssetBundle管理器
    /// 提供AssetBundle的打包配置、加载/卸载功能，以及热更新预留接口
    /// </summary>
    public class AssetBundleManager : MonoBehaviour
    {
        // 单例
        private static AssetBundleManager instance;
        public static AssetBundleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("AssetBundleManager");
                    instance = go.AddComponent<AssetBundleManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // 配置
        [Header("AssetBundle配置")]
        [SerializeField] private string baseUrl = "https://your-server.com/AssetBundles/";
        [SerializeField] private string variant = "android";
        [SerializeField] private bool simulationMode = true; // 模拟模式，用于编辑器开发
        
        // 运行时数据
        private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
        private Dictionary<string, WWW> downloadingBundles = new Dictionary<string, WWW>();
        private List<string> bundleLoadOrder = new List<string>();
        
        // 版本信息
        private AssetBundleManifest manifest;
        private Dictionary<string, Hash128> bundleHashes = new Dictionary<string, Hash128>();

        #region 初始化

        /// <summary>
        /// 初始化AssetBundle系统
        /// </summary>
        /// <param name="callback">初始化完成回调</param>
        public void Initialize(Action<bool> callback)
        {
            if (simulationMode)
            {
                Debug.Log("AssetBundleManager: 运行在模拟模式");
                callback?.Invoke(true);
                return;
            }

            StartCoroutine(InitializeCoroutine(callback));
        }

        private IEnumerator InitializeCoroutine(Action<bool> callback)
        {
            string manifestPath = GetPlatformBundlePath();
            
            // 加载主manifest
            var request = AssetBundle.LoadFromFileAsync(manifestPath);
            yield return request;

            if (request.assetBundle != null)
            {
                manifest = request.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                request.assetBundle.Unload(false);

                if (manifest != null)
                {
                    // 获取所有Bundle的哈希值
                    string[] bundleNames = manifest.GetAllAssetBundles();
                    foreach (var bundleName in bundleNames)
                    {
                        Hash128 hash = manifest.GetAssetBundleHash(bundleName);
                        bundleHashes[bundleName] = hash;
                    }

                    Debug.Log($"AssetBundleManager: 初始化成功，共 {bundleNames.Length} 个Bundle");
                    callback?.Invoke(true);
                }
                else
                {
                    Debug.LogError("AssetBundleManager: 无法加载AssetBundleManifest");
                    callback?.Invoke(false);
                }
            }
            else
            {
                Debug.LogError($"AssetBundleManager: 无法加载Manifest: {manifestPath}");
                callback?.Invoke(false);
            }
        }

        #endregion

        #region 加载方法

        /// <summary>
        /// 同步加载AssetBundle
        /// </summary>
        /// <param name="bundleName">Bundle名称</param>
        /// <returns>加载的AssetBundle</returns>
        public AssetBundle LoadAssetBundle(string bundleName)
        {
            if (loadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                return bundle;
            }

            if (simulationMode)
            {
                Debug.Log($"AssetBundleManager [Sim]: 加载Bundle - {bundleName}");
                return null;
            }

            string path = GetBundlePath(bundleName);
            bundle = AssetBundle.LoadFromFile(path);
            
            if (bundle != null)
            {
                loadedBundles[bundleName] = bundle;
                bundleLoadOrder.Add(bundleName);
                Debug.Log($"AssetBundleManager: 加载Bundle成功 - {bundleName}");
            }
            else
            {
                Debug.LogError($"AssetBundleManager: 加载Bundle失败 - {bundleName}");
            }

            return bundle;
        }

        /// <summary>
        /// 异步加载AssetBundle
        /// </summary>
        /// <param name="bundleName">Bundle名称</param>
        /// <param name="callback">加载完成回调</param>
        public void LoadAssetBundleAsync(string bundleName, Action<AssetBundle> callback)
        {
            if (loadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                callback?.Invoke(bundle);
                return;
            }

            if (simulationMode)
            {
                Debug.Log($"AssetBundleManager [Sim]: 异步加载Bundle - {bundleName}");
                StartCoroutine(SimulationAsyncLoad(bundleName, callback));
                return;
            }

            StartCoroutine(LoadAssetBundleCoroutine(bundleName, callback));
        }

        /// <summary>
        /// 从远程URL异步加载AssetBundle
        /// </summary>
        /// <param name="bundleName">Bundle名称</param>
        /// <param name="callback">加载完成回调</param>
        public void LoadAssetBundleFromWeb(string bundleName, Action<AssetBundle> callback)
        {
            if (loadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                callback?.Invoke(bundle);
                return;
            }

            StartCoroutine(DownloadAssetBundle(bundleName, callback));
        }

        /// <summary>
        /// 加载AssetBundle中的资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="bundleName">Bundle名称</param>
        /// <param name="assetName">资源名称</param>
        /// <param name="callback">加载完成回调</param>
        public void LoadAsset<T>(string bundleName, string assetName, Action<T> callback) where T : UnityEngine.Object
        {
            LoadAssetBundleAsync(bundleName, (bundle) =>
            {
                if (bundle == null)
                {
                    callback?.Invoke(null);
                    return;
                }

                T asset = bundle.LoadAsset<T>(assetName);
                callback?.Invoke(asset);
            });
        }

        /// <summary>
        /// 异步加载AssetBundle中的所有资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="bundleName">Bundle名称</param>
        /// <param name="callback">加载完成回调</param>
        public void LoadAllAssets<T>(string bundleName, Action<T[]> callback) where T : UnityEngine.Object
        {
            LoadAssetBundleAsync(bundleName, (bundle) =>
            {
                if (bundle == null)
                {
                    callback?.Invoke(null);
                    return;
                }

                T[] assets = bundle.LoadAllAssets<T>();
                callback?.Invoke(assets);
            });
        }

        #endregion

        #region 卸载方法

        /// <summary>
        /// 卸载单个AssetBundle
        /// </summary>
        /// <param name="bundleName">Bundle名称</param>
        /// <param name="unloadAllLoadedObjects">是否卸载加载的对象</param>
        public void UnloadAssetBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
            if (loadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                bundle.Unload(unloadAllLoadedObjects);
                loadedBundles.Remove(bundleName);
                bundleLoadOrder.Remove(bundleName);
                Debug.Log($"AssetBundleManager: 卸载Bundle - {bundleName}");
            }
        }

        /// <summary>
        /// 卸载所有AssetBundle
        /// </summary>
        /// <param name="unloadAllLoadedObjects">是否卸载加载的对象</param>
        public void UnloadAllAssetBundles(bool unloadAllLoadedObjects = false)
        {
            foreach (var bundle in loadedBundles.Values)
            {
                bundle.Unload(unloadAllLoadedObjects);
            }
            loadedBundles.Clear();
            bundleLoadOrder.Clear();
            Debug.Log("AssetBundleManager: 已卸载所有Bundle");
        }

        /// <summary>
        /// 卸载未使用的AssetBundle
        /// </summary>
        public void UnloadUnusedAssetBundles()
        {
            Resources.UnloadUnusedAssets();
        }

        #endregion

        #region 依赖管理

        /// <summary>
        /// 获取Bundle的所有依赖
        /// </summary>
        /// <param name="bundleName">Bundle名称</param>
        /// <returns>依赖的Bundle名称数组</returns>
        public string[] GetDependencies(string bundleName)
        {
            if (manifest != null)
            {
                return manifest.GetAllDependencies(bundleName);
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// 预加载Bundle及其依赖
        /// </summary>
        /// <param name="bundleName">Bundle名称</param>
        /// <param name="progressCallback">进度回调</param>
        /// <param name="callback">完成回调</param>
        public void PreloadBundleWithDependencies(string bundleName, Action<float> progressCallback, Action callback)
        {
            StartCoroutine(PreloadDependenciesCoroutine(bundleName, progressCallback, callback));
        }

        #endregion

        #region 热更新接口（预留）

        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="callback">回调，返回需要更新的Bundle列表</param>
        public void CheckForUpdates(Action<List<string>> callback)
        {
            if (manifest == null)
            {
                Debug.LogWarning("AssetBundleManager: Manifest未加载，无法检查更新");
                callback?.Invoke(null);
                return;
            }

            StartCoroutine(CheckForUpdatesCoroutine(callback));
        }

        /// <summary>
        /// 下载更新
        /// </summary>
        /// <param name="bundlesToUpdate">需要更新的Bundle列表</param>
        /// <param name="progressCallback">进度回调</param>
        /// <param name="callback">完成回调</param>
        public void DownloadUpdates(List<string> bundlesToUpdate, Action<float> progressCallback, Action callback)
        {
            StartCoroutine(DownloadUpdatesCoroutine(bundlesToUpdate, progressCallback, callback));
        }

        /// <summary>
        /// 获取当前版本信息
        /// </summary>
        public string GetVersionInfo()
        {
            if (manifest != null)
            {
                return $"Bundles: {loadedBundles.Count}, Hashes: {bundleHashes.Count}";
            }
            return "Manifest not loaded";
        }

        #endregion

        #region 查询方法

        /// <summary>
        /// 检查Bundle是否已加载
        /// </summary>
        public bool IsBundleLoaded(string bundleName)
        {
            return loadedBundles.ContainsKey(bundleName);
        }

        /// <summary>
        /// 获取已加载Bundle数量
        /// </summary>
        public int LoadedBundleCount => loadedBundles.Count;

        /// <summary>
        /// 获取所有Bundle名称
        /// </summary>
        public string[] GetAllBundleNames()
        {
            if (manifest != null)
            {
                return manifest.GetAllAssetBundles();
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// 获取Bundle信息
        /// </summary>
        public BundleInfo GetBundleInfo(string bundleName)
        {
            if (!loadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                return null;
            }

            return new BundleInfo
            {
                name = bundleName,
                isLoaded = true,
                size = bundle.sourceAssetBundle != null ? 
                    bundle.sourceAssetBundle.GetAllAssetNames().Length : 0,
                hash = bundleHashes.TryGetValue(bundleName, out Hash128 hash) ? hash.ToString() : "unknown"
            };
        }

        #endregion

        #region 私有方法

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private string GetPlatformBundlePath()
        {
            #if UNITY_EDITOR
            return Application.streamingAssetsPath + "/" + GetPlatformName();
            #elif UNITY_ANDROID
            return Application.streamingAssetsPath + "/" + GetPlatformName();
            #elif UNITY_IOS
            return Application.streamingAssetsPath + "/" + GetPlatformName();
            #else
            return Application.streamingAssetsPath + "/" + GetPlatformName();
            #endif
        }

        private string GetBundlePath(string bundleName)
        {
            return GetPlatformBundlePath() + "/" + bundleName;
        }

        private string GetPlatformName()
        {
            #if UNITY_EDITOR
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString().ToLower();
            #elif UNITY_ANDROID
            return "android";
            #elif UNITY_IOS
            return "ios";
            #elif UNITY_STANDALONE
            return "standalone";
            #else
            return "unknown";
            #endif
        }

        private IEnumerator LoadAssetBundleCoroutine(string bundleName, Action<AssetBundle> callback)
        {
            // 先加载依赖
            string[] dependencies = GetDependencies(bundleName);
            foreach (var dep in dependencies)
            {
                if (!loadedBundles.ContainsKey(dep))
                {
                    var depRequest = AssetBundle.LoadFromFileAsync(GetBundlePath(dep));
                    yield return depRequest;
                    
                    if (depRequest.assetBundle != null)
                    {
                        loadedBundles[dep] = depRequest.assetBundle;
                    }
                }
            }

            // 加载主Bundle
            var request = AssetBundle.LoadFromFileAsync(GetBundlePath(bundleName));
            yield return request;

            if (request.assetBundle != null)
            {
                loadedBundles[bundleName] = request.assetBundle;
                bundleLoadOrder.Add(bundleName);
                callback?.Invoke(request.assetBundle);
            }
            else
            {
                Debug.LogError($"AssetBundleManager: 加载失败 - {bundleName}");
                callback?.Invoke(null);
            }
        }

        private IEnumerator DownloadAssetBundle(string bundleName, Action<AssetBundle> callback)
        {
            if (downloadingBundles.ContainsKey(bundleName))
            {
                Debug.LogWarning($"AssetBundleManager: Bundle正在下载中 - {bundleName}");
                yield break;
            }

            string url = baseUrl + GetPlatformName() + "/" + bundleName;
            
            if (bundleHashes.TryGetValue(bundleName, out Hash128 hash))
            {
                url += "?hash=" + hash.ToString();
            }

            Debug.Log($"AssetBundleManager: 开始下载 - {url}");
            
            using (WWW www = new WWW(url))
            {
                downloadingBundles[bundleName] = www;
                
                while (!www.isDone)
                {
                    yield return null;
                }
                
                downloadingBundles.Remove(bundleName);

                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogError($"AssetBundleManager: 下载失败 - {www.error}");
                    callback?.Invoke(null);
                }
                else
                {
                    AssetBundle bundle = www.assetBundle;
                    if (bundle != null)
                    {
                        loadedBundles[bundleName] = bundle;
                        bundleLoadOrder.Add(bundleName);
                        callback?.Invoke(bundle);
                    }
                    else
                    {
                        Debug.LogError($"AssetBundleManager: Bundle加载失败 - {bundleName}");
                        callback?.Invoke(null);
                    }
                }
            }
        }

        private IEnumerator SimulationAsyncLoad(string bundleName, Action<AssetBundle> callback)
        {
            yield return new WaitForSeconds(0.5f); // 模拟加载延迟
            callback?.Invoke(null);
        }

        private IEnumerator PreloadDependenciesCoroutine(string bundleName, Action<float> progressCallback, Action callback)
        {
            string[] dependencies = GetDependencies(bundleName);
            int total = dependencies.Length + 1;
            int current = 0;

            foreach (var dep in dependencies)
            {
                if (!loadedBundles.ContainsKey(dep))
                {
                    var request = AssetBundle.LoadFromFileAsync(GetBundlePath(dep));
                    yield return request;
                    
                    if (request.assetBundle != null)
                    {
                        loadedBundles[dep] = request.assetBundle;
                    }
                }
                
                current++;
                progressCallback?.Invoke((float)current / total);
            }

            // 加载主Bundle
            if (!loadedBundles.ContainsKey(bundleName))
            {
                var request = AssetBundle.LoadFromFileAsync(GetBundlePath(bundleName));
                yield return request;
                
                if (request.assetBundle != null)
                {
                    loadedBundles[bundleName] = request.assetBundle;
                }
            }

            current++;
            progressCallback?.Invoke(1f);
            callback?.Invoke();
        }

        private IEnumerator CheckForUpdatesCoroutine(Action<List<string>> callback)
        {
            List<string> updates = new List<string>();
            
            // 在实际实现中，这里会从服务器获取版本信息并比较
            // 目前只是返回空列表
            yield return null;
            
            callback?.Invoke(updates);
        }

        private IEnumerator DownloadUpdatesCoroutine(List<string> bundlesToUpdate, Action<float> progressCallback, Action callback)
        {
            int total = bundlesToUpdate.Count;
            int current = 0;

            foreach (var bundleName in bundlesToUpdate)
            {
                // 先卸载旧版本
                if (loadedBundles.ContainsKey(bundleName))
                {
                    loadedBundles[bundleName].Unload(false);
                    loadedBundles.Remove(bundleName);
                }

                // 下载新版本
                string url = baseUrl + GetPlatformName() + "/" + bundleName;
                
                using (WWW www = new WWW(url))
                {
                    while (!www.isDone)
                    {
                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(www.error))
                    {
                        Debug.LogError($"AssetBundleManager: 更新下载失败 - {bundleName}: {www.error}");
                    }
                    else if (www.assetBundle != null)
                    {
                        loadedBundles[bundleName] = www.assetBundle;
                        Debug.Log($"AssetBundleManager: 更新成功 - {bundleName}");
                    }
                }

                current++;
                progressCallback?.Invoke((float)current / total);
            }

            callback?.Invoke();
        }

        #endregion

        #region 内部类

        /// <summary>
        /// Bundle信息
        /// </summary>
        public class BundleInfo
        {
            public string name;
            public bool isLoaded;
            public int size;
            public string hash;
        }

        #endregion
    }
}
