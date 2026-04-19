// UIManager.cs - UI管理器扩展
// 在原有UIManager基础上添加面板栈管理和扩展功能

using UnityEngine;
using System;
using System.Collections.Generic;

namespace JRPG.UI
{
    /// <summary>
    /// 面板信息
    /// </summary>
    [Serializable]
    public class PanelInfo
    {
        public string panelName;
        public BasePanel panel;
        public PanelLayer layer;
        public bool isCached;

        public PanelInfo(string name, BasePanel panel, PanelLayer layer)
        {
            this.panelName = name;
            this.panel = panel;
            this.layer = layer;
            this.isCached = false;
        }
    }

    /// <summary>
    /// UI层级配置
    /// </summary>
    [Serializable]
    public class UILayerConfig
    {
        public PanelLayer layer;
        public Transform parent;
        public int sortingOrder;
    }

    /// <summary>
    /// UI管理器扩展类
    /// 提供面板栈管理、层级管理等功能
    /// </summary>
    public static class UIManagerExtensions
    {
        #region Panel Stack Management

        /// <summary>
        /// 面板栈信息结构
        /// </summary>
        public class PanelStackInfo
        {
            public BasePanel Panel { get; set; }
            public PanelLayer Layer { get; set; }
            public int StackIndex { get; set; }
        }

        #endregion

        #region Event Definitions

        /// <summary>
        /// 面板事件参数
        /// </summary>
        public class PanelEventArgs : EventArgs
        {
            public BasePanel Panel { get; set; }
            public string PanelName { get; set; }
            public PanelLayer Layer { get; set; }

            public PanelEventArgs(BasePanel panel)
            {
                Panel = panel;
                PanelName = panel != null ? panel.PanelName : string.Empty;
                Layer = panel != null ? panel.Layer : PanelLayer.Normal;
            }
        }

        #endregion
    }

    /// <summary>
    /// UI配置数据
    /// </summary>
    [CreateAssetMenu(fileName = "UIConfig", menuName = "JRPG/UI/UIConfig")]
    public class UIConfig : ScriptableObject
    {
        [Header("层级配置")]
        public List<UILayerConfig> layerConfigs = new List<UILayerConfig>();

        [Header("预加载配置")]
        public List<string> preloadPanels = new List<string>();

        [Header("通用配置")]
        public float defaultAnimDuration = 0.3f;
        public bool enableSafeArea = true;
        public bool enableNotchAdaptation = true;
    }

    /// <summary>
    /// UI事件中心
    /// 集中管理所有UI相关事件
    /// </summary>
    public class UIEventCenter
    {
        private static UIEventCenter _instance;
        public static UIEventCenter Instance => _instance ??= new UIEventCenter();

        // 事件定义
        public event Action<string> OnPanelOpen;        // 面板打开
        public event Action<string> OnPanelClose;        // 面板关闭
        public event Action<string, object> OnPanelData; // 面板数据更新
        public event Action OnMainMenuShow;             // 显示主菜单
        public event Action OnBattleStart;              // 战斗开始
        public event Action OnBattleEnd;                // 战斗结束
        public event Action<int> OnNotification;        // 通知事件

        // 发布事件方法
        public void PublishPanelOpen(string panelName)
        {
            OnPanelOpen?.Invoke(panelName);
        }

        public void PublishPanelClose(string panelName)
        {
            OnPanelClose?.Invoke(panelName);
        }

        public void PublishPanelData(string panelName, object data)
        {
            OnPanelData?.Invoke(panelName, data);
        }

        public void PublishMainMenuShow()
        {
            OnMainMenuShow?.Invoke();
        }

        public void PublishBattleStart()
        {
            OnBattleStart?.Invoke();
        }

        public void PublishBattleEnd()
        {
            OnBattleEnd?.Invoke();
        }

        public void PublishNotification(int notificationId)
        {
            OnNotification?.Invoke(notificationId);
        }
    }

    /// <summary>
    /// UI命令类型
    /// </summary>
    public enum UICommandType
    {
        ShowPanel,
        HidePanel,
        TogglePanel,
        RefreshPanel,
        SetPanelData,
        ClearAllPanels,
        PopAllAndShow
    }

    /// <summary>
    /// UI命令结构
    /// </summary>
    [Serializable]
    public class UICommand
    {
        public UICommandType type;
        public string panelName;
        public object data;
        public Action callback;

        public UICommand(UICommandType type, string panelName = null, object data = null, Action callback = null)
        {
            this.type = type;
            this.panelName = panelName;
            this.data = data;
            this.callback = callback;
        }
    }
}
