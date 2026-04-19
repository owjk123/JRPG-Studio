// UIManager.cs - UI管理器
// 负责所有界面的管理、显示、隐藏、动画

using UnityEngine;
using System.Collections.Generic;

namespace JRPG.UI
{
    /// <summary>
    /// UI管理器 - 负责所有界面的管理
    /// </summary>
    public class UIManager : MonoBehaviour, ISystem
    {
        #region Singleton
        
        public static UIManager Instance { get; private set; }
        
        #endregion
        
        #region Fields
        
        [Header("UI References")]
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private Transform _uiRoot;
        
        // UI栈
        private Stack<BaseUI> _uiStack = new Stack<BaseUI>();
        
        // UI缓存池
        private Dictionary<string, BaseUI> _uiCache = new Dictionary<string, BaseUI>();
        
        // 当前显示的UI
        private BaseUI _currentUI;
        
        #endregion
        
        #region Events
        
        public event System.Action<BaseUI> OnUIOpened;
        public event System.Action<BaseUI> OnUIClosed;
        
        #endregion
        
        #region Properties
        
        public BaseUI CurrentUI => _currentUI;
        public int StackCount => _uiStack.Count;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        #endregion
        
        #region ISystem Implementation
        
        public void Initialize()
        {
            // 预加载常用UI
            PreloadCommonUIs();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 打开UI
        /// </summary>
        public T OpenUI<T>(string uiPath = null) where T : BaseUI
        {
            string uiName = typeof(T).Name;
            
            // 检查缓存
            if (!_uiCache.TryGetValue(uiName, out var ui))
            {
                // 从资源加载
                ui = LoadUI<T>(uiPath ?? $"UI/{uiName}");
                if (ui == null)
                {
                    Debug.LogError($"[UIManager] 无法加载UI: {uiName}");
                    return null;
                }
                
                _uiCache[uiName] = ui;
            }
            
            // 如果当前有UI，暂停它
            if (_currentUI != null && _currentUI != ui)
            {
                _currentUI.OnPause();
                _uiStack.Push(_currentUI);
            }
            
            // 打开新UI
            _currentUI = ui;
            ui.transform.SetParent(_uiRoot);
            ui.gameObject.SetActive(true);
            ui.OnOpen();
            
            OnUIOpened?.Invoke(ui);
            
            return ui as T;
        }
        
        /// <summary>
        /// 关闭当前UI
        /// </summary>
        public void CloseCurrentUI()
        {
            if (_currentUI == null) return;
            
            var ui = _currentUI;
            ui.OnClose();
            ui.gameObject.SetActive(false);
            
            OnUIClosed?.Invoke(ui);
            
            // 恢复上一个UI
            if (_uiStack.Count > 0)
            {
                _currentUI = _uiStack.Pop();
                _currentUI.OnResume();
                _currentUI.gameObject.SetActive(true);
            }
            else
            {
                _currentUI = null;
            }
        }
        
        /// <summary>
        /// 关闭所有UI
        /// </summary>
        public void CloseAllUI()
        {
            while (_uiStack.Count > 0)
            {
                var ui = _uiStack.Pop();
                ui.OnClose();
                ui.gameObject.SetActive(false);
            }
            
            if (_currentUI != null)
            {
                _currentUI.OnClose();
                _currentUI.gameObject.SetActive(false);
                _currentUI = null;
            }
        }
        
        /// <summary>
        /// 获取UI
        /// </summary>
        public T GetUI<T>() where T : BaseUI
        {
            string uiName = typeof(T).Name;
            if (_uiCache.TryGetValue(uiName, out var ui))
            {
                return ui as T;
            }
            return null;
        }
        
        /// <summary>
        /// 显示提示信息
        /// </summary>
        public void ShowToast(string message, float duration = 2f)
        {
            // TODO: 实现Toast UI
            Debug.Log($"[Toast] {message}");
        }
        
        /// <summary>
        /// 显示确认对话框
        /// </summary>
        public void ShowConfirm(string title, string message, System.Action onConfirm, System.Action onCancel = null)
        {
            // TODO: 实现确认对话框
            Debug.Log($"[Confirm] {title}: {message}");
        }
        
        #endregion
        
        #region Private Methods
        
        private void PreloadCommonUIs()
        {
            // 预加载常用UI
            // 例如: 主菜单、战斗UI、背包等
        }
        
        private T LoadUI<T>(string path) where T : BaseUI
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] 无法加载UI预制体: {path}");
                return null;
            }
            
            var go = Instantiate(prefab);
            return go.GetComponent<T>();
        }
        
        #endregion
    }
    
    /// <summary>
    /// UI基类
    /// </summary>
    public abstract class BaseUI : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] protected bool _destroyOnClose = false;
        [SerializeField] protected bool _pauseGameOnOpen = false;
        
        /// <summary>
        /// 打开时调用
        /// </summary>
        public virtual void OnOpen()
        {
            if (_pauseGameOnOpen)
            {
                Time.timeScale = 0f;
            }
        }
        
        /// <summary>
        /// 关闭时调用
        /// </summary>
        public virtual void OnClose()
        {
            if (_pauseGameOnOpen)
            {
                Time.timeScale = 1f;
            }
            
            if (_destroyOnClose)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 暂停时调用（被其他UI覆盖）
        /// </summary>
        public virtual void OnPause()
        {
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 恢复时调用（返回此UI）
        /// </summary>
        public virtual void OnResume()
        {
            gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// 系统接口
    /// </summary>
    public interface ISystem
    {
        void Initialize();
    }
}
