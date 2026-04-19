using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JRPGStudio.Core.Resources
{
    /// <summary>
    /// 资源加载管理器
    /// 提供统一的资源加载接口，支持异步加载、缓存机制和预加载功能
    /// </summary>
    public class ResourceLoader : MonoBehaviour
    {
        // 单例
        private static ResourceLoader instance;
        public static ResourceLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("ResourceLoader");
                    instance = go.AddComponent<ResourceLoader>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // 缓存配置
        [Header("缓存设置")]
        [SerializeField] private bool enableCache = true;
        [SerializeField] private int maxCacheCount = 100;
        [SerializeField] private float cacheExpirationTime = 300f; // 缓存过期时间（秒）

        // 资源缓存
        private Dictionary<string, UnityEngine.Object> resourceCache = new Dictionary<string, UnityEngine.Object>();
        private Dictionary<string, float> cacheTimestamps = new Dictionary<string, float>();
        private Dictionary<string, int> cacheAccessCount = new Dictionary<string, int>();

        // 异步加载队列
        private Queue<AsyncLoadRequest> asyncLoadQueue = new Queue<AsyncLoadRequest>();
        private List<AsyncLoadRequest> activeAsyncLoads = new List<AsyncLoadRequest>();
        private const int MaxConcurrentAsyncLoads = 5;

        // 预加载标识
        private bool isPreloading = false;

        #region 基础加载方法

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">Resources路径（不含"Resources/"前缀）</param>
        /// <returns>加载的资源，如果未找到则返回null</returns>
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("ResourceLoader: 路径为空");
                return null;
            }

            // 检查缓存
            if (enableCache && resourceCache.TryGetValue(path, out UnityEngine.Object cached))
            {
                UpdateCacheAccess(path);
                return cached as T;
            }

            // 加载资源
            T resource = Resources.Load<T>(path);
            
            if (resource != null)
            {
                AddToCache(path, resource);
            }
            else
            {
                Debug.LogWarning($"ResourceLoader: 资源加载失败 - {path}");
            }

            return resource;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">Resources路径</param>
        /// <param name="callback">加载完成回调</param>
        public void LoadAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("ResourceLoader: 路径为空");
                callback?.Invoke(null);
                return;
            }

            // 检查缓存
            if (enableCache && resourceCache.TryGetValue(path, out UnityEngine.Object cached))
            {
                UpdateCacheAccess(path);
                callback?.Invoke(cached as T);
                return;
            }

            // 添加到异步加载队列
            AsyncLoadRequest request = new AsyncLoadRequest
            {
                path = path,
                type = typeof(T),
                callback = (obj) => callback?.Invoke(obj as T)
            };

            asyncLoadQueue.Enqueue(request);
        }

        /// <summary>
        /// 异步加载资源（带进度回调）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">Resources路径</param>
        /// <param name="callback">加载完成回调</param>
        /// <param name="progressCallback">进度回调</param>
        public void LoadAsync<T>(string path, Action<T> callback, Action<float> progressCallback) where T : UnityEngine.Object
        {
            StartCoroutine(LoadAsyncCoroutine(path, typeof(T), 
                (obj) => callback?.Invoke(obj as T), 
                progressCallback));
        }

        #endregion

        #region 批量加载方法

        /// <summary>
        /// 批量加载同类型资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="folderPath">Resources文件夹下的目录路径</param>
        /// <returns>加载的资源数组</returns>
        public T[] LoadAll<T>(string folderPath) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning("ResourceLoader: 文件夹路径为空");
                return Array.Empty<T>();
            }

            T[] resources = Resources.LoadAll<T>(folderPath);
            Debug.Log($"ResourceLoader: 批量加载 {resources.Length} 个资源 - {folderPath}");

            // 添加到缓存
            if (enableCache)
            {
                foreach (var resource in resources)
                {
                    string fullPath = $"{folderPath}/{resource.name}";
                    AddToCache(fullPath, resource);
                }
            }

            return resources;
        }

        /// <summary>
        /// 批量异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="paths">资源路径数组</param>
        /// <param name="callback">全部加载完成回调</param>
        /// <param name="progressCallback">进度回调（0-1）</param>
        public void LoadAllAsync<T>(string[] paths, Action<T[]> callback, Action<float> progressCallback = null) where T : UnityEngine.Object
        {
            StartCoroutine(LoadAllAsyncCoroutine(paths, callback, progressCallback));
        }

        #endregion

        #region 预加载方法

        /// <summary>
        /// 预加载资源
        /// </summary>
        /// <param name="paths">需要预加载的资源路径数组</param>
        /// <param name="progressCallback">进度回调</param>
        /// <param name="completedCallback">完成回调</param>
        public void Preload(string[] paths, Action<float> progressCallback = null, Action completedCallback = null)
        {
            if (isPreloading)
            {
                Debug.LogWarning("ResourceLoader: 预加载正在进行中");
                return;
            }

            StartCoroutine(PreloadCoroutine(paths, progressCallback, completedCallback));
        }

        /// <summary>
        /// 预加载指定目录下的所有资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="folderPath">目录路径</param>
        /// <param name="progressCallback">进度回调</param>
        /// <param name="completedCallback">完成回调</param>
        public void PreloadFolder<T>(string folderPath, Action<float> progressCallback = null, Action completedCallback = null) where T : UnityEngine.Object
        {
            T[] resources = Resources.LoadAll<T>(folderPath);
            string[] paths = new string[resources.Length];
            
            for (int i = 0; i < resources.Length; i++)
            {
                paths[i] = $"{folderPath}/{resources[i].name}";
            }

            Preload(paths, progressCallback, completedCallback);
        }

        #endregion

        #region 卸载方法

        /// <summary>
        /// 卸载单个资源
        /// </summary>
        /// <param name="path">资源路径</param>
        public void Unload(string path)
        {
            if (resourceCache.ContainsKey(path))
            {
                resourceCache.Remove(path);
                cacheTimestamps.Remove(path);
                cacheAccessCount.Remove(path);
                Debug.Log($"ResourceLoader: 卸载资源 - {path}");
            }
        }

        /// <summary>
        /// 卸载所有缓存的资源
        /// </summary>
        public void UnloadAll()
        {
            resourceCache.Clear();
            cacheTimestamps.Clear();
            cacheAccessCount.Clear();
            Resources.UnloadUnusedAssets();
            Debug.Log("ResourceLoader: 已卸载所有缓存资源");
        }

        /// <summary>
        /// 清理过期缓存
        /// </summary>
        public void ClearExpiredCache()
        {
            float currentTime = Time.time;
            List<string> keysToRemove = new List<string>();

            foreach (var kvp in cacheTimestamps)
            {
                if (currentTime - kvp.Value > cacheExpirationTime)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                resourceCache.Remove(key);
                cacheTimestamps.Remove(key);
                cacheAccessCount.Remove(key);
            }

            if (keysToRemove.Count > 0)
            {
                Debug.Log($"ResourceLoader: 清理了 {keysToRemove.Count} 个过期缓存");
            }
        }

        #endregion

        #region 查询方法

        /// <summary>
        /// 检查资源是否存在
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>是否存在</returns>
        public bool Exists(string path)
        {
            if (enableCache && resourceCache.ContainsKey(path))
                return true;

            return Resources.Load(path) != null;
        }

        /// <summary>
        /// 获取缓存使用情况
        /// </summary>
        public CacheInfo GetCacheInfo()
        {
            return new CacheInfo
            {
                cachedCount = resourceCache.Count,
                maxCacheCount = maxCacheCount,
                oldestCacheAge = cacheTimestamps.Count > 0 ? Time.time - Mathf.Min(cacheTimestamps.Values) : 0,
                mostAccessedCache = GetMostAccessedCache()
            };
        }

        /// <summary>
        /// 获取已缓存的资源数量
        /// </summary>
        public int CachedCount => resourceCache.Count;

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

        private void Update()
        {
            // 处理异步加载队列
            ProcessAsyncLoadQueue();
            
            // 定期清理过期缓存
            if (enableCache && Random.value < 0.01f) // 大约每帧1%概率检查
            {
                ClearExpiredCache();
            }
        }

        private void ProcessAsyncLoadQueue()
        {
            // 移除已完成的异步加载
            activeAsyncLoads.RemoveAll(request => request.isDone);

            // 启动新的异步加载
            while (activeAsyncLoads.Count < MaxConcurrentAsyncLoads && asyncLoadQueue.Count > 0)
            {
                AsyncLoadRequest request = asyncLoadQueue.Dequeue();
                StartCoroutine(ProcessAsyncRequest(request));
                activeAsyncLoads.Add(request);
            }
        }

        private IEnumerator ProcessAsyncRequest(AsyncLoadRequest request)
        {
            request.operation = Resources.LoadAsync(request.path, request.type);
            
            while (!request.operation.isDone)
            {
                request.progress = request.operation.progress;
                request.progressCallback?.Invoke(request.progress);
                yield return null;
            }

            request.isDone = true;
            request.progress = 1f;

            if (request.operation.asset != null)
            {
                AddToCache(request.path, request.operation.asset);
                request.callback?.Invoke(request.operation.asset);
            }
            else
            {
                Debug.LogWarning($"ResourceLoader: 异步加载失败 - {request.path}");
                request.callback?.Invoke(null);
            }
        }

        private IEnumerator LoadAsyncCoroutine(string path, Type type, Action<UnityEngine.Object> callback, Action<float> progressCallback)
        {
            var operation = Resources.LoadAsync(path, type);
            
            while (!operation.isDone)
            {
                progressCallback?.Invoke(operation.progress);
                yield return null;
            }

            progressCallback?.Invoke(1f);

            if (operation.asset != null)
            {
                AddToCache(path, operation.asset);
                callback?.Invoke(operation.asset);
            }
            else
            {
                Debug.LogWarning($"ResourceLoader: 异步加载失败 - {path}");
                callback?.Invoke(null);
            }
        }

        private IEnumerator LoadAllAsyncCoroutine<T>(string[] paths, Action<T[]> callback, Action<float> progressCallback) where T : UnityEngine.Object
        {
            T[] results = new T[paths.Length];
            int completed = 0;

            for (int i = 0; i < paths.Length; i++)
            {
                int index = i;
                var operation = Resources.LoadAsync<T>(paths[i]);
                
                while (!operation.isDone)
                {
                    yield return null;
                }

                results[index] = operation.asset as T;
                completed++;
                progressCallback?.Invoke((float)completed / paths.Length);
            }

            callback?.Invoke(results);
        }

        private IEnumerator PreloadCoroutine(string[] paths, Action<float> progressCallback, Action completedCallback)
        {
            isPreloading = true;
            int completed = 0;

            foreach (var path in paths)
            {
                var operation = Resources.LoadAsync(path);
                
                while (!operation.isDone)
                {
                    yield return null;
                }

                if (operation.asset != null)
                {
                    AddToCache(path, operation.asset);
                }

                completed++;
                progressCallback?.Invoke((float)completed / paths.Length);
            }

            isPreloading = false;
            completedCallback?.Invoke();
        }

        private void AddToCache(string path, UnityEngine.Object resource)
        {
            if (!enableCache) return;

            // 检查缓存是否已满
            if (resourceCache.Count >= maxCacheCount)
            {
                RemoveLeastUsedCache();
            }

            resourceCache[path] = resource;
            cacheTimestamps[path] = Time.time;
            cacheAccessCount[path] = 0;
        }

        private void UpdateCacheAccess(string path)
        {
            if (cacheAccessCount.ContainsKey(path))
            {
                cacheAccessCount[path]++;
                cacheTimestamps[path] = Time.time; // 更新访问时间
            }
        }

        private void RemoveLeastUsedCache()
        {
            string leastUsedKey = null;
            int minAccess = int.MaxValue;

            foreach (var kvp in cacheAccessCount)
            {
                if (kvp.Value < minAccess)
                {
                    minAccess = kvp.Value;
                    leastUsedKey = kvp.Key;
                }
            }

            if (leastUsedKey != null)
            {
                resourceCache.Remove(leastUsedKey);
                cacheTimestamps.Remove(leastUsedKey);
                cacheAccessCount.Remove(leastUsedKey);
            }
        }

        private string GetMostAccessedCache()
        {
            string mostAccessed = null;
            int maxAccess = 0;

            foreach (var kvp in cacheAccessCount)
            {
                if (kvp.Value > maxAccess)
                {
                    maxAccess = kvp.Value;
                    mostAccessed = kvp.Key;
                }
            }

            return mostAccessed;
        }

        #endregion

        #region 内部类

        /// <summary>
        /// 异步加载请求
        /// </summary>
        private class AsyncLoadRequest
        {
            public string path;
            public Type type;
            public Action<UnityEngine.Object> callback;
            public Action<float> progressCallback;
            public ResourceRequest operation;
            public float progress;
            public bool isDone;
        }

        /// <summary>
        /// 缓存信息
        /// </summary>
        public struct CacheInfo
        {
            public int cachedCount;
            public int maxCacheCount;
            public float oldestCacheAge;
            public string mostAccessedCache;
        }

        #endregion
    }
}
